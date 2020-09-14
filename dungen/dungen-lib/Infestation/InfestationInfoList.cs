using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DunGen.Infestation
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "infestationInfoList", ItemName = "infestationInfo", IsReference = true)]
  public class InfestationInfoList : List<InfestationInfo>
  {
  }
}
