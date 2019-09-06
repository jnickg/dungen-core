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
using DunGen.Algorithm;
using DunGen.Plugins;
using DunGen.Generator;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestDungeonTileRenderer
  {
    private static readonly int _dungeonHeight_default = 51;
    private static readonly int _dungeonWidth_default = 51;
    private static readonly bool[,] _dungeonMask_default;
    private static readonly AlgorithmRandom _r = new AlgorithmRandom(1337696937);

    static TestDungeonTileRenderer()
    {
      _dungeonMask_default = new bool[_dungeonHeight_default, _dungeonWidth_default];
      for (int y = 0; y < _dungeonMask_default.GetLength(0); ++y)
      {
        for (int x = 0; x < _dungeonMask_default.GetLength(1); ++x)
        {
          _dungeonMask_default[y, x] = true;
        }
      }
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
        AlgRuns = runs,
      };
      return generator;
    }

    [TestMethod]
    public void AllAlgorithmsDefaultParams()
    {
      foreach (var algProto in AlgorithmPluginEnumerator.GetAllLoadedAlgorithms())
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

        Dungeon d = null;
        try
        {
          var generator = CreateDefaultTestGenerator(runs);
          d = generator.Generate();
        }
        catch (NotImplementedException)
        {
          // If the algorithm is not implemented, that doesn't mean the
          // renderer is broken, so skip this Algorithm.
          continue;
        }

        Assert.IsNotNull(d);
        DungeonTileRenderer renderer = new DungeonTileRenderer();
        using (Image renderedDungeon = renderer.Render(d))
        {
          Assert.IsNotNull(renderedDungeon);
          string tempFile = System.IO.Path.GetTempFileName();
          renderedDungeon.Save(tempFile, ImageFormat.Bmp);
          Assert.IsTrue(System.IO.File.Exists(tempFile));
          System.IO.File.Delete(tempFile);
        }
      }
    }
  }
}
