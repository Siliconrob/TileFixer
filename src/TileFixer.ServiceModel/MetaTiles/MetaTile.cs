using System.IO;
using ServiceStack;

namespace Tile.ServiceModel
{
  [Route("/metaTile/{zIndex}/{xIndex}/{yIndex}", "GET")]
  public class MetaTile : IReturn<Stream>
  {
    public int ZIndex { get; set; }
    public int XIndex { get; set; }
    public int YIndex { get; set; }
  }
}