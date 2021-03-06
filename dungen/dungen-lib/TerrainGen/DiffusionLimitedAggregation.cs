﻿using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DunGen.TerrainGen
{
  public class DiffusionLimitedAggregation : TerrainGenAlgorithmBase
  {
    private const string DensityFactor_Help =
      "A 0.0–1.0 decimal percentage, representing the density " +
      "of open-Tile aggregation. Values between 0.1% and 50% allowed.";

    /// <summary>
    /// See Help text.
    /// </summary>
    [DecimalParameter(
      Description = DensityFactor_Help,
      Default = 0.250,
      Minimum = 0.001,
      Maximum = 0.500)]
    public double DensityFactor { get; set; }

    /// <summary>
    /// Disabled for DiffusionLimitedAggregation.
    /// <see cref="TerrainGenAlgorithmBase.WallStrategy"/>
    /// </summary>
    [SelectionParameter(
      Description = WallStrategy_Help,
      SelectionType = typeof(WallFormation),
      Default = WallFormation.Tiles,
      Supported = false)]
    public override WallFormation WallStrategy { get; set; }

    /// <summary>
    /// Disabled for DiffusionLimitedAggregation.
    /// <see cref="TerrainGenAlgorithmBase.OpenTilesStrategy"/>
    /// </summary>
    [SelectionParameter(
      Description = OpenTilesStrategy_Help,
      SelectionType = typeof(OpenTilesHandling),
      Default = OpenTilesHandling.Overwrite,
      Supported = false)]
    public override OpenTilesHandling OpenTilesStrategy { get; set; }

    public override TerrainModBehavior Behavior
    {
      get => TerrainModBehavior.Clobber;
    }

    public override TerrainGenStyle Style
    {
      get => TerrainGenStyle.Cave;
    }

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      List<bool[,]> masks = context.Mask.SplitByAdjacency();
      foreach (var m in masks)
      {
        this.Generate_Priv(context.D.Tiles, m, context.R, context);
        // Add the generated tiles to a group
        context.D.CreateGroup(m, TileCategory.Room);
      }
    }

    private void Generate_Priv(DungeonTiles d, bool[,] mask, Random r, IAlgorithmContext context)
    {
      // if (ExistingDataStrategy != OpenTilesStrategy.Overwrite) throw new NotSupportedException("DLA can only overwrite existing data");
      // if (WallStyle != WallFormationStyle.Tiles) throw new NotSupportedException("DLA can only use entire tiles for walls.");

      d.SetAllToo(Tile.MoveType.Wall, mask);

      bool[,] map = new bool[d.Height, d.Width];

      // find center of mask mass and generate list of boundary points
      double xSum = 0,
             ySum = 0;
      int maskCount = 0;
      int[,] boundary = new int[mask.GetLength(0) * mask.GetLength(1), 2];
      int boundaryCount = 0;
      for (int i = 0; i < mask.GetLength(0); ++i)
      {
        for (int j = 0; j < mask.GetLength(1); ++j)
        {
          if (mask[i, j])
          {
            map[i, j] = false;
            xSum += i;
            ySum += j;
            maskCount++;
            if ((i == 0 || !mask[i - 1, j]) ||
                (j == 0 || !mask[i, j - 1]) ||
                (i == mask.GetLength(0) - 1 || !mask[i + 1, j]) ||
                (j == mask.GetLength(1) - 1 || !mask[i, j + 1]))
            {
              // this is on the boundary
              boundary[boundaryCount, 0] = i;
              boundary[boundaryCount++, 1] = j;
            }
          }
        }
      }
      int[] com = { (int)(xSum / maskCount), (int)(ySum / maskCount) }; // center of mass
      if (mask[com[0], com[1]])
      {
        // this location is valid, so we're all good
        d[com[1], com[0]].Physics = Tile.MoveType.Open_HORIZ;
        map[com[0], com[1]] = true;
        this.RunCallbacks(context);
      }
      else
      {
        // location is not valid so just pick a random point
        int st = r.Next(maskCount);
        int count = 0;
        for (int i = 0; i < mask.GetLength(0); ++i)
        {
          for (int j = 0; j < mask.GetLength(1); ++j)
          {
            if (mask[i, j])
            {
              if (count == st)
              {
                d[j, i].Physics = Tile.MoveType.Open_HORIZ;
                map[i, j] = true;
                this.RunCallbacks(context);
                i = mask.GetLength(0); // setting this so it breaks out of i loop as well
                break;
              }
              count++;
            }
          }
        }
      }

      int numIterations = (int)(maskCount * DensityFactor);
      Console.WriteLine("numIterations: {0}", numIterations);
      int start, tx, ty;
      for (int i = 0; i < numIterations; ++i)
      {
        // pick random boundary point
        start = r.Next(boundaryCount);
        tx = boundary[start, 0];
        ty = boundary[start, 1];
        while (hasNoTrueNeighbors(tx, ty, map, mask))
        {
          int dir = r.Next(4);
          switch (dir)
          {
            case 0: // up
              if (ty > 0 && mask[tx, ty - 1])
              {
                ty--;
              }
              break;
            case 1: // right
              if (tx < mask.GetLength(0) - 1 && mask[tx + 1, ty])
              {
                tx++;
              }
              break;
            case 2: // down
              if (ty < mask.GetLength(1) - 1 && mask[tx, ty + 1])
              {
                ty++;
              }
              break;
            case 3: // left
              if (tx > 0 && mask[tx - 1, ty])
              {
                tx--;
              }
              break;
          }
        }
        map[tx, ty] = true;
        d[ty, tx].Physics = Tile.MoveType.Open_HORIZ;
        this.RunCallbacks(context);
      }

      d.SetAllToo(Tile.MoveType.Open_HORIZ, map);
    }

    private bool hasNoTrueNeighbors(int tx, int ty, bool[,] map, bool[,] mask)
    {
      // check up
      if (ty > 0 && mask[tx, ty - 1] && map[tx, ty - 1])
      {
        return false;
      }
      if (tx > 0 && mask[tx - 1, ty] && map[tx - 1, ty])
      {
        return false;
      }
      if (ty < mask.GetLength(1) - 1 && mask[tx, ty + 1] && map[tx, ty + 1])
      {
        return false;
      }
      if (tx < mask.GetLength(0) - 1 && mask[tx + 1, ty] && map[tx + 1, ty])
      {
        return false;
      }
      return true;
    }
  }
}
