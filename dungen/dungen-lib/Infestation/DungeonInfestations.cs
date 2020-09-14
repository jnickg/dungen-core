using System;
using System.Text;
using System.Runtime.Serialization;
using DunGen.Tiles;

namespace DunGen.Infestation
{
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
