using System.Reflection;
using System.Runtime.Serialization;

namespace DunGen.Algorithm
{
  /// <summary>
  /// A serializable reference to an Algorithm type's property
  /// </summary>
  [DataContract(Name = "algPropRef")]
  public class AlgorithmPropertyReference
  {
    private SerializableType _algorithmType = null;
    private string _propertyName = string.Empty;  

    [DataMember(Name = "algType", IsRequired = true, Order = 0)]
    public SerializableType AlgorithmType
    {
      get => _algorithmType;
      set => _algorithmType = value;
    }

    [DataMember(Name = "propName", IsRequired = true, Order = 1)]
    public string PropertyName
    {
      get => _propertyName;
      set => _propertyName = value;
    }

    public PropertyInfo Info
    {
      get => AlgorithmType.ConvertToType(true).GetMatchingPropertyFor(PropertyName);
    }

    public bool IsParam
    {
      get => Info.GetParameter() != null;
    }

    public Parameter Param
    {
      get => Info.GetParameter();
    }
  }
}
