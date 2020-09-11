using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.Algorithm
{
  /// <summary>
  /// A pairing of an algorithm with its appropriate context. Also
  /// handles some basic logic of actually running the algorithm.
  /// </summary>
  public class AlgorithmRun
  {
    private IAlgorithm _alg;

    public IAlgorithm Alg
    {
      get => _alg;
      set
      {
        _alg = (IAlgorithm)value.Clone();
        if (_alg.TakesParameters)
        {
          _alg.Parameters = value.ParamsPrototype();
          _alg.Parameters = value.Parameters;
        }
      }
    }
    public IAlgorithmContext Context { get; set; }

    public void RunAlgorithm()
    {
      if (null != Alg)
      {
        Alg.Run(Context);
      }
    }

    public void PrepareFor(Dungeon d, AlgorithmRandom r = null)
    {
      if (d == null)
        throw new ArgumentNullException();
      if (r == null)
        r = AlgorithmRandom.RandomInstance();
      if (Context == null)
        Context = new AlgorithmContextBase();

      Context.D = d;
      Context.L = d.InfestationLibrary;

      if (Context.Mask == null && Context.D != null)
      {
        Context.Mask = Context.D.Tiles.DefaultMask;
      }
      if (Context.Mask.GetLength(0) != Context.D.Tiles.Height ||
          Context.Mask.GetLength(1) != Context.D.Tiles.Width)
      {
        throw new Exception("Invalid mask for algorithm run; can't be " +
          "used with given Dungeon");
      }

      if (Context.R == null)
        Context.R = r;
    }
  }
}
