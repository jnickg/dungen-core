using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace DunGen.TerrainGen
{
  /// <summary>
  /// An interface satisfied by all Terrain Generating Algorithms
  /// which ensures that the algorithm can be run, and provide
  /// some basic information about itself
  /// </summary>
  public interface ITerrainGenAlgorithm : IAlgorithm
  {
    /// <summary>
    /// How this algorithm behaves, given its current parameters.
    /// </summary>
    TerrainModBehavior Behavior { get; }

    /// <summary>
    /// The style of this algorithm.
    /// </summary>
    TerrainGenStyle Style { get; }
  }
}
