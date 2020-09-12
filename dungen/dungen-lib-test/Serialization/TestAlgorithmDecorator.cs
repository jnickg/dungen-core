using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DunGen.Serialization;
using DunGen.TerrainGen;
using Newtonsoft.Json.Schema.Generation;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestAlgorithmDecorator
  {
    [TestMethod]
    public void Test()
    {
      var alg = AlgorithmDecorator.JsonSchema<BlobRecursiveDivision>();
      Assert.IsNotNull(alg);

      var schemer = new JSchemaGenerator();
      var schema = schemer.Generate(alg.GetType());
      Assert.IsNotNull(schema);

    }
  }
}
