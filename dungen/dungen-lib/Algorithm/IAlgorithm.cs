using System;

namespace DunGen.Algorithm
{
  /// <summary>
  /// An interface satisfied by all algorithms that run when generating
  /// a dungeon
  /// </summary>
  public interface IAlgorithm : ICloneable
  {
    /// <summary>
    /// The name of this algorithm.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Whether this algorithm uses parameters. If false, getting
    /// Parameters returns null, and setting it does nothing
    /// </summary>
    bool TakesParameters { get; }
    /// <summary>
    /// The current run parameters for this algorithm. If the value is
    /// set, the new parameters should be used in the next run of the
    /// algorithm.
    /// </summary>
    AlgorithmParams Parameters { get; set; }
    /// <summary>
    /// Runs the Algorithm with the specified context
    /// </summary>
    void Run(IAlgorithmContext context);
    /// <summary>
    /// Attaches the given callback to this algorithm, to be called at
    /// various points of a call to this instance's
    /// <seealso cref="Run(IAlgorithmContext)"/>.
    /// </summary>
    /// <param name="callback">The callback to perform, which has access
    /// to the Dungeon's current state (but should NOT alter it)</param>
    void AttachCallback(Action<IAlgorithmContext> callback);
  }
}
