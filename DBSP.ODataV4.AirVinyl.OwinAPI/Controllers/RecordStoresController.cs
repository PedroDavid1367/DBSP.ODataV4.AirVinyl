using DBSP.ODataV4.AirVinyl.DataAccessLayer;
using DBSP.ODataV4.AirVinyl.Model;
using DBSP.ODataV4.AirVinyl.OwinAPI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace DBSP.ODataV4.AirVinyl.OwinAPI.Controllers
{
  public class RecordStoresController : ODataController
  {
    private AirVinylDbContext _context = new AirVinylDbContext();

    // GET odata/RecordStores
    [EnableQuery]
    public IHttpActionResult Get()
    {
      return Ok(_context.RecordStores);
    }

    // GET odata/RecordStores(key)
    [EnableQuery]
    public IHttpActionResult Get([FromODataUri] int key)
    {
      var recordStores = _context.RecordStores.Where(r => r.RecordStoreId == key);
      if (!recordStores.Any())
      {
        return NotFound();
      }
      return Ok(SingleResult.Create(recordStores));
    }

    [HttpGet]
    [ODataRoute("RecordStores({key})/Tags")]
    [EnableQuery]
    public IHttpActionResult GetRecordStoreTagsProperty([FromODataUri] int key)
    {
      // no include necessary for EF - Tags ins't a navigation property
      // in the entity model
      var recordStore = _context.RecordStores.FirstOrDefault(r => r.RecordStoreId == key);
      if (recordStore == null)
      {
        return NotFound();
      }
      var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
      var collectionPropertyValue = recordStore.GetValue(collectionPropertyToGet);

      // return the collection of tags
      return this.CreateOKHttpActionResult(collectionPropertyValue);
    }

    protected override void Dispose(bool disposing)
    {
      _context.Dispose();
      base.Dispose(disposing);
    }
  }
}