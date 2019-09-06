using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace DunGen.Infestation
{
  /// <summary>
  /// A library of infestations. Typically, the parent library contains everything compatable with
  /// a particular game system, and is then sub-divided by various labels.
  /// </summary>
  [DataContract(Name = "library", IsReference = true)]
  [KnownType("GetKnownTypes")]
  public class Library
  {
    /// <summary>
    /// Gets the known types needed to serialize any Infestation libraries.
    /// </summary>
    public static IEnumerable<Type> GetKnownTypes()
    {
      List<Type> knownTypes = new List<Type>();

      knownTypes.Add(typeof(InfestationInfo));
      knownTypes.Add(typeof(InfestationList));
      knownTypes.Add(typeof(Library));
      knownTypes.Add(typeof(InfestationType));
      knownTypes.Add(typeof(AssociationType));
      knownTypes.Add(typeof(HashSet<Label>));

      return knownTypes;
    }

    private InfestationList _allInfestations = new InfestationList();

    /// <summary>
    /// The name of this library, which can be used as a label in other libraries referencing it.
    /// </summary>
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "brief", Order = 1)]
    public string Brief { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "uri", Order = 2)]
    public string URI { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "labels", Order = 3)]
    public ISet<Label> Labels { get; private set; } = new HashSet<Label>();

    /// <summary>
    /// A single collection containing all infestations in the library.
    /// </summary>
    [DataMember(Name = "infestations", Order = 4)]
    public InfestationList AllInfestations
    {
      get => _allInfestations;
      private set => _allInfestations = new InfestationList(value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IEnumerable<InfestationInfo> GetInfestationsFor(InfestationType type)
    {
      return _allInfestations.Where(info => info.Category == type);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    public IEnumerable<InfestationInfo> GetInfestationsFor(Label label)
    {
      return _allInfestations.Where(info => info.Labels.ContainsKey(label));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="labels"></param>
    /// <returns></returns>
    public IEnumerable<InfestationInfo> GetInfestationsFor(IEnumerable<Label> labels)
    {
      return _allInfestations.Where(info => labels.All(l => info.Labels.ContainsKey(l)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newInfestation"></param>
    public void Add(InfestationInfo newInfestation)
    {
      if (_allInfestations.Contains(newInfestation))
      {
        return;
      }

      newInfestation.Parent = this;

      foreach (var l in newInfestation.Labels.Keys)
      {
        l.Parent = this;
        Labels.Add(l);
      }
    }

    public void Add(IEnumerable<InfestationInfo> newInfestations)
    {
      foreach (var info in newInfestations)
      {
        Add(info);
      }
    }
  }
}
