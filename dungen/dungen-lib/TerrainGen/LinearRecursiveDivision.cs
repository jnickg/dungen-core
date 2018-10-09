using DunGen.Algorithm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class LinearRecursiveDivision : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => GetCurrentBehavior();

    public override TerrainGenStyle Style => GetCurrentStyle();

    private const string _BuildStrategyDescription =
      "How this algorithm should interact with existing data in tiles, if " +
      "there are any in its mask";

    private const string _RoomSizeDescription =
      "The size at which subregions should no longer be divided. Set to " +
      "\'1\' to create only corridors. This is an Area measurement.";

    private const string _VaribilityDescription =
      "A 0 to 1.0 percentage of variability in where subdivisions are made, " +
      "relative to the center of the region being split, where 1.0 means 100% " +
      "of the region's breadth can be used, and 0.0 means the subdivision will " +
      "be created as close to the center as possible.";

    public enum ExistingDataHandling
    {
      Avoid,      // Attempts to create a maze without writing building walls where data already exists
      Ignore,     // Creates a maze by constructing walls, even if data exists.
      Overwrite   // Clobbers all existing data within mask, before generating a maze
    }

    [SelectionParameter(
      Description = _BuildStrategyDescription,
      SelectionType = typeof(ExistingDataHandling),
      Default = ExistingDataHandling.Avoid)]
    public ExistingDataHandling BuildStrategy { get; set; }



    [IntegerParameter(
      Description = _RoomSizeDescription,
      Default = 1,
      Minimum = 1,
      Maximum = int.MaxValue)]
    public int RoomSize { get; set; }

    [DecimalParameter(
      Description = _VaribilityDescription,
      Default = 1.0,
      Minimum = 0.0,
      Maximum = 1.0,
      PrecisionPoints = 5)]
    public double Variability { get; set; }

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      if (this.WallStrategy == WallFormation.Tiles) throw new NotImplementedException();
      if (null == r) r = new Random();

      bool[,] existingDataMask = new bool[d.Height, d.Width];

      for (int y = 0; y < d.Height; ++y)
      {
        for (int x = 0; x < d.Width; ++x)
        {
          existingDataMask[y, x] = mask[y, x] && (d[y, x].Physics == Tile.MoveType.Wall);
        }
      }

      // Prime the dungeon tiles by opening them all up (as appropriate).
      // "Ignore" shouldn't wipe existing data, but...
      switch (BuildStrategy)
      {
        case ExistingDataHandling.Ignore:
        case ExistingDataHandling.Avoid:
          d.SetAllToo(Tile.MoveType.Open_HORIZ, existingDataMask);
          break;
        case ExistingDataHandling.Overwrite:
          d.SetAllToo(Tile.MoveType.Open_HORIZ, mask);
          break;
        default:
          throw new NotImplementedException();
      }

      // Run algorithm with the appropriate mask
      // ... "Ignore" SHOULD build walls over existing data
      Rectangle startRegion = new Rectangle(0, 0, d.Width, d.Height);
      switch (BuildStrategy)
      {
        case ExistingDataHandling.Avoid:
          this.Divide(d, startRegion, existingDataMask, mask, r);
          break;
        case ExistingDataHandling.Ignore:
        case ExistingDataHandling.Overwrite:
          this.Divide(d, startRegion, mask, mask, r);
          break;
        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Selets orientation to sub-divide. TRUE means horizontal; FALSE means vertical.
    /// </summary>
    private bool DoUseHorizontalOrientation(int w, int h, Random r)
    {
      if (w < h) return true;
      else if (h < w) return false;
      else return 1 == (r.Next() % 2);
    }

    /// <summary>
    /// Gets what direction(s) need to be closed off given the current
    /// algorithm parameters, and the specified direction
    /// </summary>
    private Tile.MoveType DetermineClosureMethod(bool forHorizontalDivide)
    {
      if (this.WallStrategy == WallFormation.Tiles) return Tile.MoveType.Open_HORIZ;

      if (forHorizontalDivide)
      {
        return Tile.MoveType.Open_SOUTH;
      }
      else
      {
        return Tile.MoveType.Open_EAST;
      }
    }

    private void Divide(DungeonTiles d, Rectangle topRegion, bool[,] totalMask, bool[,] algMask, Random r)
    {
      if (null == d || null == topRegion || null == totalMask) return;
      Stack<Rectangle> subregions = new Stack<Rectangle>();
      subregions.Push(topRegion);
      if (null == r) r = new Random();

      while (subregions.Count > 0)
      {
        Rectangle currentRegion = subregions.Pop();

        if (currentRegion.Width == 1 || currentRegion.Height == 1) continue;
        if (currentRegion.Width * currentRegion.Height <= this.RoomSize)
        {
          d.Categorize(currentRegion, DungeonTiles.Category.Room);
          if (this.GroupRooms)
          {
            d.CreateGroup(currentRegion);
          }
          continue;
        }

        bool doHoriz = DoUseHorizontalOrientation(currentRegion.Width, currentRegion.Height, r);

        // Origin of new wall - TODO is there more efficient math for this?
        int wx_offset_base = (int)((1.0 - Variability) / 2.0 * currentRegion.Width);
        int wy_offset_base = (int)((1.0 - Variability) / 2.0 * currentRegion.Height);
        int wx_offset_rand = ((int)Math.Ceiling(Variability * r.NextDouble() * currentRegion.Width)) - 1;
        int wy_offset_rand = ((int)Math.Ceiling(Variability * r.NextDouble() * currentRegion.Height)) - 1;
        int wx_offset = wx_offset_base + wx_offset_rand;
        int wy_offset = wy_offset_base + wy_offset_rand;
        int wx = currentRegion.X + (doHoriz ? 0 : wx_offset);
        int wy = currentRegion.Y + (doHoriz ? wy_offset : 0);

        // Directionality
        int dx = doHoriz ? 1 : 0;
        int dy = doHoriz ? 0 : 1;

        int len = doHoriz ? currentRegion.Width : currentRegion.Height;

        // Break new wall into as many sub-walls as are appropriate (based on mask)
        HashSet<List<Tile>> subWalls = new HashSet<List<Tile>>();
        List<Tile> currentSubWall = new List<Tile>();
        for (int i = 0; i < len; ++i, wx += dx, wy += dy)
        {
          if (algMask[wy,wx] && (totalMask[wy, wx] || this.BuildStrategy == ExistingDataHandling.Ignore))
          {
            currentSubWall.Add(d[wy, wx]);
          }
          else
          {
            if (currentSubWall.Count > 0)
            {
              subWalls.Add(currentSubWall);
              currentSubWall = new List<Tile>();
            }
            continue;
          }
        }
        subWalls.Add(currentSubWall);

        Tile.MoveType newWalls = DetermineClosureMethod(doHoriz);

        // Build division wall, honoring mask & leaving a passage
        foreach (List<Tile> wall in subWalls)
        {
          int passageIdx = r.Next(0, wall.Count);
          for (int i = 0; i < wall.Count; ++i)
          {
            if (i == passageIdx) continue;
            wall[i].Physics = wall[i].Physics.CloseOff(newWalls);
          }
        }

        // Now subdivide the two new regions
        Rectangle subregion1 = new Rectangle(
          currentRegion.X,
          currentRegion.Y,
          doHoriz ? currentRegion.Width : wx - currentRegion.X + 1,
          doHoriz ? wy - currentRegion.Y + 1 : currentRegion.Height);
        Rectangle subregion2 = new Rectangle(
          doHoriz ? currentRegion.X : wx + 1,
          doHoriz ? wy + 1 : currentRegion.Y,
          doHoriz ? currentRegion.Width : currentRegion.X + currentRegion.Width - wx - 1,
          doHoriz ? currentRegion.Y + currentRegion.Height - wy - 1 : currentRegion.Height);

        // Push the new subregions ont othe stack
        subregions.Push(subregion1);
        subregions.Push(subregion2);

        if (this.GroupForDebug)
        {
          d.CreateGroup(subregion1);
          d.CreateGroup(subregion2);
        }

        this.RunCallbacks(d);
      }
    }

    private TerrainModBehavior GetCurrentBehavior()
    {
      return BuildStrategy == ExistingDataHandling.Overwrite ?
        TerrainModBehavior.Clobber :
        TerrainModBehavior.Build;
    }

    private TerrainGenStyle GetCurrentStyle()
    {
      return RoomSize > 0 ?
        TerrainGenStyle.Bldg :
        TerrainGenStyle.Bldg_Halls;
    }
  }
}
