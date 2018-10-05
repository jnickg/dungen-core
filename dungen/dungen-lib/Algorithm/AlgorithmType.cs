using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  /// <summary>
  /// An wrapper of System.Type that can be serialized, and loaded without the Type's
  /// assembly. Lazily attempts to retrieve the actual Type value.
  /// </summary>
  [DataContract(Name = "type")]
  public class AlgorithmType
  {
    [DataMember(Name = "assyQualName", IsRequired = true)]
    public string AssemblyQualifiedName { get; set; } = string.Empty;

    public AlgorithmType()
    { }

    public AlgorithmType(Type t)
    {
      this.AssemblyQualifiedName = t.AssemblyQualifiedName;
    }

    public AlgorithmType(string assemblyQualifiedName)
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

    static public implicit operator Type(AlgorithmType thisAlgType)
    {
      return thisAlgType.ConvertToType(false);
    }
    static public implicit operator AlgorithmType(Type t)
    {
      return new AlgorithmType(t);
    }

    public static bool operator ==(AlgorithmType a, AlgorithmType b)
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
    public static bool operator !=(AlgorithmType a, AlgorithmType b)
    {
      return !(a == b);
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

      AlgorithmType p = obj as AlgorithmType;
      if (null == (System.Object)p)
      {
        return false;
      }

      return (AssemblyQualifiedName == p.AssemblyQualifiedName);
    }
    public bool Equals(AlgorithmType p)
    {
      if ((object)p == null)
      {
        return false;
      }

      return (AssemblyQualifiedName == p.AssemblyQualifiedName);
    }
  }
}
