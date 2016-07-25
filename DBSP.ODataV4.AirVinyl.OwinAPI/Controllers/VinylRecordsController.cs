using DBSP.ODataV4.AirVinyl.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace DBSP.ODataV4.AirVinyl.OwinAPI.Controllers
{
  public class VinylRecordsController : ODataController
  {
    private AirVinylDbContext _ctx = new AirVinylDbContext();

    [HttpGet]
    [ODataRoute("VinylRecords")]
    public IHttpActionResult GetAllVinylRecords()
    {
      return Ok(_ctx.VinylRecords);
    }

    [HttpGet]
    [ODataRoute("VinylRecords({key})")]
    public IHttpActionResult GetOneVinylRecord([FromODataUri] int key)
    {
      var vinylRecord = _ctx.VinylRecords.FirstOrDefault(v => v.VinylRecordId == key);
      if (vinylRecord == null)
      {
        return NotFound();
      }
      return Ok(vinylRecord);
    }

    protected override void Dispose(bool disposing)
    {
      _ctx.Dispose();
      base.Dispose(disposing);
    }
  }
}