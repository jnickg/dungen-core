using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DunGen.Generator
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "row", ItemName = "mask")]
  public class BoolList : List<bool> { }
}
