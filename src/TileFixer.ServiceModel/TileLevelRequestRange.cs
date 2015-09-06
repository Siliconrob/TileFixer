using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;

namespace Tile.ServiceModel
{
  public class TileLevelRequestRange
  {
    public TileRequest SouthEast { get; set; }
    public TileRequest NorthWest { get; set; }
  }

  public static class TileLevelRequestRangeExtensions
  {
    public static List<TileRequest> LevelRequests(this TileLevelRequestRange current)
    {
      var requests = new List<TileRequest>();
      if (current.SouthEast.Z != current.NorthWest.Z)
      {
        throw new ArgumentException("SouthEast Z level {0} does not match NorthWest Z level {1}".Fmt(current.SouthEast.Z, current.NorthWest.Z));
      }
      foreach (var x in Enumerable.Range(0, (current.NorthWest.X - current.SouthEast.X) + 1))
      {
        var currentX = current.SouthEast.X + x;
        foreach (var y in Enumerable.Range(0, (current.SouthEast.Y - current.NorthWest.Y) + 1))
        {
          var currentY = current.NorthWest.Y + y;
          requests.Add(new TileRequest {Z = current.NorthWest.Z, X = currentX, Y = currentY });
        }
      }
      return requests;
    }
  }
}