using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen
{
  /// <summary>
  /// A collection of Tiles comprosing a Dungeon's physical layout. Can be
  /// categorized and grouped
  /// </summary>
  public class DungeonTiles
  {
    #region Nested Types
    /// <summary>
    /// Categorical purpose buckets into which Tiles can be put during terrain
    /// generation, to indicate a special purpose later in the process of
    /// generating a dungeon (such as placing a door, trap, etc.)
    /// </summary>
    public enum Category
    {
      Normal,     // Only the Tile's data is important
      Threshold,  // Marks a threshold between connected areas, groups, or otherwise
      Niche,      // A small offshoot from a normal hallway that shouldn't be culled
      Room,       // A room that should be connected to hallways
      Hallway     // A hallway that should be connected to something else
      // TODO more categories?
    }
    #endregion

    #region Statics
    public static readonly int GroupId_AllTiles = 0;
    #endregion

    #region Private Members
    private Tile[,] Tiles { get; set; }
    private IDictionary<Category, IList<ISet<Tile>>> CategoryGroups { get; set; }
    #endregion

    #region Properties
    public Dungeon Parent { get; internal set; }
    public ISet<Tile> Tiles_Set { get; set; }
    public IDictionary<int, Point> TilesById { get; set; }
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
    /// A List of Tile groupings, represented as Sets. Each list should contain
    /// only tiles contained by the "Tiles" property.
    /// </summary>
    public IList<ISet<Tile>> Groups { get; private set; }

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
      List<Tile> tilePool = new List<Tile>(this.GetAllIn(mask));
      return tilePool[r.Next(tilePool.Count)];
    }

    public IEnumerable<Tile> GetAllIn(bool[,] mask)
    {
      if (null == mask) return new HashSet<Tile>();

      ISet<Tile> rtn = new HashSet<Tile>();
      for (int y = 0; y < this.Height; ++y)
      {
        for (int x = 0; x < this.Width; ++x)
        {
          if (mask[y,x]) rtn.Add(this[y,x]);
        }
      }

      return rtn;
    }

    public DungeonTiles(int width, int height, Tile.MoveType startingPhyics = Tile.MoveType.Wall)
    {
      ResetTiles(width, height, startingPhyics);
    }

    public void ResetTiles(int width, int height, Tile.MoveType startingPhyics = Tile.MoveType.Wall)
    {
      this.Tiles = new Tile[height, width];
      this.TilesById = new Dictionary<int, Point>();
      this.Groups = new List<ISet<Tile>>();
      this.Tiles_Set = new HashSet<Tile>();
      for (int y = 0; y < height; ++y)
      {
        for (int x = 0; x < width; ++x)
        {
          this[y, x] = new Tile()
          {
            Parent = this,
            Physics = startingPhyics
          };
          this.Tiles_Set.Add(this[y, x]);
          this.TilesById.Add(this[y, x].Id, new Point(x, y));
        }
      }
      this.IsHex = false; // TODO
      this.CategoryGroups = new Dictionary<Category, IList<ISet<Tile>>>();
      foreach(Category c in (Category[])Enum.GetValues(typeof(Category)))
      {
        this.CategoryGroups.Add(c, new List<ISet<Tile>>());
      }
    }

    public void Categorize(ISet<Tile> tiles = null, Category cat = Category.Normal)
    {
      if (null == tiles) return;
      this.CategoryGroups[cat].Add(new HashSet<Tile>(tiles));
    }

    public void Categorize(bool[,]mask, Category cat = Category.Normal)
    {
      if (null == mask) return;
      if (mask.GetLength(0) != this.Tiles.GetLength(0)
       || mask.GetLength(1) != this.Tiles.GetLength(1))
      {
        throw new ArgumentException("Invalid mask");
      }

      ISet<Tile> newSet = new HashSet<Tile>();

      for (int y = 0; y < this.Height; ++y)
      {
        for (int x = 0; x < this.Width; ++x)
        {
          if (mask[y, x]) newSet.Add(this[y, x]);
        }
      }

      this.Categorize(newSet, cat);
    }

    public void Categorize(Rectangle region, Category cat = Category.Normal)
    {
      if (null == region) return;
      if (region.X + region.Width > this.Width ||
          region.Y + region.Height > this.Height ||
          region.X < 0 || region.Y < 0)
      {
        throw new ArgumentException("Invalid region");
      }

      ISet<Tile> newSet = new HashSet<Tile>();

      int endY = region.Y + region.Height;
      int endX = region.X + region.Width;
      for (int y = region.Y; y < endY; ++y)
      {
        for (int x = region.X; x < endX; ++x)
        {
          newSet.Add(this[y, x]);
        }
      }

      this.Categorize(newSet, cat);
    }

    public List<Category> GetCategoriesFor(int x, int y)
    {
      List<Category> cellCats = new List<Category>();
      Point requestedPoint = new Point(x, y);
      foreach (Category c in this.CategoryGroups.Keys)
      {
        bool found = false;
        foreach (var g in this.CategoryGroups[c])
        {
          foreach (Tile t in g)
          {
            if (t.Location == requestedPoint)
            {
              cellCats.Add(c);
              found = true;
              break;
            }
          }
          if (found) break;
        }
      }
      return cellCats;
    }

    /// <summary>
    /// Finds EVERY group in specified category containing the provided tile location,
    /// and removes it from that category
    /// </summary>
    public void DeCategorizeAll(int x, int y, Category cat)
    {
      List<ISet<Tile>> matchingGroupos = new List<ISet<Tile>>();
      Point requestedPoint = new Point(x, y);
      foreach (ISet<Tile> g in this.CategoryGroups[cat])
      {
        bool found = false;
        foreach (Tile t in g)
        {
          if (t.Location == requestedPoint)
          {
            found = true;
            break;
          }
        }
        if (found)
        {
          matchingGroupos.Add(g);
        }
      }
      foreach (ISet<Tile> g in matchingGroupos)
      {
        this.CategoryGroups[cat].Remove(g);
      }
    }

    /// <summary>
    /// Creates a new group of Tiles from the set specified and returns the
    /// ID of the new group.
    /// </summary>
    public int CreateGroup(ISet<Tile> tiles = null)
    {
      if (null == tiles) return -1;
      ISet<Tile> newSet = new HashSet<Tile>(tiles);
      this.Groups.Add(newSet);
      return this.Groups.IndexOf(newSet);
    }

    public int CreateGroup(bool[,] mask)
    {
      if (null == mask) return -1;
      if (mask.GetLength(0) != this.Tiles.GetLength(0)
       || mask.GetLength(1) != this.Tiles.GetLength(1))
      {
        throw new ArgumentException("Invalid mask");
      }

      ISet<Tile> newSet = new HashSet<Tile>();

      for (int y = 0; y < this.Height; ++y)
      {
        for (int x = 0; x < this.Width; ++x)
        {
          if (mask[y, x]) newSet.Add(this[y, x]);
        }
      }

      return this.CreateGroup(newSet);
    }

    public int CreateGroup(Rectangle region)
    {
      if (null == region) return -1;
      if (region.X + region.Width > this.Width ||
          region.Y + region.Height > this.Height ||
          region.X < 0 || region.Y < 0)
      {
        throw new ArgumentException("Invalid region");
      }

      ISet<Tile> newSet = new HashSet<Tile>();

      int endY = region.Y + region.Height;
      int endX = region.X + region.Width;
      for (int y = region.Y; y < endY; ++y)
      {
        for (int x = region.X; x < endX; ++x)
        {
          newSet.Add(this[y, x]);
        }
      }

      return this.CreateGroup(newSet);
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
          return (!TileIsValid(x, y - 1) || (0 == (this[y - 1, x].Physics & direction.GetOpposite())));
        case Tile.MoveType.Open_EAST:
          return (!TileIsValid(x + 1, y) || (0 == (this[y, x + 1].Physics & direction.GetOpposite())));
        case Tile.MoveType.Open_SOUTH:
          return (!TileIsValid(x, y + 1) || (0 == (this[y + 1, x].Physics & direction.GetOpposite())));
        case Tile.MoveType.Open_WEST:
          return (!TileIsValid(x - 1, y) || (0 == (this[y, x - 1].Physics & direction.GetOpposite())));
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
    #endregion
  }
}
