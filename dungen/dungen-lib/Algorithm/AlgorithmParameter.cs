using DunGen.Plugins;
using System;

namespace DunGen.Algorithm
{
  public class AlgorithmParameter : Parameter
  {
    /// <summary>
    /// A more specific base type for the Algorithms allowed by this parameter. Any
    /// type inheriting from IAlgorithm is valid.
    /// </summary>
    public Type AlgorithmBaseType { get; set; } = typeof(IAlgorithm);

    /// <summary>
    /// The type of algorithm to be used by default. This currently can't hold
    /// parameters, so users are limited to specifying a basic algorithm type
    /// whose default parameters will be used.
    /// </summary>
    public Type DefaultType { get; set; }

    public AlgorithmParameter()
      : base(typeof(IAlgorithm))
    { }

    public override object GetDefault()
    {
      return AlgorithmPluginEnumerator.GetAlgorithm(DefaultType);
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      parsedValue = default(ParsedType); // Will return will if unsuccessful
      IAlgorithm algValue = null;
      Type typeOfValue = value.GetType();

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      // If the value is an info for some reason, instantiate it
      if (typeOfValue == typeof(AlgorithmInfo))
      {
        AlgorithmInfo infoVal = value as AlgorithmInfo;
        if (infoVal != null)
        {
          algValue = infoVal.ToInstance();
          valueOk = true;
        }
      }

      // The new value passed is an actual Algorithm
      if (typeof(IAlgorithm).IsAssignableFrom(typeOfValue))
      {
        valueOk = BaseType.IsAssignableFrom(typeOfValue) &&
          AlgorithmBaseType.IsAssignableFrom(typeOfValue) &&
          !typeOfValue.IsAbstract;
        if (!valueOk) return false;
        algValue = value as IAlgorithm;
      }

      parsedValue = (ParsedType)algValue;

      return valueOk;
    }
  }
}
