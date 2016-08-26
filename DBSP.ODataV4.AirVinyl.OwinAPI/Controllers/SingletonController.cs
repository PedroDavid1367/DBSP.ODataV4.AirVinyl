using DBSP.ODataV4.AirVinyl.DataAccessLayer;
using DBSP.ODataV4.AirVinyl.Model;
using DBSP.ODataV4.AirVinyl.OwinAPI.Helpers;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace DBSP.ODataV4.AirVinyl.OwinAPI.Controllers
{
  public class SingletonController : ODataController
  {
    private AirVinylDbContext _context = new AirVinylDbContext();
    private const int _key = 5;

    [HttpGet]
    [ODataRoute("Kenneth")]
    public IHttpActionResult GetSingletonKenneth()
    {
      // find Kenneth - he's got id 6

      var personKenneth = _context.People.FirstOrDefault(p => p.PersonId == _key);
      return Ok(personKenneth);
    }

    [HttpGet]
    [ODataRoute("Kenneth/Email")]
    [ODataRoute("Kenneth/FirstName")]
    [ODataRoute("Kenneth/LastName")]
    [ODataRoute("Kenneth/DateOfBirth")]
    [ODataRoute("Kenneth/Gender")]
    public IHttpActionResult GetPropertyOfKenneth()
    {
      // find Kenneth
      var person = _context.People.FirstOrDefault(p => p.PersonId == _key);
      if (person == null)
      {
        return NotFound();
      }

      var propertyToGet = Url.Request.RequestUri.Segments.Last();
      var propertyValue = person.GetValue(propertyToGet);

      if (propertyValue == null)
      {
        // null = no content
        return StatusCode(HttpStatusCode.NoContent);
      }

      return this.CreateOKHttpActionResult(propertyValue);
    }

    [HttpGet]
    [ODataRoute("Kenneth/Email/$value")]
    [ODataRoute("Kenneth/FirstName/$value")]
    [ODataRoute("Kenneth/LastName/$value")]
    [ODataRoute("Kenneth/DateOfBirth/$value")]
    [ODataRoute("Kenneth/Gender/$value")]
    public object GetPersonPropertyRawValue()
    {
      var person = _context.People.FirstOrDefault(p => p.PersonId == _key);
      if (person == null)
      {
        return NotFound();
      }

      var propertyToGet = Url.Request.RequestUri
          .Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');
      var propertyValue = person.GetValue(propertyToGet);

      if (propertyValue == null)
      {
        // null = no content
        return StatusCode(HttpStatusCode.NoContent);
      }

      // return the raw value => ToString()
      return this.CreateOKHttpActionResult(propertyValue.ToString());
    }

    [HttpGet]
    [EnableQuery]
    [ODataRoute("Kenneth/Friends")]
    public IHttpActionResult GetCollectionPropertyForKenneth()
    {
      // find Kenneth, including the requested collection (path segment)
      var person = _context.People.Include(Url.Request.RequestUri.Segments.Last())
          .FirstOrDefault(p => p.PersonId == _key);

      if (person == null)
      {
        return NotFound();
      }

      var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
      var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

      // return the collection
      return this.CreateOKHttpActionResult(collectionPropertyValue);
    }

    [HttpGet]
    [EnableQuery]
    [ODataRoute("Kenneth/VinylRecords")]
    public IHttpActionResult GetVinylRecordsForKenneth()
    {
      var person = _context.People.FirstOrDefault(p => p.PersonId == _key);
      if (person == null)
      {
        return NotFound();
      }

      // return the collection
      return Ok(_context.VinylRecords.Where(v => v.Person.PersonId == _key));
    }

    [HttpPatch]
    [ODataRoute("Kenneth")]
    public IHttpActionResult PartiallyUpdateKenneth(Delta<Person> patch)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      // find Kenneth
      var currentPerson = _context.People.FirstOrDefault(p => p.PersonId == _key);

      // apply the patch, and save the changes
      patch.Patch(currentPerson);
      _context.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }

    protected override void Dispose(bool disposing)
    {
      // dispose the context
      _context.Dispose();
      base.Dispose(disposing);
    }
  }
}