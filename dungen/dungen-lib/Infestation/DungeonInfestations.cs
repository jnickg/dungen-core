using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DunGen.Tiles;

namespace DunGen.Infestation
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "infestationInfoList", ItemName = "infestationInfo", IsReference = true)]
  public class InfestationInfoList : List<InfestationInfo>
  {
  }

  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "groupInfestations", KeyName = "group", ItemName = "infestations", IsReference = true)]
  public class GroupInfestationInfos : Dictionary<TileGroupInfo, InfestationInfoList>
  {
    public void SafeAdd(TileGroupInfo k, InfestationInfo newElement)
    {
      if (k == null) return;
      if (newElement == null) return;

      InfestationInfoList valueCollection = null;
      if (!TryGetValue(k, out valueCollection))
      {
        Add(k, new InfestationInfoList());
      }

      this[k].Add(newElement);
    }
  }

  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "tileInfestations", KeyName = "tile", ItemName = "infestations", IsReference = true)]
  public class TileInfestationInfos : Dictionary<Tile, InfestationInfoList>
  {
    public void SafeAdd(Tile k, InfestationInfo newElement)
    {
      if (k == null) return;
      if (newElement == null) return;

      InfestationInfoList valueCollection = null;
      if (!TryGetValue(k, out valueCollection))
      {
        Add(k, new InfestationInfoList());
      }

      this[k].Add(newElement);
    }
  }


  [DataContract(Name = "dInfestations", IsReference = true)]
  public class DungeonInfestations
  {
    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "generalInfestations", Order = 0)]
    public InfestationInfoList OverallInfestations { get; set; } = new InfestationInfoList();

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "groupInfestations", Order = 1)]
    public GroupInfestationInfos GroupInfestations { get; set; } = new GroupInfestationInfos();

    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name = "tileInfestations", Order = 2)]
    public TileInfestationInfos TileInfestations { get; set; } = new TileInfestationInfos();

    public void Associate(InfestationInfo newElement)
    {
      OverallInfestations.Add(newElement);
    }

    public void Associate(TileGroupInfo k, InfestationInfo newElement)
    {
      GroupInfestations.SafeAdd(k, newElement);
    }

    public void Associate(Tile k, InfestationInfo newElement)
    {
      TileInfestations.SafeAdd(k, newElement);
    }
  }
}
