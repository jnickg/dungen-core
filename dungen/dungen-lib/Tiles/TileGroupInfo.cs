using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Tiles
{
  /// <summary>
  /// Stores information about a group of tiles
  /// </summary>
  [DataContract(Name = "tileGroup", IsReference = true)]
  [KnownType(typeof(Tile))]
  public class TileGroupInfo
  {
    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "parent", EmitDefaultValue = false, Order = 0)]
    public Dungeon Parent { get; set; } = null;
    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "category", Order = 1)]
    public TileCategory Category { get; set; } = TileCategory.Normal;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "tiles", Order = 2)]
    public TileSet Tiles { get; set; } = new TileSet();
  }
}
