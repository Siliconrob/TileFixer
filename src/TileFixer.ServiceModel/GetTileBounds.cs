using ServiceStack;

namespace Tile.ServiceModel
{
  [Route("/getBounds/{zIndex}/{xIndex}/{yIndex}", "GET")]
  public class GetTileBounds : IReturn<BoundingBox>
  {
    public int ZIndex { get; set; }
    public int XIndex { get; set; }
    public int YIndex { get; set; }
  }
}