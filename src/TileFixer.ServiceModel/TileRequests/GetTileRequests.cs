using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;

namespace Tile.ServiceModel
{
  [Route("/getTileRequests/point",
    Notes = "Example querystring use /getTileRequests/point?latitude=32.755488&longitude=-97.330766",
    Verbs = "GET")]
  [Route("/getTileRequests/point/{Latitude}/{Longitude}", "GET")]
  [DataContract]
  public class GetTileRequests : IReturn<List<GetTileBounds>>
  {
    [DataMember(Name = "latitude")]
    public double Latitude { get; set; }
    [DataMember(Name = "longitude")]
    public double Longitude { get; set; }
  }
}