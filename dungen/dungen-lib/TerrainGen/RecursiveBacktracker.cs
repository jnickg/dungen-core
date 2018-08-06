using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DunGen.TerrainGen
{
  public class RecursiveBacktracker : TerrainGenAlgorithmBase
  {
    public enum OpenTilesStrategy
    {
      Avoid,
      ConnectToRooms,
      Ignore
    }

    [SelectionAlgorithmParameterInfo(
      "How this algorithm should interact with existing open tiles, if there are any in its mask",
      typeof(OpenTilesStrategy),
      OpenTilesStrategy.Avoid)]
    public OpenTilesStrategy ExistingDataStrategy { get; set; }

    [BooleanAlgorithmParameterInfo(
      "Whether this algorithm should use entire tiles as walls (true) or just tile borders (false)",
      true)]
    public bool TilesAsWalls { get; set; }

    [IntegerAlgorithmParamInfo(
      "If using tiles as walls, how many tiles to pad the outside of the algorithm's mask",
      1,
      0,
      int.MaxValue)]
    public int BorderPadding { get; set; }

    // TODO make this thing work
    [DecimalAlgorithmParamInfo(
      "A 0.0 to 1.0 percentage factor of how likely the algorithm is to maintain its current direction",
      0.33,
      0.01,
      0.95,
      2)]
    public double Momentum { get; set; }

    public override TerrainModBehavior Behavior
    {
      get
      {
        return TerrainModBehavior.Carve;
      }
    }

    public override TerrainGenStyle Style
    {
      get
      {
        return TerrainGenStyle.Bldg_Halls;
      }
    }

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      List<bool[,]> masks = mask.SplitByAdjacency();
      foreach (var m in masks)
      {
        this.Generate_Priv(d, m, r);
        // Add the generated tiles to a group
        d.CreateGroup(m);
      }
    }

    private void Generate_Priv(DungeonTiles d, bool[,] mask, Random r)
    {
      // Below we assume mask and d are the same dimensions
      if (d.Height != mask.GetLength(0) || d.Width != mask.GetLength(1))
      {
        throw new ArgumentException("Invalid mask");
      }

      if (null == r) r = new Random();

      bool[,] isExplored = new bool[d.Height, d.Width];
      List<Point> unmaskedPoints = new List<Point>();
      for (int y = 0; y < isExplored.GetLength(0); ++y)
      {
        for (int x = 0; x < isExplored.GetLength(1); ++x)
        {
          isExplored[y, x] = false;
          if (this.ExistingDataStrategy == OpenTilesStrategy.Avoid &&
              d[y, x].Physics != Tile.MoveType.Wall)
          {
            isExplored[y, x] = true;
          }
          // If it's not masked out, add it to the pool of potential
          // starting points
          if (mask[y, x]) unmaskedPoints.Add(new Point(x, y));
        }
      }

      // Only odd coordinates are valid starts due to limitations of algorithm below
      Predicate<Point> oddPoints = p => p.X % 2 != 0 && p.Y % 2 != 0;
      Predicate<Point> paddedWithinBorder = p => p.X > BorderPadding && p.Y > BorderPadding && p.X < isExplored.GetLength(1) - BorderPadding && p.Y < isExplored.GetLength(0) - BorderPadding;
      List<Point> originPool = unmaskedPoints.FindAll(oddPoints).FindAll(paddedWithinBorder);

      if (originPool.Count == 0) return; // no op -- we can't run algorithm at all

      // Find an origin point from which to start
      Point origin = originPool[r.Next() % originPool.Count];

      // Launch into recursive algorithm
      if (this.TilesAsWalls)
      {
        this.RecursiveBacktrack_TilesAsWalls(d, origin.X, origin.Y, isExplored, mask, r);
      }
      else
      {
        throw new NotImplementedException("Doesn't yet support Boundaries as walls."); // TODO
      }
    }

    private enum Direction
    {
      N = 0,
      E,
      S,
      W
    }

    private void RecursiveBacktrack_TilesAsWalls(DungeonTiles d, int currentX, int currentY, bool[,] explored, bool[,] overallMask, Random r, Direction lastDirection = Direction.N)
    {
      if (null == explored || null == overallMask) return;
      if (currentX >= explored.GetLength(1) || currentY >= explored.GetLength(0) || currentX < 0 || currentY < 0) return;

      // Case 1: We have explored this tile, or it's not witn the mask bounds.
      if (!overallMask[currentY, currentX] || explored[currentY, currentX]) return;

      // Case 2: We are at an unexplored tile. Mark the tile as explored and carve it up
      explored[currentY, currentX] = true;
      d[currentY, currentX].Physics = d[currentY, currentX].Physics.OpenUp(Tile.MoveType.Open_HORIZ);

      // ...then proceed to explore its adjacent tiles
      List<Direction> untried = new List<Direction>()
      {
        Direction.N,
        Direction.E,
        Direction.S,
        Direction.W
      };

      do
      {
        // Pick a random direction to explore
        Direction dir = lastDirection;
        // Can use momentum
        if (untried.Contains(lastDirection))
        {
          List<Direction> candidates = new List<Direction>();
          for (int i = 0; i < (int)(Momentum * 100); ++i)
          {
            candidates.Add(lastDirection);
          }
          foreach(Direction nextDir in untried.Where(aDir => aDir != lastDirection))
          {
            for (int i = 0; i < (int)((1.0 - Momentum) * 33.3); ++i)
            {
              candidates.Add(nextDir);
            }
          }
          dir = candidates[r.Next(candidates.Count)];
        }
        else // Last direction already explored; pick randomly
        {
          dir = untried[r.Next(untried.Count)];
        }
        untried.Remove(dir);

        // We calculate adjacent and proceeding coordinate so that
        // turns only occur on every other tile, so that entire tiles
        // are used as the walls between hallways
        int x2, y2, x3, y3; // Adjacent and next coordinate
        switch (dir)
        {
          case Direction.N: // up
            x2 = currentX;
            y2 = currentY - 1;
            x3 = currentX;
            y3 = currentY - 2;
            break;
          case Direction.E: // right
            x2 = currentX + 1;
            y2 = currentY;
            x3 = currentX + 2;
            y3 = currentY;
            break;
          case Direction.S: // down
            x2 = currentX;
            y2 = currentY + 1;
            x3 = currentX;
            y3 = currentY + 2;
            break;
          case Direction.W: // left
            x2 = currentX - 1;
            y2 = currentY;
            x3 = currentX - 2;
            y3 = currentY;
            break;
          default: // Should never happen -- just to make compiler happy
            x3 = x2 = currentX;
            y3 = y2 = currentY;
            break;
        }
        if (x2 < BorderPadding || y2 < BorderPadding || x3 < BorderPadding || y3 < BorderPadding  // Out of bounds
          || x2 >= explored.GetLength(1) - BorderPadding || y2 >= explored.GetLength(0) - BorderPadding // Out of bounds
          || x3 >= explored.GetLength(1) - BorderPadding || y3 >= explored.GetLength(0) - BorderPadding// Out of bounds
          || !overallMask[y2, x2] || !overallMask[y3, x3]   // Not in mask
          || explored[y2, x2] || explored[y3, x3])      // Already visited
        {
          continue;
        }

        // Special case: we want to connect to the area due to our strategy, but do NOT want to recurse
        if (d[y3, x3].Physics != Tile.MoveType.Wall &&
          this.ExistingDataStrategy == OpenTilesStrategy.ConnectToRooms)
        {
          if (d.GetCategoriesFor(x3, y3).Contains(DungeonTiles.Category.Room))
          {
            explored[y2, x2] = true;
            d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            explored[y3, x3] = true;
            d[y3, x3].Physics = d[y3, x3].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            // That tile's group has been connected, so remove it from dependant
            d.DeCategorizeAll(x3, y3, DungeonTiles.Category.Room);
          }
          continue;
        }

        // Tiles as walls means we traverse an adjacent (x2,y2) tile,
        // before moving to the "next" tile (x3,y3). So, do that.
        explored[y2, x2] = true;
        d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(Tile.MoveType.Open_HORIZ);

        // R E C U R S E !
        this.RecursiveBacktrack_TilesAsWalls(d, x3, y3, explored, overallMask, r, dir);

      } while (untried.Count > 0);

      // Case 3: We have explored this tile and all tried recursing
      // into al of its adjacent tiles. Time to pop back up in the
      // stack, to explore a previous tile's adjacents
      return;
    }
  }
}
