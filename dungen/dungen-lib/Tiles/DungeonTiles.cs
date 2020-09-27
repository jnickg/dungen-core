using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace DunGen.Tiles
{
  /// <summary>
  /// A collection of Tiles comprosing a Dungeon's physical layout. Can be
  /// categorized and grouped
  /// </summary>
  [DataContract(Name = "layout", IsReference = true)]
  public class DungeonTiles : ICloneable
  {
    #region Statics
    public static readonly int GroupId_AllTiles = 0;
    #endregion

    #region Private Members
    private bool[,] _defaultMask = null;

    private Tile[,] _tiles;

    /// <summary>
    /// Shim member tied to actual Tiles data, for the purposes of serialization.
    /// For performance purposes, users should not use this member directly. Instead,
    /// interact with the Tiles field.
    /// </summary>
    [DataMember(Name = "tiles", Order = 2)]
    private TileCollection _tiles_DataContract
    {
      get
      {
        return Tiles.Jaggedize_DC();
      }

      set
      {
        Tiles = value.UnJaggedize();
      }
    }

    private Tile[,] Tiles {
      get
      {
        return _tiles;
      }

      set
      {
        this._tiles = value;
        this.TilesById = new Dictionary<int, Point>();
        this.Tiles_Set = new TileSet();
        for (int y = 0; y < value.GetLength(0); ++y)
        {
          for (int x = 0; x < value.GetLength(1); ++x)
          {
            value[y, x].Parent = this;
            this.Tiles_Set.Add(value[y, x]);
            this.TilesById.Add(value[y, x].Id, new Point(x, y));
          }
        }
      }
    }
    #endregion

    #region Properties
    [DataMember(Name = "parent", Order = 0)]
    public Dungeon Parent { get; internal set; }

    public TileSet Tiles_Set { get; set; }

    public IDictionary<int, Point> TilesById { get; set; }

    [DataMember(Name = "hex", Order = 1)]
    public bool IsHex { get; private set; }

    public Tile this[int y, int x]
    {
      get
      {
        return Tiles[y, x];
      }
      set
      {
        Tiles[y, x] = value;
      }
    }

    /// <summary>
    /// Default all-true mask for this set of DungeonTiles.
    /// </summary>
    public bool[,] DefaultMask
    {
      get
      {
        if (null == _defaultMask ||
          this._defaultMask.GetLength(0) != this.Height ||
          this._defaultMask.GetLength(1) != this.Width)
        {
          _defaultMask = new bool[this.Height, this.Width];

          for (int y = 0; y < _defaultMask.GetLength(0); ++y)
          {
            for (int x = 0; x < _defaultMask.GetLength(1); ++x)
            {
              _defaultMask[y, x] = true;
            }
          }
        }

        return _defaultMask;
      }
    }

    /// <summary>
    /// A [READONLY] collection of groups of Tiles, contained by this object.
    /// </summary>
    public IList<ISet<Tile>> Groups
    {
      get
      {
        if (null == Parent) return new List<ISet<Tile>>();

        return Parent.Groups.Select(g => (ISet<Tile>)g.Tiles).ToList();
      }
    }

    public int Width
    {
      get
      {
        if (Tiles == null) return 0;
        return Tiles.GetLength(1);
      }
    }

    public int Height
    {
      get
      {
        if (Tiles == null) return 0;
        return Tiles.GetLength(0);
      }
    }
    #endregion

    #region Members
    public Tile GetRandomTile(Random r = null)
    {
      if (null == r) r = new Random();
      return this[r.Next(this.Height), r.Next(this.Width)];
    }

    public Tile GetRandomTile(bool[,] mask, Random r = null)
    {
      if (null == r) r = new Random();
      if (null == mask) throw new ArgumentNullException();
      List<Tile> tilePool = new List<Tile>(this.GetTilesIn(mask));
      return tilePool[r.Next(tilePool.Count)];
    }

    public DungeonTiles()
    {
      ResetTiles(1, 1);
    }

    public DungeonTiles(int width, int height, Tile.MoveType startingPhyics = Tile.MoveType.Wall)
    {
      ResetTiles(width, height, startingPhyics);
    }

    public void ResetTiles(int width, int height, Tile.MoveType startingPhyics = Tile.MoveType.Wall)
    {
      Tile[,] tiles = new Tile[height, width];
      for (int y = 0; y < height; ++y)
      {
        for (int x = 0; x < width; ++x)
        {
          tiles[y, x] = new Tile()
          {
            Parent = this,
            Physics = startingPhyics
          };
        }
      }
      this.Tiles = tiles;
      this.IsHex = false; // TODO
    }

    public ISet<Tile> GetTilesIn(bool[,] mask)
    {
      if (null == mask) return new HashSet<Tile>();

      if (mask.GetLength(0) != this.Tiles.GetLength(0)
       || mask.GetLength(1) != this.Tiles.GetLength(1))
      {
        throw new ArgumentException("Invalid mask");
      }

      ISet<Tile> tiles = new HashSet<Tile>();

      for (int y = 0; y < this.Height; ++y)
      {
        for (int x = 0; x < this.Width; ++x)
        {
          if (mask[y, x]) tiles.Add(this[y, x]);
        }
      }

      return tiles;
    }

    public ISet<Tile> GetTilesIn(Rectangle region)
    {
      if (null == region) return new HashSet<Tile>();
      if (region.X + region.Width > this.Width ||
          region.Y + region.Height > this.Height ||
          region.X < 0 || region.Y < 0)
      {
        throw new ArgumentException("Invalid region");
      }

      ISet<Tile> tiles = new HashSet<Tile>();

      int endY = region.Y + region.Height;
      int endX = region.X + region.Width;
      for (int y = region.Y; y < endY; ++y)
      {
        for (int x = region.X; x < endX; ++x)
        {
          tiles.Add(this[y, x]);
        }
      }

      return tiles;
    }

    public void SetAllToo(Tile.MoveType physics, bool[,] mask = null)
    {
      for (int y = 0; y < this.Height; ++y)
      {
        for(int x = 0; x < this.Width; ++x)
        {
          if (null == mask || mask[y, x])
          {
            this[y, x].Physics = physics;
          }
        }
      }
    }

    public void CarveBetween(Tile t1, Tile t2)
    {
      CarveBetween(t1.Location, t2.Location);
    }

    /// <summary>
    /// Carves openings from/to the specified Points
    /// </summary>
    public void CarveBetween(Point p1, Point p2)
    {
      if (this.IsHex) throw new NotImplementedException();
    }

    public Tile.MoveType GetCardinality(Tile from, Tile to)
    {
      return GetCardinality(from.Location, to.Location);
    }

    /// <summary>
    /// If adjacent, returns the cardinal direction from/to the specified Points.
    /// If not adjacent, gives closest approximation of cardinality.
    /// </summary>
    public Tile.MoveType GetCardinality(Point from, Point to)
    {
      if (this.IsHex) throw new NotImplementedException();

      int dx = to.X - from.X;
      int dy = to.Y - from.Y;

      if (Math.Abs(dx) > Math.Abs(dy)) return (dx > 0) ? Tile.MoveType.Open_EAST : Tile.MoveType.Open_WEST;
      else return (dy > 0) ? Tile.MoveType.Open_SOUTH: Tile.MoveType.Open_NORTH;
    }

    public bool TileIsValid(int x, int y)
    {
      if (x < 0 || y < 0 || x >= this.Width || y >= this.Height) return false;
      else return true;
    }

    public int GetAdjacentOpensFor(int x, int y)
    {
      if (!TileIsValid(x, y)) return 0;
      int adjacentEmpties = 0;

      if (TileIsValid(x + 1, y) &&
        0 != (this[y, x + 1].Physics & Tile.MoveType.Open_WEST))
      {
        ++adjacentEmpties;
      }
      if (TileIsValid(x, y + 1) &&
        0 != (this[y + 1, x].Physics & Tile.MoveType.Open_NORTH))
      {
        ++adjacentEmpties;
      }
      if (TileIsValid(x - 1, y) &&
        0 != (this[y, x - 1].Physics & Tile.MoveType.Open_EAST))
      {
        ++adjacentEmpties;
      }
      if (TileIsValid(x, y - 1) &&
        0 != (this[y - 1, x].Physics & Tile.MoveType.Open_SOUTH))
      {
        ++adjacentEmpties;
      }
      return adjacentEmpties;
    }

    /// <summary>
    /// Checks if a wall exists anywhere that prevents movement from the specified tile
    /// to the adjacent tile in the specified direction (including walls in that tile
    /// which would prevent movement). If the movement direction is not possible due to
    /// being at the edge of a map, returns true. If the specified tile is invalid,
    /// returns true.
    /// </summary>
    public bool WallExists(int x, int y, Tile.MoveType direction)
    {
      if (!TileIsValid(x, y)) return true;
      if (0 == (this[y, x].Physics & direction)) return true;
      switch (direction)
      {
        case Tile.MoveType.Wall:
          throw new ArgumentException();
        case Tile.MoveType.Open_NORTH:
          return (!TileIsValid(x, y - 1) || (this[y - 1, x].Physics & direction.GetOpposite()) == 0);
        case Tile.MoveType.Open_EAST:
          return (!TileIsValid(x + 1, y) || (this[y, x + 1].Physics & direction.GetOpposite()) == 0);
        case Tile.MoveType.Open_SOUTH:
          return (!TileIsValid(x, y + 1) || (this[y + 1, x].Physics & direction.GetOpposite()) == 0);
        case Tile.MoveType.Open_WEST:
          return (!TileIsValid(x - 1, y) || (this[y, x - 1].Physics & direction.GetOpposite()) == 0);
        case Tile.MoveType.Open_HORIZ:
          return WallExists(x, y, Tile.MoveType.Open_NORTH) &&
                 WallExists(x, y, Tile.MoveType.Open_EAST) &&
                 WallExists(x, y, Tile.MoveType.Open_SOUTH) &&
                 WallExists(x, y, Tile.MoveType.Open_WEST);
        case Tile.MoveType.Open_UP:
        case Tile.MoveType.Open_DOWN:
        case Tile.MoveType.Open_VERT:
        case Tile.MoveType.Open_ALL:
        case Tile.MoveType.Open_HEX_NW:
        case Tile.MoveType.Open_HEX_NE:
        case Tile.MoveType.Open_HEX_SE:
        case Tile.MoveType.Open_HEX_SW:
        case Tile.MoveType.Open_HEX:
        case Tile.MoveType.Open_HEX_HORIZ:
        default:
          throw new NotImplementedException();
      }
    }

    public object Clone()
    {
      return new DungeonTiles()
      {
        Tiles = this.Tiles.Clone() as Tile[,],
        Parent = this.Parent,
        IsHex = this.IsHex
      };
    }
    #endregion
  }
}
