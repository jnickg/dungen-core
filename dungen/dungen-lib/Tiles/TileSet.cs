using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace DunGen.Tiles
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "tileSet", ItemName = "tile", IsReference = true)]
  public class TileSet : HashSet<Tile>
  {
    public TileSet() : base() { }
    public TileSet(IEnumerable<Tile> tiles) : base(tiles) { }

    /// <summary>
    /// Returns whether this set of Tiles contains the given location.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Contains(int x, int y)
    {
      return this.Any(t => t.Location.X == x && t.Location.Y == y);
    }

    /// <summary>
    /// Returns whether this set of Tiles contains the given location.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool Contains(Point location)
    {
      return Contains(location.X, location.Y);
    }
  }
}
