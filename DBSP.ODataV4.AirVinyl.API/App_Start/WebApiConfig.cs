using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using System.Web.OData.Builder;
using DBSP.ODataV4.AirVinyl.Model;

namespace DBSP.ODataV4.AirVinyl.API
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());
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
