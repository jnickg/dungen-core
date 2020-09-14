using System;

namespace DunGen.Algorithm
{
  /// <summary>
  /// An attribute tag used to identify information about an IAlgorithm
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class AlgorithmUsage : System.Attribute
  {
    /// <summary>
    /// A description of this Algorithm.
    /// </summary>
    public string Description { get; set; } = string.Empty;
  }
}
