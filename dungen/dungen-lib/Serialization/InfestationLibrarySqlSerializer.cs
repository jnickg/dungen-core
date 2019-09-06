using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.ComponentModel.DataAnnotations;
using DunGen.Infestation;

namespace DunGen.Serialization
{
  /// <summary>
  /// TODO
  /// </summary>
  internal class DunGenSchemaContext : DbContext
  {
    #region Nested Types
    internal class LabelEntity
    {
      [Key]
      public string label_name { get; set; }
      public int lib_id { get; set; }
      public string description { get; set; }
      public string uri { get; set; }
    }

    internal class LabelAssociationEntity
    {
      [Key]
      public int assoc_id { get; set; }
      public string label_name { get; set; }
      public string related_label_name { get; set; }
      public string type { get; set; }
      public string description { get; set; }
      public decimal associativity { get; set; }
    }

    internal class InfestationLabelEntity
    {
      [Key]
      public int infestlbl_id { get; set; }
      public int infest_id { get; set; }
      public string label_name { get; set; }
      public decimal associativity { get; set; }
    }

    internal class InfestationEntity
    {
      [Key]
      public int infest_id { get; set; }
      public int lib_id { get; set; }
      public string category { get; set; }
      public string name { get; set; }
      public string brief { get; set; }
      public string description { get; set; }
      public decimal occurrence_factor { get; set; }
      public int size { get; set; }
      public string uri { get; set; }
    }

    internal class LibraryEntity
    {
      [Key]
      public int lib_id { get; set; }
      public string name { get; set; }
      public string brief { get; set; }
      public string uri { get; set; }
    }
    #endregion
    public string CxnString { get; set; } = string.Empty;

    public DbSet<LibraryEntity> LIBRARY_DATA { get; set; }
    public DbSet<InfestationEntity> INFESTATION_DATA { get; set; }
    public DbSet<LabelEntity> LABEL_DATA { get; set; }
    public DbSet<LabelAssociationEntity> LABELASSOCIATION_DATA { get; set; }
    public DbSet<InfestationLabelEntity> INFESTLABEL_DATA { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlServer(this.CxnString);
    }
  }

  /// <summary>
  /// TODO
  /// </summary>
  public class InfestationLibrarySqlSerializer
  {
    /// <summary>
    /// 
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionString"></param>
    public InfestationLibrarySqlSerializer(string connectionString)
    {
      ConnectionString = connectionString;
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Library GetLibrary(int id)
    {
      DunGenSchemaContext.LibraryEntity library = null;
      IEnumerable<DunGenSchemaContext.LabelEntity> labels = null;
      IEnumerable<DunGenSchemaContext.InfestationEntity> infestations = null;
      IEnumerable<DunGenSchemaContext.InfestationLabelEntity> infestationLabels = null;
      IEnumerable<DunGenSchemaContext.LabelAssociationEntity> labelAssociations = null;

      using (var context = new DunGenSchemaContext() { CxnString = ConnectionString })
      {
        // Getting the library is implicitly "loaded" with the call to Single(). The others can
        // see some funky Linq/DQ related lazy loading, which leads to disposed-object errors, so
        // we call ToList() at the end of each one to force them to actually load.

        library = context.LIBRARY_DATA.Single(lib => lib.lib_id == id);

        // Don't bother with other queries if no such library exists.
        if (null == library)
        {
          return null;
        }

        labels = context.LABEL_DATA.Where(label => label.lib_id == id).ToList();

        infestations = context.INFESTATION_DATA.Where(infestation => infestation.lib_id == id).ToList();

        infestationLabels = context.INFESTLABEL_DATA.Where(label =>
          infestations.Select(i => i.infest_id).Contains(label.infest_id) &&
          labels.Select(lbl => lbl.label_name).Contains(label.label_name)).ToList();

        labelAssociations = context.LABELASSOCIATION_DATA.Where(assoc =>
            labels.Select(lbl => lbl.label_name).Contains(assoc.label_name) &&
            labels.Select(lbl => lbl.label_name).Contains(assoc.related_label_name)).ToList();
      }

      // Instantiate a library now, so we can assign it as a parent to deserialized objects below.
      Library loadedLibrary = new Library();

      var loadedLabels = new List<Label>();
      loadedLabels.AddRange(labels.Select(lbl => new Label()
      {
        Parent = loadedLibrary,
        Name = lbl.label_name,
        Description = lbl.description,
        URI = lbl.uri,
        // Associations set later
      }).ToList());

      // Set associations for all labels
      foreach (var lbl in loadedLabels)
      {
        foreach (var assoc in labelAssociations.Where(a => a.label_name == lbl.Name))
        {
          var associatedLabel = loadedLabels.Single(l => l.Name == assoc.related_label_name);
          var associationData = new Tuple<AssociationType, double>(
            Enum.Parse<AssociationType>(assoc.type),
            (double)assoc.associativity);
          lbl.Associations.Add(associatedLabel, associationData);
        }
      }

      // Assign labels to their appropriate infestations
      var loadedInfestations = new List<InfestationInfo>();
      foreach (var infest in infestations)
      {
        var infestInfo = new InfestationInfo()
        {
          Parent = loadedLibrary,
          Name = infest.name,
          Brief = infest.brief,
          Overview = infest.description,
          URI = infest.uri,
          Category = Enum.Parse<InfestationType>(infest.category, true),
          OccurrenceFactor = (double)infest.occurrence_factor,
          Size = infest.size,
          // Labels are set below
        };

        foreach (var infestLbl in infestationLabels.Where(l => infest.infest_id == l.infest_id))
        {
          var theLabel = loadedLabels.Single(l => l.Name == infestLbl.label_name);
          // This will clobber earlier entries from the DB if more than one entry associates
          // infestation A with label L. So, below we assume the table has a constraint ensuring no
          // two rows pair A and L.
          infestInfo.Labels[theLabel] = (double)infestLbl.associativity;
        }

        loadedInfestations.Add(infestInfo);
      }

      // Assign deserialized information to the output object
      loadedLibrary.Name = library.name;
      loadedLibrary.URI = library.uri;
      loadedLibrary.Brief = library.brief;
      loadedLabels.ToList().ForEach(lbl => loadedLibrary.Labels.Add(lbl));
      loadedLibrary.AllInfestations.AddRange(loadedInfestations);

      return loadedLibrary;
    }
  }
}
