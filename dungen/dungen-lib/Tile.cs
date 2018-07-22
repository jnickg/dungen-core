using System;
using System.Drawing;

namespace DunGen
{
  public class Tile
  {
    #region Nested Types

    /// <summary>
    /// Enumeration of traversal physics for a Tile. If the value is "Wall"
    /// then the Tile is considered filled, and non-traversible. If the
    /// value starts with "Open_" the Tile is considered opened and
    /// traversible. For all "Open_" values, the suffix indicates which
    /// borders are opened for entry/exit. That is, if a Tile's MoveType
    /// value is "Open_WEST" then the tile is open and traversible, and
    /// may be entered/existed through the WEST border of the Tile. If
    /// TileA and TileB are next to each other, touching at their EAST
    /// and WEST borders respectively, in order to move from one tile to
    /// the other BOTH must have appropriate values: Open_EAST and Open_WEST
    /// respectively.
    /// </summary>
    public enum MoveType
    {
      Wall            = 0x00000000,
      Open_NORTH      = 0x00000001,
      Open_EAST       = 0x00000002,
      Open_SOUTH      = 0x00000004,
      Open_WEST       = 0x00000008,
      Open_HORIZ      = Open_NORTH | Open_EAST | Open_SOUTH | Open_WEST,
      Open_UP         = 0x00000010,
      Open_DOWN       = 0x00000020,
      Open_VERT       = Open_UP | Open_DOWN,
      Open_ALL        = Open_HORIZ | Open_VERT,
      Open_HEX_NW     = 0x00000040,
      Open_HEX_NE     = 0x00000080,
      Open_HEX_SE     = 0x00000100,
      Open_HEX_SW     = 0x00000200,
      Open_HEX_HORIZ  = Open_NORTH | Open_SOUTH | Open_HEX_NE | Open_HEX_NW | Open_HEX_SE | Open_HEX_SW,
    }

    #endregion

    #region Statics

    private static int TileId;

    static Tile()
    {
      TileId = 0;
    }

    #endregion

    #region Members

    public DungeonTiles Parent { get; internal set; }

    public bool IsOwned
    {
      get { return null != Parent; }
    }

    public Point Location
    {
      get
      {
        if (null == Parent) return new Point(-1, -1);
        return Parent.TilesById[this.Id];
      }
    }

    public int Id { get; private set; }

    public MoveType Physics { get; set; }

    public Tile()
    {
      this.Id = System.Threading.Interlocked.Increment(ref TileId);
      this.Physics = MoveType.Wall;
    }

    #endregion
  }

  public static partial class Extensions
  {
    /// <summary>
    /// Gets the opposite MoveType to the one specified. Can do most
    /// hex directionality, but will get confused with hex horiz vs
    /// non-hex horiz. Will have to create a new func later to handle
    /// that case (maybe with specified "preferHex" bool?)
    /// </summary>
    /// <param name="t">The MoveType for which to find the opposite</param>
    /// <returns>The opposite MoveType</returns>
    public static Tile.MoveType GetOpposite(this Tile.MoveType t)
    {
      switch (t)
      {
        case Tile.MoveType.Wall:
          return Tile.MoveType.Open_ALL;
        case Tile.MoveType.Open_NORTH:
          return Tile.MoveType.Open_SOUTH;
        case Tile.MoveType.Open_EAST:
          return Tile.MoveType.Open_WEST;
        case Tile.MoveType.Open_SOUTH:
          return Tile.MoveType.Open_NORTH;
        case Tile.MoveType.Open_WEST:
          return Tile.MoveType.Open_EAST;
        case Tile.MoveType.Open_HORIZ:
          return Tile.MoveType.Open_VERT;
        case Tile.MoveType.Open_UP:
          return Tile.MoveType.Open_DOWN;
        case Tile.MoveType.Open_DOWN:
          return Tile.MoveType.Open_UP;
        case Tile.MoveType.Open_VERT:
          return Tile.MoveType.Open_HORIZ;
        case Tile.MoveType.Open_ALL:
          return Tile.MoveType.Wall;
        case Tile.MoveType.Open_HEX_NW:
          return Tile.MoveType.Open_HEX_SE;
        case Tile.MoveType.Open_HEX_NE:
          return Tile.MoveType.Open_HEX_SW;
        case Tile.MoveType.Open_HEX_SE:
          return Tile.MoveType.Open_HEX_NW;
        case Tile.MoveType.Open_HEX_SW:
          return Tile.MoveType.Open_HEX_NE;
        case Tile.MoveType.Open_HEX_HORIZ:
          throw new NotSupportedException("Can't do that, sorry");
        default:
          throw new ArgumentException("Unknown Tile.MoveType");
      }
    }

    public static Tile.MoveType CloseOff(this Tile.MoveType t, Tile.MoveType direction)
    {
      return t & ~direction;
    }

    public static Tile.MoveType CloseOff(this Tile.MoveType t, int direction)
    {
      return t.CloseOff((Tile.MoveType)direction);
    }

    public static Tile.MoveType OpenUp(this Tile.MoveType t, Tile.MoveType direction)
    {
      return t | direction;
    }

    public static Tile.MoveType OpenUp(this Tile.MoveType t, int direction)
    {
      return t.OpenUp((Tile.MoveType)direction);
    }
  }
}
