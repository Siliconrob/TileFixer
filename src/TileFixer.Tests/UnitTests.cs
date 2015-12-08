﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Testing;
using Tile.ServiceInterface;
using Tile.ServiceModel;

namespace TileFixer.Tests
{
  [TestFixture]
  public class UnitTests
  {
    private readonly ServiceStackHost appHost;

    public UnitTests()
    {
      appHost = new BasicAppHost(typeof (TileLayer).Assembly)
      {
        ConfigureContainer = container =>
        {
          //Add your IoC dependencies here
        }
      }
        .Init();
    }

    [OneTimeTearDown]
    public void TestFixtureTearDown()
    {
      appHost.Dispose();
    }

    [Test]
    public void TestGetBoundedTileRequests()
    {
      var boundsRequest = new GetBoundedTileRequests
      {
        NorthWestLat = 31,
        NorthWestLng = -96,
        SouthEastLat = 29,
        SouthEastLng = -97
      };
      var url = boundsRequest.ToGetUrl();
      var service = appHost.Container.Resolve<TileLayer>();
      var response = (List<TileLevelRequestRange>) service.Get(boundsRequest);

      var tileRequests = response.ElementAt(13).LevelRequests();

      //Assert.That(response.ToJson(), Is.EqualTo(testBounds));
      Assert.That(tileRequests.Count > 0);
    }

    [Test]
    public void TestGetBounds()
    {
      const string testBounds =
        "{\"NorthWest\":{\"Latitude\":32.768798828125,\"Longitude\":-97.3828125,\"Projection\":3857},\"NorthEast\":{\"Latitude\":32.768798828125,\"Longitude\":-97.3388671875,\"Projection\":3857},\"SouthWest\":{\"Latitude\":32.7318420410156,\"Longitude\":-97.3828125,\"Projection\":3857},\"SouthEast\":{\"Latitude\":32.7318420410156,\"Longitude\":-97.3388671875,\"Projection\":3857}}";
      var service = appHost.Container.Resolve<TileLayer>();
      var boundsRequests = new GetTileBounds {zIndex = 13, xIndex = 1880, yIndex = 3306};
      var response = (BoundingBox) service.Get(boundsRequests);
      Assert.That(response.ToJson(), Is.EqualTo(testBounds));
    }
  }
}