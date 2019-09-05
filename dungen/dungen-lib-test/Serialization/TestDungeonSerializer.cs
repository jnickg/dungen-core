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
  public class TestDungeonSerializer
  {
    [TestMethod]
    public void SaveAndLoad()
    {
      DungeonGenerator generator = TestHelpers.GetTestDungeonGenerator();

      Dungeon d_original = generator.Generate();

      DungeonSerializer serializer = new DungeonSerializer();
      string dungeonFile = System.IO.Path.GetTempFileName();
      serializer.Save(d_original, dungeonFile, FileMode.Create);
      Assert.IsTrue(File.Exists(dungeonFile));

      Dungeon d_loaded = serializer.Load(dungeonFile);
      Assert.IsNotNull(d_loaded);

      //
      // The basic qualities of the dungeon should match.
      //

      // The constituent tiles should be set appropriately
      Assert.AreEqual(d_original, d_original.Tiles.Parent);
      Assert.AreEqual(d_loaded, d_loaded.Tiles.Parent);

      // If the dungeons aren't the same dimensions, we definitely failed.
      Assert.AreEqual(d_original.Tiles.Width, d_loaded.Tiles.Width);
      Assert.AreEqual(d_original.Tiles.Height, d_loaded.Tiles.Height);
      
      // Physics must be identical, or else we didn't load the same dungeon
      foreach (Tile t in d_original.Tiles.Tiles_Set)
      {
        Assert.IsTrue(t.Physics == d_loaded.Tiles[t.Location.Y, t.Location.X].Physics);
      }

      //
      // We also save the "runs" associated with a file, to allow re-creating
      // it. Those should be saved/loaded
      //

      Assert.AreEqual(d_original.Runs.Count, d_loaded.Runs.Count);

      for (int i = 0; i < d_original.Runs.Count; ++i)
      {
        Assert.AreEqual(d_original.Runs[i].RandomSeed, d_loaded.Runs[i].RandomSeed);
        Assert.AreEqual(d_original.Runs[i].Info, d_loaded.Runs[i].Info);
        for (int m = 0; m < d_original.Runs[i].Mask.Count; ++m)
        {
          Assert.IsTrue(Enumerable.SequenceEqual(d_original.Runs[i].Mask[m], d_loaded.Runs[i].Mask[m]));
        }
      }

      File.Delete(dungeonFile);
    }
  }
}