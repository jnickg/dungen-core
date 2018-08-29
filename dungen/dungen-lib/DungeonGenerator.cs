using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DunGen
{
  public class DungeonGenerator
  {
    #region Nested Types
    public class DungeonGeneratorOptions
    {
      /// <summary>
      /// Whether to reset the dungeon when Generated
      /// </summary>
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
      /// <summary>
      /// A list of egress connections. If populated prior to generation, algorithms
      /// must connect to these points. After generating, this contains the dungeon's
      /// egress connections.
      /// </summary>
      public IList<Point> EgressConnections { get; set; } = new List<Point>();
      /// <summary>
      /// Ongoing list of the algorithms that have been run.
      /// </summary>
      public IList<AlgorithmRun> TerrainGenAlgRuns { get; set; } = new List<AlgorithmRun>();
      /// <summary>
      /// Callbacks to run while generating.
      /// </summary>
      public IList<Action<DungeonTiles>> TerrainGenCallbacks { get; set; } = new List<Action<DungeonTiles>>();
    }
    #endregion

    #region Statics
    private static Dungeon CreateNewOrReturn(DungeonGeneratorOptions options, Dungeon d = null)
    {
      if (null == options) return new Dungeon();

      if (null == d)
      {
        d = new Dungeon();
      }

      if (d.Tiles.Width != options.Width || d.Tiles.Height != options.Height || options.DoReset)
      {
        d.Tiles.ResetTiles(options.Width, options.Height);
      }

      return d;
    }

    public static Dungeon Generate(DungeonGeneratorOptions options, Dungeon workingDungeon = null)
    {
      bool reset = false;
      List<AlgorithmRun> terrainAlgRuns = new List<AlgorithmRun>();
      int width = 0;
      int height = 0;

      if (null != options)
      {
        terrainAlgRuns.AddRange(options.TerrainGenAlgRuns);
        reset = options.DoReset;
        width = options.Width;
        height = options.Height;
      }

      // Input validation

      if (null == terrainAlgRuns)
      {
        throw new ArgumentNullException();
      }

      foreach (var run in terrainAlgRuns)
      {
        if (run.Context.Mask.GetLength(0) != height ||
            run.Context.Mask.GetLength(1) != width)
        {
          throw new ArgumentException("Inconsistent width/height in specified options (check algorithm mask sizes)");
        }
      }

      // Create the dungeon
      Dungeon generatedDungeon = CreateNewOrReturn(options, workingDungeon);

      // Generate terrain
      DungeonTiles tiles = generatedDungeon.Tiles;

      // Add new dungeon to algorithm's context
      foreach (var algRun in terrainAlgRuns)
      {
        algRun.Context.D = generatedDungeon;
      }

      for (int i = 0; i < terrainAlgRuns.Count; ++i)
      {
        ISet<Tile> algTiles = new HashSet<Tile>();
        for (int y = 0; y < tiles.Height; ++y)
        {
          for (int x = 0; x < tiles.Width; ++x)
          {
            if (terrainAlgRuns[i].Context.Mask[y, x])
            {
              algTiles.Add(tiles[y, x]);
            }
          }
        }
        if (null != options.TerrainGenCallbacks && options.TerrainGenCallbacks.Count > 0)
        {
          foreach (var cb in options.TerrainGenCallbacks)
          {
            ITerrainGenAlgorithm tgAlg = terrainAlgRuns[i].Alg as ITerrainGenAlgorithm;
            if (null != tgAlg)
            {
              tgAlg.AttachCallback(cb);
            }
          }
        }
        terrainAlgRuns[i].RunAlgorithm();
      }

      // TODO Generate infestations

      return generatedDungeon;
    }
    #endregion
    
    #region Members
    public DungeonGeneratorOptions Options { get; set; }

    public Dungeon WorkingDungeon { get; set; }

    public Dungeon Generate()
    {
      return Generate(this.Options, this.WorkingDungeon);
    }
    #endregion
  }
}
