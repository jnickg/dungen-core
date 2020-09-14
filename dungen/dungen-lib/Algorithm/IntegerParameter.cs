using System;

namespace DunGen.Algorithm
{
  public class IntegerParameter : Parameter
  {
    /// <summary>
    /// The minimum value for this Parameter.
    /// </summary>
    public int Minimum { get; set; } = int.MinValue;

    /// <summary>
    /// The maxmimum value for this Parameter.
    /// </summary>
    public int Maximum { get; set; } = int.MaxValue;

    /// <summary>
    /// The default value for this Parameter.
    /// </summary>
    public int Default { get; set; } = 0;

    public IntegerParameter()
      : base(typeof(int))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      Type valueType = value.GetType();
      int intValue = 0;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      // See if we can simply cast it to an int
      if (valueType.IsPrimitive && BaseType.IsAssignableFrom(valueType))
      {
        intValue = (int)value;
        valueOk = (intValue >= Minimum && intValue <= Maximum);
      }
      // Next try parsing it from string
      if (int.TryParse(value.ToString(), out intValue))
      {
        valueOk = (intValue >= Minimum && intValue <= Maximum);
      }

      parsedValue = (ParsedType)Convert.ChangeType(intValue, typeof(ParsedType));
      return valueOk;
    }
  }
}
