using DunGen.Generator;
using DunGen.Infestation;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using DunGen.Tiles;

namespace DunGen
{
  /// <summary>
  /// A dungeon, its tiles, infestations, and information about how
  /// it was created.
  /// </summary>
  [DataContract(Name = "dungeon", IsReference = true)]
  public class Dungeon
  {
    #region Private Members
    private DungeonTiles PROPERTY_tiles;
    private List<TileGroupInfo> PROPERTY_groups = new List<TileGroupInfo>();
    #endregion

    /// <summary>
    /// The tiles associated with this Dungeon
    /// </summary>
    [DataMember(Name = "layout", Order = 0)]
    public DungeonTiles Tiles
    {
      get { return PROPERTY_tiles; }
      set
      {
        this.PROPERTY_tiles = value;
        value.Parent = this;
        if (null != Groups)
        {
          PROPERTY_groups.Clear(); // TODO this is probably not the best thing to do
          PROPERTY_groups.Add(new TileGroupInfo()
          {
            Category = TileCategory.Normal,
            Parent = this,
            Tiles = value.Tiles_Set
          });
        }
      }
    }

    /// <summary>
    /// A collection of grouped tiles, representing rooms, contiguous hallways, forest clearings,
    /// cave openings, etc.
    /// </summary>
    [DataMember(Name = "groups", Order = 1)]
    public List<TileGroupInfo> Groups
    {
      get => PROPERTY_groups;
      set
      {
        if (null == value)
        {
          PROPERTY_groups = value;
          return;
        }

        if (!value.All(group => group != null &&
                                group.Tiles.All(t => t.IsOwned && 
                                                     t.Parent != null &&
                                                     t.Parent == Tiles)))
        {
          throw new ArgumentException("Can only contain Tiles owned by this instance");
        }

        this.PROPERTY_groups = value;
        foreach (var group in value)
        {
          group.Parent = this;
        }
      }
    }

    /// <summary>
    /// The library used to infest this dungeon.
    /// </summary>
    [DataMember(Name = "library", Order = 2, EmitDefaultValue = false)]
    public Library InfestationLibrary { get; set; } = null;

    /// <summary>
    /// A collection of infestations, associated with groups of tiles in this dungeon.
    /// </summary>
    [DataMember(Name = "infestations", Order = 3)]
    public DungeonInfestations Infestations { get; set; } = new DungeonInfestations();

    /// <summary>
    /// An ordered list of Algorithm Runs that have been run on this object. This can be used to
    /// reproduce the Dungeon again, or undo selected steps.
    /// </summary>
    [DataMember(Name = "runs", Order = 4)]
    public AlgorithmRunInfoList Runs { get; set; } = new AlgorithmRunInfoList();

    /// <summary>
    /// Constructs an empty, tile-less dungeon instance.
    /// </summary>
    public Dungeon()
    {
      this.Tiles = new DungeonTiles(0, 0);
    }

    public void CreateGroup(ISet<Tile> tiles, TileCategory category = TileCategory.Normal)
    {
      if (null == tiles) return;

      TileGroupInfo newGroup = new TileGroupInfo()
      {
        Parent = this,
        Category = category,
        Tiles = new TileSet(tiles)
      };

      Groups.Add(newGroup);
    }

    public void CreateGroup(bool[,] mask, TileCategory category = TileCategory.Normal)
    {
      if (null == mask) return;

      ISet<Tile> tiles = Tiles.GetTilesIn(mask);
      if (null == tiles || tiles.Count == 0) return;

      CreateGroup(tiles, category);
    }

    public void CreateGroup(Rectangle region, TileCategory category = TileCategory.Normal)
    {
      ISet<Tile> tiles = Tiles.GetTilesIn(region);
      if (null == tiles || tiles.Count == 0) return;

      CreateGroup(tiles, category);
    }

    public IEnumerable<TileCategory> GetCategoriesFor(Tile t)
    {
      return Groups.Where(info => info.Tiles.Contains(t))
                   .Select(info => info.Category);
    }

    public IEnumerable<TileCategory> GetCategoriesFor(int x, int y)
    {
      Tile t = Tiles[y, x];
      return GetCategoriesFor(t);
    }

    public IEnumerable<TileCategory> GetCategoriesFor(Point location)
    {
      return GetCategoriesFor(location.X, location.Y);
    }

    public IEnumerable<TileCategory> GetCategoriesFor(IEnumerable<Tile> tiles)
    {
      if (null == tiles) return new List<TileCategory>();

      return Groups.Where(info => info.Tiles.Any(t => tiles.Contains(t)))
                   .Select(info => info.Category);
    }

    public IEnumerable<TileCategory> GetCategoriesFor(Rectangle region)
    {
      ISet<Tile> tiles = Tiles.GetTilesIn(region);
      return GetCategoriesFor(tiles);
    }

    public IEnumerable<TileCategory> GetCategoriesFor(bool[,] mask)
    {
      if (null == mask) return new List<TileCategory>();

      ISet<Tile> tiles = Tiles.GetTilesIn(mask);
      return GetCategoriesFor(tiles);
    }
  }
}
