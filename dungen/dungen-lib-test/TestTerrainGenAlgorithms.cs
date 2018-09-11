//#define RENDER_OUTPUT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DunGen;
using DunGen.Lib;
using DunGen.TerrainGen;
using System.Collections.Generic;
using System;
using DunGen.Rendering;
using System.Drawing;
using System.Drawing.Imaging;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestTerrainGenAlgorithms
  {
    private static readonly int _dungeonHeight_default = 51;
    private static readonly int _dungeonWidth_default = 51;
    private static readonly bool[,] _dungeonMask_default;
    private static readonly AlgorithmRandom _r = new AlgorithmRandom(1337696937);
    private static AlgorithmPluginManager _notPlugins = new AlgorithmPluginManager();

    static TestTerrainGenAlgorithms()
    {
      _dungeonMask_default = new bool[_dungeonHeight_default, _dungeonWidth_default];
      for (int y = 0; y < _dungeonMask_default.GetLength(0); ++y)
      {
        for (int x = 0; x < _dungeonMask_default.GetLength(1); ++x)
        {
          _dungeonMask_default[y, x] = true;
        }
      }

      _notPlugins.Enumerate();
    }

    private DungeonGenerator CreateDefaultTestGenerator(IList<AlgorithmRun> runs)
    {
      DungeonGenerator generator = new DungeonGenerator();
      generator.WorkingDungeon = new Dungeon()
      {
        Tiles = new DungeonTiles(_dungeonWidth_default, _dungeonHeight_default)
      };
      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = false,
        EgressConnections = null,
        Width = _dungeonWidth_default,
        Height = _dungeonHeight_default,
        TerrainGenAlgRuns = runs,
      };
      return generator;
    }

    [TestMethod]
    public void RunWithDefaultParams()
    {
      foreach (var algProto in _notPlugins.AlgorithmProtos)
      {
        IList<AlgorithmRun> runs = new List<AlgorithmRun>()
        {
          new AlgorithmRun()
          {
            Alg = algProto.Clone() as IAlgorithm,
            Context = new AlgorithmContextBase()
            {
              Mask = _dungeonMask_default,
              R = _r
            }
          },
        };
        var generator = CreateDefaultTestGenerator(runs);
        Dungeon d = generator.Generate();
#if RENDER_OUTPUT
        DungeonTileRenderer renderer = new DungeonTileRenderer();
        using (Image renderedDungeon = renderer.Render(d))
        {
          renderedDungeon.Save(String.Format("dungeon_{0}.bmp", algProto.Name), ImageFormat.Bmp);
        }
#endif
      }
    }

    // TODO use answer at this link to devise a reflective way to create individual tests for each algorithm in a huge dungeon
    // https://stackoverflow.com/questions/44789698/is-there-a-better-way-to-pass-dynamic-inputs-in-line-to-a-datatestmethod-i-e-h
    [TestMethod]
    public void RunHugeDungeon()
    {
      const int hugeSize = 37;
      bool[,] hugeMask = new bool[hugeSize, hugeSize];
      for (int y = 0; y < hugeMask.GetLength(0); ++y)
      {
        for (int x = 0; x < hugeMask.GetLength(1); ++x)
        {
          hugeMask[y, x] = true;
        }
      }
      var hugeContext = new AlgorithmContextBase()
      {
        Mask = hugeMask,
        R = _r
      };

      foreach (var algProto in _notPlugins.AlgorithmProtos)
      {
        IList<AlgorithmRun> runs = new List<AlgorithmRun>()
        {
          new AlgorithmRun()
          {
            Alg = algProto.Clone() as IAlgorithm,
            Context = hugeContext
          },
        };
        var generator = CreateDefaultTestGenerator(runs);
        generator.Options.Height = hugeSize;
        generator.Options.Width = hugeSize;
        generator.Options.DoReset = true;
        
        Dungeon d = generator.Generate();
      }
    }
  }
}
