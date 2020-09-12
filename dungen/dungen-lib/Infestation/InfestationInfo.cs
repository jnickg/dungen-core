using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Infestation
{
  /// <summary>
  /// First-class type of an infestation. This is to be used by an Infestation algorithm, it that
  /// algorithm prefers to organize how infestations are added to a dungeon. These are basically
  /// labels that are common/significant enough that they deserve special treatment by infestation
  /// algorthms.
  /// </summary>
  public enum InfestationType
  {
    /// <summary>
    /// No type assigned. Labels should be used for taxonomically organizing this infestation.
    /// </summary>
    None,

    /// <summary>
    /// An item that players can interact with, take, etc.
    /// </summary>
    Item,

    /// <summary>
    /// An actor that players may have to fight, interact with, avoid, etc.
    /// </summary>
    Actor,

    /// <summary>
    /// A hazard, such as a puzzle or trap, that players must circumvent or overcome.
    /// </summary>
    Hazard
  }
  
  /// <summary>
  /// 
  /// </summary>
  public enum AssociationType
  {
    /// <summary>
    /// 
    /// </summary>
    General,

    /// <summary>
    /// 
    /// </summary>
    PartOf,

    /// <summary>
    /// 
    /// </summary>
    Expands,

    /// <summary>
    /// 
    /// </summary>
    SimilarTo
  }

  /// <summary>
  /// Base implementation that all Infestation instances can inherit from
  /// </summary>
  [DataContract(Name = "infestation", IsReference = true)]
  [KnownType(nameof(GetKnownTypes))]
  public class InfestationInfo : IInfestation
  {
    /// <see cref="IInfestation.Parent"/>
    [DataMember(Name = "parent", Order = 0)]
    public Library Parent { get; set; } = null;

    /// <see cref="IInfestation.Name"/>
    [DataMember(Name = "name", Order = 1)]
    public string Name { get; set; } = string.Empty;

    /// <see cref="IInfestation.Brief"/>
    [DataMember(Name = "brief", Order = 2)]
    public string Brief { get; set; } = string.Empty;

    /// <see cref="IInfestation.Overview"/>
    [DataMember(Name = "overview", Order = 3)]
    public string Overview { get; set; } = string.Empty;

    /// <see cref="IInfestation.URI"/>
    [DataMember(Name = "uri", Order = 4)]
    public string URI { get; set; } = string.Empty;

    /// <see cref="IInfestation.Size"/>
    [DataMember(Name = "size", Order = 5)]
    public int Size { get; set; } = 1;

    /// <see cref="IInfestation.OccurrenceFactor"/>
    [DataMember(Name = "occurrence_factor", Order = 6)]
    public double OccurrenceFactor { get; set; } = 1.0;

    /// <see cref="IInfestation.Category"/>
    [DataMember(Name = "category", Order = 7)]
    public InfestationType Category { get; set; } = InfestationType.None;

    /// <see cref="IInfestation.Labels"/>
    [DataMember(Name = "labels", Order = 8)]
    public IDictionary<Label, double> Labels { get; set; } = new Dictionary<Label, double>();

    /// <see cref="Library.GetKnownTypes"/>
    public static IEnumerable<Type> GetKnownTypes()
    {
      return Library.GetKnownTypes();
    }

    /// <see cref="object.ToString"/>
    public override string ToString()
    {
      return String.Format("{0} ({1}, {2}) - {3}", Name, Category, Size, Brief);
    }
  }
}
