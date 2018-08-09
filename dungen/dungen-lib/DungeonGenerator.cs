using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen
{
  public class DungeonGenerator
  {
    #region Nested Types
    public class DungeonGeneratorOptions
    {
      public bool DoReset { get; set; }
      /// <summary>
      /// The width of the dungeon to generate. Ignored if tiles are passed in, and
      /// DoReset is false.
      /// </summary>
      public int Width { get; set; }
      /// <summary>
      /// The height of the dungeon to generate. Ignored if tiles are passed in, and
      /// DoReset is false.
      /// </summary>
      public int Height { get; set; }
      public IList<Point> EgressConnections { get; set; } = new List<Point>();
      public IDictionary<ITerrainGenAlgorithm, bool[,]> TerrainGenAlgs { get; set; } = new Dictionary<ITerrainGenAlgorithm, bool[,]>();
      public IList<Action<DungeonTiles>> TerrainGenCallbacks { get; set; } = new List<Action<DungeonTiles>>();
    }
    #endregion

    #region Statics
    public static Dungeon Generate(DungeonGeneratorOptions options)
    {
      Random r = new Random();
      bool reset = false;
      List<ITerrainGenAlgorithm> terrainAlgs = new List<ITerrainGenAlgorithm>();
      List<bool[,]> algMasks = new List<bool[,]>();
      int width = 0;
      int height = 0;

      if (null != options)
      {
        terrainAlgs.AddRange(options.TerrainGenAlgs.Keys);
        algMasks.AddRange(options.TerrainGenAlgs.Values);
        reset = options.DoReset;
        width = options.Width;
        height = options.Height;
      }

      // Input validation

      if (null == terrainAlgs || null == algMasks)
      {
        throw new ArgumentNullException();
      }

      if (terrainAlgs.Count != algMasks.Count)
      {
        throw new ArgumentException("Must provide mask for each given algorithm");
      }

      foreach (var alg in algMasks)
      {
        if (alg.GetLength(0) != height ||
            alg.GetLength(1) != width)
        {
          throw new ArgumentException("Inconsistent width/height in specified options (check algorithm mask sizes)");
        }
      }

      // Create the dungeon
      Dungeon generatedDungeon = new Dungeon()
      {
        Tiles = new DungeonTiles(width, height)
      };

      // Generate terrain
      DungeonTiles tiles = generatedDungeon.Tiles;
      for (int i = 0; i < terrainAlgs.Count; ++i)
      {
        ISet<Tile> algTiles = new HashSet<Tile>();
        for (int y = 0; y < tiles.Height; ++y)
        {
          for (int x = 0; x < tiles.Width; ++x)
          {
            if (algMasks[i][y, x])
            {
              algTiles.Add(tiles[y, x]);
            }
          }
        }
        if (null != options.TerrainGenCallbacks && options.TerrainGenCallbacks.Count > 0)
        {
          foreach (var cb in options.TerrainGenCallbacks)
          {
            terrainAlgs[i].AttachCallback(cb);
          }
        }
        terrainAlgs[i].Run(tiles, algMasks[i], r);
        generatedDungeon.Algorithms.Add(terrainAlgs[i], algMasks[i]);
      }

      // TODO Generate infestations

      return generatedDungeon;
    }
    #endregion
    
    #region Members
    public DungeonGeneratorOptions Options { get; set; }

    public Dungeon Generate()
    {
      return Generate(this.Options);
    }
    #endregion
  }
}
