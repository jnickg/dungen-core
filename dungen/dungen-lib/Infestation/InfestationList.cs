using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Infestation
{
  /// <summary>
  /// A shim class to assist with serialization. Stores a collection of
  /// <see cref="InfestationInfo"/> objects.
  /// </summary>
  [CollectionDataContract(Name = "infestationList", ItemName = "infestation")]
  [KnownType(nameof(GetKnownTypes))]
  public class InfestationList : List<InfestationInfo>
  {
    /// <see cref="Library.GetKnownTypes"/>
    public static IEnumerable<Type> GetKnownTypes()
    {
      return Library.GetKnownTypes();
    }

    /// <summary>
    /// 
    /// </summary>
    public InfestationList()
      : base()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection"></param>
    public InfestationList(IEnumerable<InfestationInfo> collection)
      : base(collection)
    {
    }
  }
}
