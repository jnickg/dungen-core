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
  public class TestAlgorithmParameter
  {
    [TestMethod]
    public void CompositeAlgorithmParameter()
    {
      CompositeAlgorithm alg = new CompositeAlgorithm();
      Assert.IsNotNull(alg.Algorithms);
      Assert.AreEqual(alg.Algorithms.Count, 0);

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

      IAlgorithm algFromPalette = paletteItem.CreateInstance();
      Assert.AreEqual(algFromPalette.GetType(), alg.GetType());
      Assert.IsNotNull(algFromPalette);
      Assert.IsTrue(algFromPalette.TakesParameters);
      Assert.IsNotNull(algFromPalette.Parameters);
      Assert.IsTrue(algFromPalette.Parameters.List.Count > 0);

      IAlgorithm algFromInfo = algInfo.CreateInstance();
      Assert.AreEqual(algFromInfo.GetType(), alg.GetType());
      Assert.IsNotNull(algFromInfo);
      Assert.IsTrue(algFromInfo.TakesParameters);
      Assert.IsNotNull(algFromInfo.Parameters);
      Assert.IsTrue(algFromInfo.Parameters.List.Count > 0);

      Assert.AreEqual(alg.Parameters.List.Count, algFromPalette.Parameters.List.Count);
      Assert.AreEqual(alg.Parameters.List.Count, algFromInfo.Parameters.List.Count);
      for (int i = 0; i < alg.Parameters.List.Count; ++i)
      {
        Assert.AreEqual(alg.Parameters.List[i].Value, algFromPalette.Parameters.List[i].Value);
        Assert.AreEqual(alg.Parameters.List[i].Value, algFromInfo.Parameters.List[i].Value);
      }
    }
  }
}