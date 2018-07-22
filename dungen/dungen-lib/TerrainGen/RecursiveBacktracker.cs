using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class RecursiveBacktracker : ITerrainGenAlgorithm
  {
    #region Private Members
    private int PROPERTY_borderPadding;
    #endregion

    public bool MaskOpenTiles { get; set; } = true;
    public bool TilesAsWalls { get; set; } = true;
    public int BorderPadding
    {
      get
      {
        return PROPERTY_borderPadding;
      }
      set
      {
        // Retain int ness for simplicity, but ensure it's positive
        PROPERTY_borderPadding = Math.Clamp(value, 0, Int32.MaxValue);
      }
    }

    public string Name
    {
      get
      {
        return "RecursiveBacktracker";
      }
    }

    public TerrainModification Behavior
    {
      get
      {
        return TerrainModification.Carve;
      }
    }

    public RecursiveBacktracker()
    {
      this.BorderPadding = 1;
    }

    public void Run(DungeonTiles d, bool[,] mask, Random r)
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

      bool[,] exploredMap = new bool[d.Height, d.Width];
      List<Point> unmaskedPoints = new List<Point>();
      for (int y = 0; y < exploredMap.GetLength(0); ++y)
      {
        for (int x = 0; x < exploredMap.GetLength(1); ++x)
        {
          exploredMap[y, x] = false;
          if (this.MaskOpenTiles &&
              d[y, x].Physics != Tile.MoveType.Wall)
          {
            exploredMap[y, x] = true;
          }
          // If it's not masked out, add it to the pool of potential
          // starting points
          if (mask[y, x]) unmaskedPoints.Add(new Point(x, y));
        }
      }

      // Only odd coordinates are valid starts due to limitations of algorithm below
      Predicate<Point> oddPoints = p => p.X % 2 != 0 && p.Y % 2 != 0;
      Predicate<Point> paddedWithinBorder = p => p.X > BorderPadding && p.Y > BorderPadding && p.X < exploredMap.GetLength(1) - BorderPadding && p.Y < exploredMap.GetLength(0) - BorderPadding;
      List<Point> originPool = unmaskedPoints.FindAll(oddPoints).FindAll(paddedWithinBorder);

      if (originPool.Count == 0) return; // no op -- we can't run algorithm at all

      // Find an origin point from which to start
      Point origin = originPool[r.Next() % originPool.Count];

      // Launch into recursive algorithm
      if (this.TilesAsWalls)
      {
        this.RecursiveBacktrack_TilesAsWalls(d, origin.X, origin.Y, exploredMap, mask, r);
      }
      else
      {
        throw new NotImplementedException("Doesn't yet support Boundaries as walls."); // TODO
      }
    }

    private void RecursiveBacktrack_TilesAsWalls(DungeonTiles d, int currentX, int currentY, bool[,] explored, bool[,] overallMask, Random r)
    {
      if (null == explored || null == overallMask) return;
      if (currentX >= explored.GetLength(1) || currentY >= explored.GetLength(0) || currentX < 0 || currentY < 0) return;

      // Case 1: We have explored this tile, or it's not witn the mask bounds.
      if (!overallMask[currentY, currentX] || explored[currentY, currentX]) return;

      // Case 2: We are at an unexplored tile. Mark the tile as explored and carve it up
      explored[currentY, currentX] = true;
      d[currentY, currentX].Physics = d[currentY, currentX].Physics.OpenUp(Tile.MoveType.Open_HORIZ);

      // ...then proceed to explore its adjacent tiles
      bool[] untriedDir = { true, true, true, true };
      do
      {
        // Pick a random direction to explore
        int dir = 0;
        do { dir = r.Next() % 4; } while (!untriedDir[dir]);
        untriedDir[dir] = false; // We're trying it now

        // We calculate adjacent and proceeding coordinate so that
        // turns only occur on every other tile, so that entire tiles
        // are used as the walls between hallways
        int x2, y2, x3, y3; // Adjacent and next coordinate
        switch (dir)
        {
          case 0: // up
            x2 = currentX;
            y2 = currentY - 1;
            x3 = currentX;
            y3 = currentY - 2;
            break;
          case 1: // right
            x2 = currentX + 1;
            y2 = currentY;
            x3 = currentX + 2;
            y3 = currentY;
            break;
          case 2: // down
            x2 = currentX;
            y2 = currentY + 1;
            x3 = currentX;
            y3 = currentY + 2;
            break;
          case 3: // left
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

        // Tiles as walls means we traverse an adjacent (x2,y2) tile,
        // before moving to the "next" tile (x3,y3). So, do that.
        explored[y2, x2] = true;
        d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(Tile.MoveType.Open_HORIZ);

        // R E C U R S E !
        this.RecursiveBacktrack_TilesAsWalls(d, x3, y3, explored, overallMask, r);

      } while (untriedDir[0] || untriedDir[1] || untriedDir[2] || untriedDir[3]);

      // Case 3: We have explored this tile and all tried recursing
      // into al of its adjacent tiles. Time to pop back up in the
      // stack, to explore a previous tile's adjacents
      return;
    }
  }
}
