using System;
using System.Collections.Generic;
using System.Text;
using DunGen.Infestation;

namespace DunGen.Algorithm
{
  /// <summary>
  /// A context in which an Algorithm can be run.
  /// </summary>
  public interface IAlgorithmContext
  {
    /// <summary>
    /// The dungeon on which the Algorithm should run
    /// </summary>
    Dungeon D { get; set; }
    /// <summary>
    /// The library used by any algorithms that will infest the tiles
    /// </summary>
    Library L { get; set; }
    /// <summary>
    /// The mask which the Algorithm should use when operating on
    /// the Context's dungeon
    /// </summary>
    bool[,] Mask { get; set; }
    /// <summary>
    /// If not NULL, A user-specified Random object to be used by the
    /// Algorithm.
    /// </summary>
    AlgorithmRandom R { get; set; }
  }

  /// <summary>
  /// A general context for an Algorithm, with zero frills.
  /// </summary>
  public class AlgorithmContextBase : IAlgorithmContext
  {
    /// <see cref="IAlgorithmContext.D"/>
    public Dungeon D { get; set; }

    /// <see cref="IAlgorithmContext.L"/>
    public Library L { get; set; }

    /// <see cref="IAlgorithmContext.Mask"/>
    public bool[,] Mask { get; set; }

    /// <see cref="IAlgorithmContext.R"/>
    public AlgorithmRandom R { get; set; }
  }
}
