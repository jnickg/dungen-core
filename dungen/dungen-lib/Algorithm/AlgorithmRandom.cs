using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using System.Text;

namespace DunGen.Algorithm
{
  /// <summary>
  /// A pseudorandom generator that can be saved/loaded, producing the
  /// same random pattern each time it is loaded. This makes the class
  /// useful for reproducing results in an algorithm relying on randomness
  /// </summary>
  [DataContract(Name = "rand")]
  public class AlgorithmRandom : Random
  {
    private int? _seed = null;

    /// <summary>
    /// The seed value used to initialize the base Randomness provider.
    /// If changed internally, this will reconstruct the object in place
    /// to use the new seed value.
    /// </summary>
    [DataMember(IsRequired = true, Name = "seed")]
    public int Seed
    {
      get => _seed ?? default(int);
      private set
      {
        // Should only happen when deserializing (setting Seed after empty ctor)
        if (null != _seed) throw new Exception("Can't re-assign seed after construction!");

        // Reconstruct the base Random using reflection, and pass the new value in
        this.GetType().GetConstructor(new Type[] { typeof(int) }).Invoke(this, new object[] { value });
        _seed = value; // Explicit just for sanity
      }
    }

    public UInt64 UseCount { get; private set; } = 0;

    // Only used for deserialization
    private AlgorithmRandom()
      : base()
    { }

    /// <summary>
    /// Constructs the Randomness provider using the specified base
    /// seed value.
    /// </summary>
    /// <param name="Seed"></param>
    public AlgorithmRandom(int Seed)
      : base(Seed)
    {
      this._seed = Seed;
    }

    /// <summary>
    /// Uses a random Randomness provider to construct a new AlgorithmRandom
    /// instance using a random random seed for its Randomness provider.
    /// </summary>
    /// <returns>A new, randomly-initialized AlgorithmRandom instance.</returns>
    public static AlgorithmRandom RandomInstance()
    {
      return new AlgorithmRandom(new Random().Next());
    }

    public override int Next()
    {
      ++UseCount;
      return base.Next();
    }

    public override int Next(int maxValue)
    {
      ++UseCount;
      return base.Next(maxValue);
    }

    public override int Next(int minValue, int maxValue)
    {
      ++UseCount;
      return base.Next(minValue, maxValue);
    }

    public override void NextBytes(byte[] buffer)
    {
      ++UseCount;
      base.NextBytes(buffer);
    }

    public override void NextBytes(Span<byte> buffer)
    {
      ++UseCount;
      base.NextBytes(buffer);
    }

    public override double NextDouble()
    {
      ++UseCount;
      return base.NextDouble();
    }
  }
}
