﻿using System;
using System.Web;
using ServiceStack.Logging;
using ServiceStack.Logging.NLogger;
using ServiceStack.Text;
using Tile.ServiceModel;

namespace Tile.Fixer
{
  public class Global : HttpApplication
  {
    private RouteAppHost _appHost;

    protected void Application_Start(object sender, EventArgs e)
    {
      LogManager.LogFactory = new NLogFactory();
      // Initialize ServiceStack Host
      _appHost = new RouteAppHost();
      _appHost.Init();
    }

    protected void Session_Start(object sender, EventArgs e)
    {
    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {
    }

    protected void Application_Error(object sender, EventArgs e)
    {
      // Code that runs when an unhandled error occurs
      var ex = Server.GetLastError();
      if (ex != null && ex.Message.Length > 0)
      {
        this.Log().ErrorFormat("Message: {0}, Dump: {1}, StackTrace: {2}", ex.Message, ex.Dump(), ex.StackTrace);
      }
    }

    protected void Session_End(object sender, EventArgs e)
    {
    }

    protected void Application_End(object sender, EventArgs e)
    {
    }
  }
}