using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  /// <summary>
  /// An wrapper of System.Type that can be serialized, and loaded without the Type's
  /// assembly. Lazily attempts to retrieve the actual Type value.
  /// </summary>
  [DataContract(Name = "type")]
  public class SerializableType
  {
    [DataMember(Name = "assyQualName", IsRequired = true)]
    public string AssemblyQualifiedName { get; set; } = string.Empty;

    [DataMember(Name = "displayName")]
    public string DisplayName => GetDisplayName(this);

    public SerializableType()
    { }

    public SerializableType(Type t)
    {
      this.AssemblyQualifiedName = t.AssemblyQualifiedName;
    }

    public SerializableType(string assemblyQualifiedName)
    {
      this.AssemblyQualifiedName = assemblyQualifiedName;
    }

    public Type ConvertToType(bool throwOnFail = false)
    {
      return Type.GetType(this.AssemblyQualifiedName, throwOnFail);
    }

    public bool IsTypeLoaded()
    {
      return null != ConvertToType();
    }

    static public implicit operator Type(SerializableType thisAlgType)
    {
      return thisAlgType.ConvertToType(false);
    }
    static public implicit operator SerializableType(Type t)
    {
      return new SerializableType(t);
    }

    public static bool operator ==(SerializableType a, SerializableType b)
    {
      // If both are null, or both are same instance, return true.
      if (System.Object.ReferenceEquals(a, b))
      {
        return true;
      }

      if (((object)a == null) || ((object)b == null))
      {
        return false;
      }

      // Return true if the fields match:
      return a.AssemblyQualifiedName == b.AssemblyQualifiedName;
    }
    public static bool operator !=(SerializableType a, SerializableType b)
    {
      return !(a == b);
    }

    public static string GetDisplayName(SerializableType t)
    {
      Type actualType = t;
      if (actualType == null) return "Object";
      switch (actualType.Name)
      {
        case "Double":
          return "number";
        case "Int32":
          return "integer";
        case "Boolean":
          return "boolean";
        case "Enum":
          return "Array";
        case "IAlgorithm":
          return "Algorithm";
        default:
          return "Object";
      }
    }

    public override int GetHashCode()
    {
      int hashCode = 0;
      if (null != ConvertToType())
      {
        hashCode = ConvertToType().GetHashCode();
      }
      else
      {
        hashCode = base.GetHashCode();
      }
      return hashCode;
    }

    public override bool Equals(System.Object obj)
    {
      if (obj == null)
      {
        return false;
      }

      Type typeObj = obj as Type;
      if (null != typeObj)
      {
        return this.AssemblyQualifiedName == typeObj.AssemblyQualifiedName;
      }

      SerializableType p = obj as SerializableType;
      if (null == (System.Object)p)
      {
        return false;
      }

      return (AssemblyQualifiedName == p.AssemblyQualifiedName);
    }
    public bool Equals(SerializableType p)
    {
      if ((object)p == null)
      {
        return false;
      }

      return (AssemblyQualifiedName == p.AssemblyQualifiedName);
    }
  }
}
