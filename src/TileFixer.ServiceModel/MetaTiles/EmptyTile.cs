using System.IO;
using ServiceStack;

namespace Tile.ServiceModel
{
  [Route("/emptyTile/{z}/{x}/{y}", "GET")]
  public class EmptyTile : IReturn<Stream>
  {
    public int z { get; set; }

    public int x { get; set; }

    public int y { get; set; }
  }
}