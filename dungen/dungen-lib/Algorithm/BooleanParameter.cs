using System;

namespace DunGen.Algorithm
{
  public class BooleanParameter : Parameter
  {
    /// <summary>
    /// The default boolean value for this parameter.
    /// </summary>
    public bool Default { get; set; }

    public BooleanParameter()
      : base(typeof(bool))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      bool valBool = false;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (valueOk) return true;

      if (value.GetType().IsPrimitive)
      {
        valBool = (bool)value;
        valueOk = true;
      }
      if (value is string)
      {
        valueOk = Boolean.TryParse(value.ToString(), out valBool);
      }

      parsedValue = (ParsedType)Convert.ChangeType(valBool, typeof(ParsedType));

      return valueOk;
    }
  }
}
