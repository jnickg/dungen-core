using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Infestation
{
  /// <summary>
  /// 
  /// </summary>
  [DataContract(Name = "label", IsReference = true)]
  public class Label
  {
    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "name", Order = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "parent", Order = 2)]
    public Library Parent { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "description", Order = 3)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "uri", Order = 4)]
    public string URI { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "associations", Order = 5)]
    public IDictionary<Label, Tuple<AssociationType, double>> Associations { get; set; } = new Dictionary<Label, Tuple<AssociationType, double>>();

    /// <see cref="object.ToString"/>
    public override string ToString()
    {
      return String.Format("{0} ({1} Associations)", Name, Associations.Count);
    }
  }
}
