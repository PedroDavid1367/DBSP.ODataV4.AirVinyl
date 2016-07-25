using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using System.Web.OData.Builder;
using DBSP.ODataV4.AirVinyl.Model;

namespace DBSP.ODataV4.AirVinyl.OwinAPI
{
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      var config = WebApiConfig.Register();

      app.UseWebApi(config);
    }
  }

  public static class WebApiConfig
  {
    public static HttpConfiguration Register()
    {
      var config = new HttpConfiguration();

      config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());

      return config;
    }

    private static IEdmModel GetEdmModel()
    {
      var builder = new ODataConventionModelBuilder();
      builder.Namespace = "AirVinyl";
      builder.ContainerName = "AirVinylContainer";

      builder.EntitySet<Person>("People");
      builder.EntitySet<VinylRecord>("VinylRecords");

      return builder.GetEdmModel();
    }
  }
}
