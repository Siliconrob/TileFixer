using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;

namespace Tile.ServiceModel
{

  [Route("/getBoundedTileRequests/bounds",
    Notes = "Example querystring use /getBoundedTileRequests/bounds?southEastLat=32.789913&southEastLng=-97.276607&northWestLat=32.710796&northWestLng=-97.418056",
    Verbs = "GET")]
  [DataContract]
  public class GetBoundedTileRequests : IReturn<List<TileLevelRequestRange>>
  {
    [DataMember(Name = "southEastLat")]
    public double SouthEastLat { get; set; }
    [DataMember(Name = "southEastLng")]
    public double SouthEastLng { get; set; }
    [DataMember(Name = "northWestLat")]
    public double NorthWestLat { get; set; }
    [DataMember(Name = "northWestLng")]
    public double NorthWestLng { get; set; }

    public override string ToString()
    {
      return this.ToJson();
    }
  }
}