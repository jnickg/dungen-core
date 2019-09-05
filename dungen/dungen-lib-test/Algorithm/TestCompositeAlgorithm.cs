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
  public class TestCompositeAlgorithm
  {
    [TestMethod]
    public void Consistency()
    {
      CompositeAlgorithm alg = new CompositeAlgorithm();
      Assert.IsNotNull(alg.Algorithms);
      Assert.AreEqual(alg.Algorithms.Count, 0);
      Assert.IsFalse(alg.TakesParameters);
      Assert.IsTrue(alg.Parameters == null || alg.Parameters.List.Count == 0);

      var newAlgs = new List<IAlgorithm>()
      {
        new NopAlgorithm(),
        new MonteCarloRoomCarver(),
        new NopTerrainGen(),
      };

      alg.Algorithms.AddRange(newAlgs);
      Assert.AreEqual(alg.Algorithms.Count, newAlgs.Count);

      var algInfo = alg.ToInfo();
      Assert.IsNotNull(algInfo);
      Assert.AreEqual(algInfo.Type, alg.GetType());

      var paletteItem = alg.ToPaletteItem();
      Assert.IsNotNull(paletteItem);
      Assert.IsNotNull(paletteItem.Info);

      IAlgorithm algFromPalette = paletteItem.CreateInstance();
      Assert.AreEqual(algFromPalette.GetType(), alg.GetType());
      Assert.IsNotNull(algFromPalette);
      Assert.IsFalse(algFromPalette.TakesParameters);
      Assert.IsTrue(algFromPalette.Parameters == null || algFromPalette.Parameters.List.Count == 0);

      IAlgorithm algFromInfo = algInfo.CreateInstance();
      Assert.AreEqual(algFromInfo.GetType(), alg.GetType());
      Assert.IsNotNull(algFromInfo);
      Assert.IsFalse(algFromInfo.TakesParameters);
      Assert.IsTrue(algFromInfo.Parameters == null || algFromInfo.Parameters.List.Count == 0);

      CompositeAlgorithm compositeFromPalette = algFromPalette as CompositeAlgorithm;
      Assert.IsNotNull(compositeFromPalette);
      Assert.IsTrue(alg.Algorithms.Count == compositeFromPalette.Algorithms.Count);
      Assert.AreEqual(alg.Name, compositeFromPalette.Name);

      CompositeAlgorithm compositeFromInfo = algFromInfo as CompositeAlgorithm;
      Assert.IsNotNull(compositeFromInfo);
      Assert.IsTrue(alg.Algorithms.Count == compositeFromInfo.Algorithms.Count);
      Assert.AreEqual(alg.Name, compositeFromInfo.Name);

      // TODO test that the constituent algorithms are ALSO all equal
    }
  }
}