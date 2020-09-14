using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DunGen.Tiles
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "row", ItemName = "tileData", IsReference = true)]
  public class TileList : List<Tile> { }
}
