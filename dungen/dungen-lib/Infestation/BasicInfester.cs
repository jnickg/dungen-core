using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DunGen.Algorithm;
using DunGen;
using DunGen.Tiles;

namespace DunGen.Infestation
{
  /// <summary>
  /// A boring, parameterless infestation algorithm. Simply adds one random
  /// <seealso cref="InfestationType.Item"/> to each <seealso cref="TileGroupInfo"/> that is
  /// associated with the given dungeon.
  /// </summary>
  public class BasicInfester : InfestationAlgorithmBase
  {
    /// <see cref="InfestationAlgorithmBase._runAlgorithm(IAlgorithmContext)"/>
    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      Library _l = context.L;
      Dungeon _d = context.D;
      Random _r  = context.R;

      // Add one thing to each group in the dungeon
      foreach (TileGroupInfo group in _d.Groups.Where(grp => context.Mask.ContainsAny(grp.Tiles)))
      {
        InfestationInfo randomItem = _l.AllInfestations.Where(info => info.Category == InfestationType.Item)
                                                       .Random(_r);
        _d.Infestations.Associate(group, randomItem);
      }
    }
  }
}
