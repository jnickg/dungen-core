using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace DunGen.Infestation
{
  /// <summary>
  /// An interface satisfied by all Terrain Generating Algorithms
  /// which ensures that the algorithm can be run, and provide
  /// some basic information about itself
  /// </summary>
  public interface IInfestationAlgorithm : IAlgorithm
  {
    // TODO
  }

  /// <summary>
  /// Shared base implementation for Terrain Generating algorithms.
  /// Plugin developers can expand on this abstract type, or completely
  /// implement the ITerrainGenAlgorithm interface themselves.
  /// </summary>
  public abstract class InfestationAlgorithmBase : AlgorithmBase, IInfestationAlgorithm
  {
    /// <see cref="AlgorithmBase._runInternal(IAlgorithmContext)"/>
    protected override void _runInternal(IAlgorithmContext context)
    {
      if (context == null) throw new ArgumentNullException("Can't infest without any context!");
      if (context.D == null) throw new ArgumentNullException("Can't infest nothing!");
      if (context.Mask == null) context.Mask = context.D.Tiles.DefaultMask;

      if (context.L == null || context.L.AllInfestations == null || context.L.AllInfestations.Count == 0)
      {
        // We have nothing to do if there is nothing to infest with
        return;
      }

      _runAlgorithm(context);
    }

    /// <summary>
    /// Runs the internal Infestation Algorithm, after the base implementation takes care of any
    /// common parameter checking. Implementor can assume the context is valid.
    /// </summary>
    /// <param name="context">The context in which this Algorithm is to be run.</param>
    protected abstract void _runAlgorithm(IAlgorithmContext context);

    /// <see cref="object.ToString"/>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendFormat("Algorithm \"{0}\"\n", Name);
      foreach (var param in Parameters.List)
      {
        sb.AppendLine(param.ToString());
      }

      return sb.ToString();
    }
  }
}
