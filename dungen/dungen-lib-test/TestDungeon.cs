using Microsoft.VisualStudio.TestTools.UnitTesting;
using DunGen;
using DunGen.Lib;
using DunGen.TerrainGen;
using System.Collections.Generic;
using System;
using DunGen.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using DunGen.Generator;
using DunGen.Algorithm;
using DunGen.Serialization;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestDungeon
  {
    



    [TestMethod]
    public void AlgorithmRunInfo_ExactReproduction()
    {
      DungeonGenerator g1 = TestHelpers.GetTestDungeonGenerator();

      Dungeon d1 = g1.Generate();

      DungeonGenerator g2 = new DungeonGenerator()
      {
        WorkingDungeon = d1,
        Options = new DungeonGenerator.DungeonGeneratorOptions()
        {
          DoReset = true,
          Height = d1.Tiles.Height,
          Width = d1.Tiles.Width,
          // Here's what we're actually testing: that reconstructing the run gives exact results
          AlgRuns = d1.Runs.ReconstructRuns(),
        }
      };

      Dungeon d2 = g2.Generate();

#if RENDER_OUTPUT
      DungeonSerializer saver = new DungeonSerializer();
      DungeonTileRenderer renderer = new DungeonTileRenderer();
      try
      {
        saver.Save(d1, "d1_test.dgd", FileMode.Create);
        using (Image dungeonImage = renderer.Render(d1))
        {
          dungeonImage.Save("d1_test.bmp", ImageFormat.Bmp);
        }
        saver.Save(d2, "d2_test.dgd", FileMode.Create);
        using (Image dungeonImage = renderer.Render(d2))
        {
          dungeonImage.Save("d2_test.bmp", ImageFormat.Bmp);
        }
      }
      catch (Exception) { }
#endif

      for (int y = 0; y < d1.Tiles.Height; ++y)
      {
        for (int x = 0; x < d1.Tiles.Width; ++x)
        {
          Assert.AreEqual(d1.Tiles[y, x].Physics, d2.Tiles[y, x].Physics);
        }
      }
    }
  }
}
