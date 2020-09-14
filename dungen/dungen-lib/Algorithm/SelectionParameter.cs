using System;
using System.Collections.Generic;

namespace DunGen.Algorithm
{
  public class SelectionParameter : Parameter
  {
    private object _default = null;
    private Type _selectionType = null;

    /// <summary>
    /// The Enumerated type of this Selection Parameter. NULL if unspecified.
    /// </summary>
    public Type SelectionType
    {
      get => _selectionType;
      set
      {
        if (value != null && !value.IsEnum)
        {
          throw new ArgumentException("Provided \"selection\" must be an Enumeration");
        }

        if (_default != null && _default.GetType() != value)
        {
          _default = null;
        }

        _selectionType = value;
      }
    }

    /// <summary>
    /// The default value for this Selection Parameter. NULL if unspecified.
    /// </summary>
    public object Default
    {
      get => _default;
      set
      {
        if (value != null && !value.GetType().IsEnum)
        {
          throw new ArgumentException("Provided \"Default\" value must be an Enumeration");
        }

        if (SelectionType != null && SelectionType != value.GetType())
        {
          throw new ArgumentException("\"Default\" must be a value of the provided Selection type");
        }

        _default = value;
      }
    }

    /// <summary>
    /// A list of available choices from the Selection type. If there are
    /// exceptions or values this Parameter does not support, this property
    /// will reflect those exceptions.
    /// </summary>
    public List<string> Choices
    {
      get
      {
        List<string> choices = new List<string>();
        foreach (var v in Enum.GetValues(SelectionType))
        {
          choices.Add(v.ToString());
        }
        return choices;
      }
    }

    public SelectionParameter()
      : base(typeof(Enum))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      object valueObj = null;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      if (value is string || value.GetType().IsEnum)
      {
        valueOk = Choices.Contains(value.ToString());
        if (!valueOk) return false;
        valueOk = Enum.TryParse(SelectionType, value.ToString(), out valueObj);
      }
      if (value.GetType().IsPrimitive)
      {
        valueOk = Enum.IsDefined(SelectionType, value);
        valueObj = Enum.ToObject(SelectionType, value);
      }

      parsedValue = (ParsedType)Convert.ChangeType(valueObj, typeof(ParsedType));

      return valueOk;
    }
  }
}
