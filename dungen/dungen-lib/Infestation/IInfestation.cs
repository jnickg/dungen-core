using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.Infestation
{
  /// <summary>
  /// The base interface through which users can interact with an infestation. Provides
  /// basic information about the infestation itself, and exposes any methods needed to
  /// manipulate it.
  /// </summary>
  public interface IInfestation
  {
    /// <summary>
    /// 
    /// </summary>
    Library Parent { get; }
    /// <summary>
    /// The name of this infestation, that most succinctly identifies it to an end user.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// A brief description of this infestation, or tip about it, for an end user.
    /// </summary>
    string Brief { get; }
    /// <summary>
    /// A more detailed overview of this infestation, giving the end user basic information about
    /// how to interact with it.
    /// </summary>
    string Overview { get; }
    /// <summary>
    /// A URI to a resource the end user can access, to learn more about this infestation.
    /// </summary>
    string URI { get; }
    /// <summary>
    /// The minimum number of contiguous tiles needed to contain this infestation.
    /// </summary>
    int Size { get; }
    /// <summary>
    /// The standalone (non-labeled) Weighted Occurrence Factor for this infestation. A Weighted
    /// Occurrence Factor defaults to 1.0, with 0.0 meaning "impossible to occur," and 2.0 meaning
    /// "twice as likely to occur." All WOF values are relative to each other, within the context
    /// of a single <see cref="Library"/>
    /// </summary>
    double OccurrenceFactor { get; }
    /// <summary>
    /// The first-class type of infestation that this instance is.
    /// </summary>
    InfestationType Category { get; }
    /// <summary>
    /// Additional labels for this infestation that can be used for reference, or to influence
    /// the likelihood of it appearing.
    /// </summary>
    IDictionary<Label, double> Labels { get; }
  }
}
