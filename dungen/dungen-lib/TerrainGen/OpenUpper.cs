using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class OpenUpper : ITerrainGenAlgorithm
  {
    public string Name
    {
      get
      {
        return "Open Upper";
      }
    }

    public TerrainModification Behavior
    {
      get
      {
        return TerrainModification.Carve;
      }
    }

    public OpenUpper()
    {
      // no op
    }

    public void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      d.SetAllToo(Tile.MoveType.Open_HORIZ, mask);
    }
  }
}
