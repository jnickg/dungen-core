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
using DunGen.Plugins;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestAlgorithmPaletteSerializer
  {
    [TestMethod]
    public void SaveAndLoad()
    {
      AlgorithmPalette p_original = AlgorithmPalette.DefaultPalette(
        AlgorithmPluginEnumerator.GetAllLoadedAlgorithms());
      DungeonGenerator generator = TestHelpers.GetTestDungeonGenerator();
      Assert.IsNotNull(p_original);

      AlgorithmPaletteSerializer serializer = new AlgorithmPaletteSerializer();
      string paletteFile = System.IO.Path.GetTempFileName();
      serializer.Save(p_original, paletteFile, FileMode.Create);
      Assert.IsTrue(File.Exists(paletteFile));

      AlgorithmPalette p_loaded = serializer.Load(paletteFile);
      Assert.IsNotNull(p_loaded);

      Assert.AreEqual(p_original.Keys.Count, p_loaded.Keys.Count);
      Assert.IsTrue(Enumerable.SequenceEqual(p_original.Keys, p_loaded.Keys));

      foreach (var k in p_original.Keys)
      {
        Assert.AreEqual(p_original[k].Info, p_loaded[k].Info);
        Assert.AreEqual(p_original[k].PaletteColor, p_loaded[k].PaletteColor);
      }

      File.Delete(paletteFile);
    }

    [TestMethod]
    public void ExportForPdn()
    {
      AlgorithmPalette palette = AlgorithmPalette.DefaultPalette(
        AlgorithmPluginEnumerator.GetAllLoadedAlgorithms());
      Assert.IsNotNull(palette);

      AlgorithmPaletteSerializer serializer = new AlgorithmPaletteSerializer();
      string paletteFile = System.IO.Path.GetTempFileName();
      serializer.ExportForPdn(palette, paletteFile, FileMode.Create);
      Assert.IsTrue(File.Exists(paletteFile));
      
      // TODO actually inspect PDN template file
    }
  }
}