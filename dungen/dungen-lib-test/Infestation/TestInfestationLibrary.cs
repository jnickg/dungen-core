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
using DunGen.Infestation;
using DunGen.Serialization;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestInfestationLibrary
  {
    [TestMethod]
    public void LoadsFromDatabase()
    {
      Library library = TestHelpers.GetTestLibrary();

      Assert.IsNotNull(library);
      Assert.AreEqual(library.AllInfestations.Count, 1000);
      Assert.AreEqual(library.Labels.Count, 7);

      // TODO Test the loaded library a little more thoroughly :-)
    }

    [TestMethod]
    public void InfestationAlgorithmInfests()
    {
      Library lib = TestHelpers.GetTestLibrary();

      // TODO!
    }
  }
}