﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DunGen
{
  public class DungeonGenerator
  {
    #region Nested Types
    /// <summary>
    /// An object containing user-specified options for how the DungeonGenerator should
    /// behave when creating a new Dungeon
    /// </summary>
    public class DungeonGeneratorOptions
    {
      /// <summary>
      /// Whether to reset the dungeon when Generated
      /// </summary>
      public bool DoReset { get; set; } = false;
      /// <summary>
      /// The width of the dungeon to generate. Ignored if tiles are passed in, and
      /// DoReset is false.
      /// </summary>
      public int Width { get; set; } = 0;
      /// <summary>
      /// The height of the dungeon to generate. Ignored if tiles are passed in, and
      /// DoReset is false.
      /// </summary>
      public int Height { get; set; } = 0;
      /// <summary>
      /// A list of egress connections. If populated prior to generation, algorithms
      /// must connect to these points. After generating, this contains the dungeon's
      /// egress connections.
      /// </summary>
      public IList<Point> EgressConnections { get; set; } = new List<Point>();
      /// <summary>
      /// List of algorithm runs to use with this set of options
      /// </summary>
      public IList<AlgorithmRun> TerrainGenAlgRuns { get; set; } = new List<AlgorithmRun>();
      /// <summary>
      /// Callbacks to run while generating.
      /// </summary>
      public IList<Action<DungeonTiles>> TerrainGenCallbacks { get; set; } = new List<Action<DungeonTiles>>();
    }
    #endregion

    #region Statics
    /// <summary>
    /// Given the specified input options, and optional input dungeon, prepares and returns a
    /// Dungeon object that is ready to work on.
    /// </summary>
    private static Dungeon PrepareWorkingDungeon(DungeonGeneratorOptions options, Dungeon d = null)
    {
      if (null == options) return new Dungeon();

      if (null == d)
      {
        d = new Dungeon();
      }

      if ((d.Tiles.Width != options.Width || d.Tiles.Height != options.Height) && options.DoReset)
      {
        d.Tiles.ResetTiles(options.Width, options.Height);
      }

      return d;
    }

    public static Dungeon Generate(DungeonGeneratorOptions options, Dungeon starterDungeon = null)
    {
      bool reset = false;
      List<AlgorithmRun> terrainAlgRuns = new List<AlgorithmRun>();
      int width = 0;
      int height = 0;

      if (null != options)
      {
        terrainAlgRuns.AddRange(options.TerrainGenAlgRuns);
        reset = options.DoReset;
      }

      // Input validation

      if (null == terrainAlgRuns)
      {
        throw new ArgumentNullException();
      }

      // Prepare context for each algorithm run appropriately.

      Dungeon workingDungeon = PrepareWorkingDungeon(options, starterDungeon);
      // Prepare algorithm runs to work on the dungeon
      foreach (var run in terrainAlgRuns)
      {
        run.PrepareFor(workingDungeon);
      }

      // Generate terrain
      DungeonTiles tiles = workingDungeon.Tiles;
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

      return workingDungeon;
    }
    #endregion
    
    #region Members
    public DungeonGeneratorOptions Options { get; set; }

    public Dungeon WorkingDungeon { get; set; }

    /// <summary>
    /// Generates and returns a dungeon given this object's specified options and
    /// working dungeon.
    /// </summary>
    public Dungeon Generate()
    {
      return Generate(this.Options, this.WorkingDungeon);
    }
    #endregion
  }
}
