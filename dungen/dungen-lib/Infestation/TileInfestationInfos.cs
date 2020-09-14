using System.Collections.Generic;
using System.Runtime.Serialization;
using DunGen.Tiles;

namespace DunGen.Infestation
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "tileInfestations", KeyName = "tile", ItemName = "infestations", IsReference = true)]
  public class TileInfestationInfos : Dictionary<Tile, InfestationInfoList>
  {
    public void SafeAdd(Tile k, InfestationInfo newElement)
    {
      if (k == null) return;
      if (newElement == null) return;

      InfestationInfoList valueCollection = null;
      if (!TryGetValue(k, out valueCollection))
      {
        Add(k, new InfestationInfoList());
      }

      this[k].Add(newElement);
    }
  }
}
