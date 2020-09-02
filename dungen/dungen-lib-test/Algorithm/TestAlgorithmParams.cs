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
  public class TestAlgorithmParams
  {
    public class TestAlgorithm : AlgorithmBase
    {
      public enum Selectable
      {
        Option1 = 1,
        Option2 = 2,
        Option3 = 3
      }

      public const Selectable DefaultSelectable = Selectable.Option2;
      public const double MinimumDecimal = 0.0d;
      public const double MaximumDecimal = 37.0d;
      public const int PrecisionForDouble = 2;
      public const double DefaultDecimal = 18.5d;
      public const int MinimumInteger = 0;
      public const int MaximumInteger = 10;
      public const int DefaultInteger = 5;
      public const bool DefaultBool = true;

      [SelectionParameter(
        Description = "Selectable parameter",
        SelectionType = typeof(Selectable),
        Default = DefaultSelectable)]
      public Selectable SelectableParam { get; set; }

      [DecimalParameter(
        Description ="Some Decimal-based parameter",
        Minimum = MinimumDecimal,
        Maximum = MaximumDecimal,
        Default = DefaultDecimal,
        Precision = PrecisionForDouble)]
      public double DecimalParam { get; set; }

      [IntegerParameter(
        Description = "Some Integer-based parameter",
        Minimum = MinimumInteger,
        Maximum = MaximumInteger,
        Default = DefaultInteger)]
      public int IntegerParam { get; set; }

      [BooleanParameter(
        Description = "Some Boolean-based parameter",
        Default = DefaultBool)]
      public bool BoolParam { get; set; }

      [AlgorithmParameter(
        Description = "Some Algorithm-based parameter",
        AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
        DefaultType = typeof(NopTerrainGen))]
      public IAlgorithm AlgorithmParam { get; set; }

      protected override void _runInternal(IAlgorithmContext context)
      {
        // nop
      }
    }

    [TestMethod]
    public void SelectionParameter()
    {
      TestAlgorithm alg = new TestAlgorithm();
      Assert.AreEqual(alg.SelectableParam, TestAlgorithm.DefaultSelectable);

      alg.SelectableParam = (TestAlgorithm.Selectable)37;
      Assert.ThrowsException<ArgumentException>(() => TestHelpers.GenerateWith(alg));

      TestAlgorithm.Selectable modifiedVal = TestAlgorithm.Selectable.Option1;
      alg.SelectableParam = modifiedVal;

      Dungeon d = TestHelpers.GenerateWith(alg);
      Assert.IsNotNull(d);
      Assert.AreEqual(d.Runs.Count, 1);

      IEditableParameter matchingParam = d.Runs.First().Info.Parameters.List.First(p => p.ParamName == nameof(alg.SelectableParam));
      Assert.IsNotNull(matchingParam);
      Assert.IsNotNull(matchingParam.Value);
      Assert.AreEqual(matchingParam.Value, alg.SelectableParam);

      TestAlgorithm alg_post = d.Runs.First().Info.ToInstance() as TestAlgorithm;
      Assert.AreEqual(alg.SelectableParam, alg_post.SelectableParam);
    }

    [TestMethod]
    public void DecimalParam()
    {
      TestAlgorithm alg = new TestAlgorithm();
      Assert.AreEqual(alg.DecimalParam, TestAlgorithm.DefaultDecimal);

      alg.DecimalParam = TestAlgorithm.MinimumDecimal - 1.0;
      Assert.ThrowsException<ArgumentException>(() => TestHelpers.GenerateWith(alg));
      alg.DecimalParam = TestAlgorithm.MaximumDecimal + 1.0;
      Assert.ThrowsException<ArgumentException>(() => TestHelpers.GenerateWith(alg));

      double modifiedVal = TestAlgorithm.MinimumDecimal;
      alg.DecimalParam = modifiedVal;

      Dungeon d = TestHelpers.GenerateWith(alg);
      Assert.IsNotNull(d);
      Assert.AreEqual(d.Runs.Count, 1);

      IEditableParameter matchingParam = d.Runs.First().Info.Parameters.List.First(p => p.ParamName == nameof(alg.DecimalParam));
      Assert.IsNotNull(matchingParam);
      Assert.IsNotNull(matchingParam.Value);
      Assert.AreEqual(matchingParam.Value, alg.DecimalParam);

      TestAlgorithm alg_post = d.Runs.First().Info.ToInstance() as TestAlgorithm;
      Assert.AreEqual(alg.DecimalParam, alg_post.DecimalParam);
    }


    [TestMethod]
    public void IntegerParam()
    {
      TestAlgorithm alg = new TestAlgorithm();
      Assert.AreEqual(alg.IntegerParam, TestAlgorithm.DefaultInteger);

      alg.IntegerParam = TestAlgorithm.MinimumInteger - 1;
      Assert.ThrowsException<ArgumentException>(() => TestHelpers.GenerateWith(alg));
      alg.IntegerParam = TestAlgorithm.MaximumInteger + 1;
      Assert.ThrowsException<ArgumentException>(() => TestHelpers.GenerateWith(alg));

      int modifiedVal = TestAlgorithm.MinimumInteger;
      alg.IntegerParam = modifiedVal;

      Dungeon d = TestHelpers.GenerateWith(alg);
      Assert.IsNotNull(d);
      Assert.AreEqual(d.Runs.Count, 1);

      IEditableParameter matchingParam = d.Runs.First().Info.Parameters.List.First(p => p.ParamName == nameof(alg.IntegerParam));
      Assert.IsNotNull(matchingParam);
      Assert.IsNotNull(matchingParam.Value);
      Assert.AreEqual(matchingParam.Value, alg.IntegerParam);

      TestAlgorithm alg_post = d.Runs.First().Info.ToInstance() as TestAlgorithm;
      Assert.AreEqual(alg.IntegerParam, alg_post.IntegerParam);
    }

    [TestMethod]
    public void BoolParam()
    {
      TestAlgorithm alg = new TestAlgorithm();
      Assert.AreEqual(alg.BoolParam, TestAlgorithm.DefaultBool);

      bool modifiedVal = !TestAlgorithm.DefaultBool;
      alg.BoolParam = modifiedVal;

      Dungeon d = TestHelpers.GenerateWith(alg);
      Assert.IsNotNull(d);
      Assert.AreEqual(d.Runs.Count, 1);

      IEditableParameter matchingParam = d.Runs.First().Info.Parameters.List.First(p => p.ParamName == nameof(alg.BoolParam));
      Assert.IsNotNull(matchingParam);
      Assert.IsNotNull(matchingParam.Value);
      Assert.AreEqual(matchingParam.Value, alg.BoolParam);

      TestAlgorithm alg_post = d.Runs.First().Info.ToInstance() as TestAlgorithm;
      Assert.AreEqual(alg.BoolParam, alg_post.BoolParam);
    }

    [TestMethod]
    public void AlgorithmParam()
    {
      TestAlgorithm alg = new TestAlgorithm();
      Assert.AreEqual(alg.AlgorithmParam, new NopTerrainGen());

      alg.AlgorithmParam = new NopAlgorithm(); // Not an ITerrainGenAlgorithm
      Assert.ThrowsException<ArgumentException>(() => TestHelpers.GenerateWith(alg));

      IAlgorithm modifiedVal = new MonteCarloRoomCarver()
      {
        GroupRooms = true,
        OpenTilesStrategy = TerrainGenAlgorithmBase.OpenTilesHandling.Avoid,
        BorderPadding = 1
      };

      alg.AlgorithmParam = modifiedVal;

      Dungeon d = TestHelpers.GenerateWith(alg);
      Assert.IsNotNull(d);
      Assert.AreEqual(d.Runs.Count, 1);

      IEditableParameter matchingParam = d.Runs.First().Info.Parameters.List.First(p => p.ParamName == nameof(alg.AlgorithmParam));
      Assert.IsNotNull(matchingParam);
      Assert.IsNotNull(matchingParam.Value);
      Assert.AreEqual(matchingParam.Value, alg.AlgorithmParam);

      TestAlgorithm alg_post = d.Runs.First().Info.ToInstance() as TestAlgorithm;
      Assert.AreEqual(alg.AlgorithmParam, alg_post.AlgorithmParam);

      // TODO go through each parameter of alg.AlgorithmParam to make sure _those_ params are equal?
    }


    [TestMethod]
    public void Consistency()
    {
      
    }
  }
}