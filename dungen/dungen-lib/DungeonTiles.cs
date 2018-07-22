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
      Threshold,  // Marks a threshold between areas, groups, or otherwise
      DeadEnd,    // The end of a hall of some type
      Niche,      // A small offshoot from a normal hallway
      // TODO more categories?
    }
    #endregion

    #region Statics
    public static readonly int GroupId_AllTiles = 0;
    #endregion

    #region Private Members
    private Tile[,] Tiles { get; set; }
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
    }

    /// <summary>
    /// Creates a new group of Tiles from the set specified and returns the
    /// ID of the new group.
    /// </summary>
    public int CreateGroup(ISet<Tile> tiles = null)
    {
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

    public bool TileIsValid(int x, int y)
    {
      if (x < 0 || y < 0 || x >= this.Width || y >= this.Height) return false;
      else return true;
    }
    #endregion
  }
}
