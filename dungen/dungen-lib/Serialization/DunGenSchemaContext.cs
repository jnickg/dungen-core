using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DunGen.Serialization
{
  /// <summary>
  /// 
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
}
