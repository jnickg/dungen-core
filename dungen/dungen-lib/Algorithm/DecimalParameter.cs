using System;

namespace DunGen.Algorithm
{
  public class DecimalParameter : Parameter
  {
    /// <summary>
    /// The minimum value for this Parameter.
    /// </summary>
    public double Minimum { get; set; } = double.MinValue;

    /// <summary>
    /// The maxmimum value for this Parameter.
    /// </summary>
    public double Maximum { get; set; } = double.MaxValue;

    /// <summary>
    /// The default value for this Parameter.
    /// </summary>
    public double Default { get; set; } = 0.0;

    /// <summary>
    /// The minimum number of precision points the algorithm agrees to honor
    /// when the algorithm runs. If "0", the value is unspecified.
    /// </summary>
    public int Precision { get; set; } = 0;

    public DecimalParameter()
      : base(typeof(double))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      double dblValue = 0.0;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      // See if we can simply cast it
      if (value.GetType().IsPrimitive)
      {
        dblValue = (double)value;
        valueOk = (dblValue >= Minimum && dblValue <= Maximum);
      }
      // Next try parsing it
      if (double.TryParse(value.ToString(), out dblValue))
      {
        valueOk = (dblValue >= Minimum && dblValue <= Maximum);
      }

      parsedValue = (ParsedType)Convert.ChangeType(dblValue, typeof(ParsedType));
      return valueOk;
    }
  }
}
