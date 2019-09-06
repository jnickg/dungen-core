using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DunGen.TerrainGen
{
  public class BlobRecursiveDivision : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => TerrainModBehavior.Clobber;

    public override TerrainGenStyle Style => TerrainGenStyle.Cave;

    private const string _RoomSizeDescription =
      "The size at which subregions should no longer be divided. Set to " +
      "\'1\' to create only corridors. This is an Area measurement.";

    private const string _GapCountDescription =
      "How many gaps to leave in new boundaries. Algorithm uses the lesser of " +
      "this value and the the total length of the wall times MaxGapProportion";

    private const string _MaxGapProportionDescription =
      "The maximum proportion of a total boundary generated that can be open " +
      "gaps. Algorithm uses the lesser of this percentage, and GapCount gaps";

    [IntegerParameter(
      Description = _RoomSizeDescription,
      Default = 1,
      Minimum = 1,
      Maximum = int.MaxValue)]
    public int RoomSize { get; set; }

    [IntegerParameter(
      Description = _GapCountDescription,
      Default = 1,
      Minimum = 1,
      Maximum = int.MaxValue)]
    public int GapCount { get; set; }

    [DecimalParameter(
      Description = _MaxGapProportionDescription,
      Default = 0.05,
      Minimum = 0.01,
      Maximum = 0.99,
      Precision = 2)]
    public double MaxGapProportion { get; set; }

    private class Subregion
    {
      public IList<Tile> Tiles { get; set; }

      public Subregion(IEnumerable<Tile> tiles)
      {
        this.Tiles = new List<Tile>(tiles);
      }
    }

    private enum Subregion_Split
    {
      NONE = 0,
      A    = 1,
      B    = 2
    }

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      DungeonTiles workingTiles = context.D.Tiles;

      // Implemented via http://weblog.jamisbuck.org/2015/1/15/better-recursive-division-algorithm

      if (this.WallStrategy != WallFormation.Boundaries) throw new NotImplementedException();

      // CLOBBER!
      workingTiles.SetAllToo(Tile.MoveType.Open_HORIZ, context.Mask);

      Stack<Subregion> subregions = new Stack<Subregion>();

      // 1. Collect all the cells in the maze into a single region.
      Subregion topRegion = new Subregion(workingTiles.GetTilesIn(context.Mask));
      subregions.Push(topRegion);

      while (subregions.Count > 0)
      {
        Subregion parentRegion = subregions.Pop();

        if (parentRegion.Tiles.Count <= RoomSize)
        {
          if (RoomSize > 1)
          {
            if (this.GroupRooms)
            {
              workingTiles.Parent.CreateGroup(parentRegion.Tiles.ToHashSet(), TileCategory.Room);
            }
          }
          continue;
        }

        // 2. Split the region into two, using the following process:
        List<Tile> S = new List<Tile>();
        Dictionary<Tile, Subregion_Split> tileSubregions = new Dictionary<Tile, Subregion_Split>();
        foreach (Tile t in parentRegion.Tiles)
        {
          tileSubregions[t] = Subregion_Split.NONE;
        }

        //   2.1  Choose two cells from the region at random as “seeds”.
        //        Identify one as subregion A and one as subregion B.
        //        Then put them into a set S.
        Tile randomSeed_A = parentRegion.Tiles.PickRandomly(context.R);
        Tile randomSeed_B = randomSeed_A; // Shouldn't be equal when we're done
        while (randomSeed_A == randomSeed_B)
        {
          randomSeed_B = parentRegion.Tiles.PickRandomly(context.R);
        }
        tileSubregions[randomSeed_A] = Subregion_Split.A;
        tileSubregions[randomSeed_B] = Subregion_Split.B;
        S.Add(randomSeed_A);
        S.Add(randomSeed_B);

        while (S.Count > 0)
        {
          //   2.2  Choose a cell at random from S. Remove it from the set.
          Tile currentTile = S.PullRandomly(context.R);
          
          //   2.3  For each of that cell’s neighbors, if the neighbor
          //        is not already associated with a subregion, add it to S,
          //        and associate it with the same subregion as the cell itself.
          IList<Tile> nextAdjacents = currentTile
            .GetAdjacents()
            .Where(t => parentRegion.Tiles.Contains(t))
            .ToList();
          foreach (Tile t in nextAdjacents)
          {
            if (tileSubregions[t] == Subregion_Split.NONE)
            {
              S.Add(t);
              tileSubregions[t] = tileSubregions[currentTile];
            }
          }
        } //   2.4  Repeat 2.2 and 2.3 until the entire region has been split into two.

        // 3. Construct a wall between the two regions by identifying cells
        //    in one region that have neighbors in the other region. Leave a
        //    gap by omitting the wall from one such cell pair.
        List<Tuple<Tile, Tile>> wallBoundaries = new List<Tuple<Tile, Tile>>();

        wallBoundaries.AddRange(
          tileSubregions.Keys
            // Safe to filter with hard-coded A/B due to associative property
            .Where(k => tileSubregions[k] == Subregion_Split.A)
            .SelectMany<Tile, Tuple<Tile, Tile>>(
              t => t.GetAdjacents()
                    .Where(adj => parentRegion.Tiles.Contains(adj))
                    .Where(sr => tileSubregions[sr] == Subregion_Split.B)
                    .Select(t2 => new Tuple<Tile, Tile>(t, t2))));

        // Leave as many gaps opened as requested
        int maxGaps = Math.Min(
          this.GapCount, 
          (int)Math.Ceiling(wallBoundaries.Count * MaxGapProportion));
        for (int i = 0; i < maxGaps; ++i)
        {
          wallBoundaries.PullRandomly(context.R);
        }

        foreach (Tuple<Tile, Tile> pair in wallBoundaries)
        {
          Tile.MoveType touchingBoundary = workingTiles.GetCardinality(pair.Item1, pair.Item2);
          pair.Item1.Physics = pair.Item1.Physics.CloseOff(touchingBoundary);
        }

        ISet<Tile> subregion_A = parentRegion.Tiles.Where(t => tileSubregions[t] == Subregion_Split.A).ToHashSet();
        ISet<Tile> subregion_B = parentRegion.Tiles.Where(t => tileSubregions[t] == Subregion_Split.B).ToHashSet();

        if (this.GroupForDebug)
        {
          workingTiles.Parent.CreateGroup(subregion_A);
          workingTiles.Parent.CreateGroup(subregion_B);
        }

        this.RunCallbacks(context);

        // Push new subregions to the stack
        subregions.Push(new Subregion(subregion_A));
        subregions.Push(new Subregion(subregion_B));
      } // 4. Repeat 2 and 3 for each subregion
    }
  }
}
