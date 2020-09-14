using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DunGen.Algorithm
{
  /// <summary>
  /// An attribute tag used to mark which properties of an IAlgorithmParameter instance
  /// are to be considered modifiable parameters of the algorithm.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public abstract class Parameter : System.Attribute
  {
    /// <summary>
    /// The type of Parameter connected to this Parameter
    /// </summary>
    public Type BaseType { get; protected set; } = null;

    /// <summary>
    /// The relative ordering of this Attribute, relative to others, when
    /// multiple AlgorithmParameterInfo attributes are applied to a single
    /// Algorithm Property. If multiple attributes are applied to a non-
    /// composite Algorithm Property (i.e. a basic type), the lowest-ordered
    /// valid AlgorithmParameterInfo wil lbe used. If used on a composite
    /// Algorithm Property, this will determine the order in which the
    /// values appear.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// If not NULL or empty, a human-readable description of this Parameter.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this Parameter is supported by the Algorithm implementation.
    /// Defaults to true, but may be set to false by Algorithms that don't
    /// support inherited Parameters (i.e. by overriding the Parameter and
    /// defining a new AlgorithmParameterInfo for the Parameter).
    /// 
    /// If false, this parameter will still appear in the Algorithm's
    /// "Parameters" property.
    /// </summary>
    public bool Supported { get; set; } = true;

    /// <summary>
    /// Whether to show this Parameter. Can be set to false for common
    /// Parameters not worth showing repeatedly, or for super secret
    /// hidden Easter-egg Parameters.
    /// </summary>
    public bool Show { get; set; } = true;

    /// <summary>
    /// Creates this Parameter with the specified Type. The type is used by clients
    /// to determine which interface to provide, when editing the parameter.
    /// </summary>
    /// <param name="paramType">The elementary type of this Parameter.</param>
    public Parameter(Type paramType)
    {
      this.BaseType = paramType;
    }

    /// <summary>
    /// Gets the appropriate default value for this Parameter. This can be a primitive
    /// value or an instance of an object, depending on the Parameter's type.
    /// </summary>
    public abstract object GetDefault();

    public virtual bool TryApplyValue(IEditableParameter source, IAlgorithm destination)
    {
      if (destination == null || source == null) throw new ArgumentNullException();

      PropertyInfo matchingProperty = destination.GetMatchingPropertyFor(source);
      object parsedValue;
      if (!TryParseValue(source.Value, out parsedValue))
      {
        return false;
      }

      matchingProperty.SetValue(destination, parsedValue);
      return true;
    }

    public virtual bool TryParseValue<ParsedType>(IAlgorithm source, string propertyName, out ParsedType parsedValue)
    {
      PropertyInfo prop = source.GetMatchingPropertyFor(propertyName);
      return TryParseValue(source, prop, out parsedValue);
    }

    public bool TryParseValue<ParsedType>(IAlgorithm source, PropertyInfo prop, out ParsedType parsedValue)
    {
      object sourceVal = prop.GetValue(source);
      return TryParseValue(sourceVal, out parsedValue);
    }

    public bool TryParseValue<ParsedType>(IEditableParameter source, out ParsedType parsedValue)
    {
      return TryParseValue(source.Value, out parsedValue);
    }

    public virtual bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      Type valueType = value.GetType();
      Type baseType = BaseType;
      Type typeToParse = typeof(ParsedType);

      parsedValue = default(ParsedType);

      // Check if we won't be able to produce the requested type.
      if (typeToParse.IsAssignableFrom(baseType))
      {
        parsedValue = (ParsedType)value;
        return true;
      }

      return false;
    }

    /// <summary>
    /// Gets a collection of all supported Parameter Types, which the client should
    /// be able to handle editing.
    /// </summary>
    public static IEnumerable<Type> GetParamTypes()
    {
      List<Type> paramTypes = new List<Type>()
      {
        typeof(int),
        typeof(double),
        typeof(bool),
        typeof(IAlgorithm),
        typeof(Enum)
      };

      return paramTypes;
    }

    /// <summary>
    /// Gets a collection of all known types that have Parameter tags associated
    /// with them, for the purposes of serialization.
    /// </summary>
    public static IEnumerable<Type> GetKnownTypes()
    {
      List<Type> knownTypes = new List<Type>()
      {
        typeof(int),                      // IntegerAlgorithmParamInfo
        typeof(double),                   // DecimalAlgorithmParamInfo
        typeof(bool),                     // BooleanAlgorithmParamInfo
        typeof(SerializableType),         // AlgorithmAlgorithmParamInfo
      };

      // Reflect through every Algorithm type loaded, to identify all
      // enumerations we need to know about for SelectionAlgorithmParameterInfo
      foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
      {
        // Linq is so handy but seriously it's garbage to debug those nested .Where().Select() calls
        Type[] types = assy.GetTypes();
        // Of all the loaded Algorithms...
        types = types.Where(t => typeof(IAlgorithm).IsAssignableFrom(t)).ToArray();
        IEnumerable<PropertyInfo> props = types.SelectMany(t => t.GetProperties().AsEnumerable());
        // ... We want Enum-based properties that are marked as an Algorithm Parameter
        props = props.Where(p => p.PropertyType.IsEnum &&
                                 p.PropertyType.GetCustomAttributes<Parameter>(true) != null);
        knownTypes.AddRange(props.Select(p => p.PropertyType));
      }

      // Add all known algorithms so we can handle composite algorithms,
      // or algorithms taking other algs as parameters.
      knownTypes.AddRange(AlgorithmBase.GetKnownTypes());

      // Transform it to a set and back, to remove duplicates
      return knownTypes.Distinct().ToList();
    }
  }
}
