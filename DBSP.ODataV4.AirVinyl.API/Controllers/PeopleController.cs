using DBSP.ODataV4.AirVinyl.API.Helpers;
using DBSP.ODataV4.AirVinyl.DataAccessLayer;
using DBSP.ODataV4.AirVinyl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace DBSP.ODataV4.AirVinyl.API.Controllers
{
  public class PeopleController : ODataController
  {
    private AirVinylDbContext _ctx = new AirVinylDbContext();

    public IHttpActionResult Get()
    {
      return Ok(_ctx.People);
    }

    public IHttpActionResult Get([FromODataUri] int key)
    {
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }
      return Ok(person);
    }

    [HttpGet]
    [ODataRoute("People({key})/Email")]
    [ODataRoute("People({key})/FirstName")]
    [ODataRoute("People({key})/LastName")]
    [ODataRoute("People({key})/DateOfBirth")]
    [ODataRoute("People({key})/Gender")]
    public IHttpActionResult GetPersonProperty([FromODataUri] int key)
    {
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }

      var propertyToGet = Url.Request.RequestUri.Segments.Last();
      if (!person.HasProperty(propertyToGet))
      {
        return NotFound();
      }

      var propertyValue = person.GetValue(propertyToGet);
      if (propertyValue == null)
      {
        return StatusCode(HttpStatusCode.NoContent);
      }

      return this.CreateOKHttpActionResult(propertyValue);
    }

    [HttpGet]
    [ODataRoute("People({key})/Friends")]
    [ODataRoute("People({key})/VinylRecords")]
    [EnableQuery]
    public IHttpActionResult GetPersonCollectionProperty([FromODataUri] int key)
    {
      var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();

      var person = _ctx.People.Include(collectionPropertyToGet)
                              .FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }

      var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

      return this.CreateOKHttpActionResult(collectionPropertyValue);
    }

    [HttpGet]
    [ODataRoute("People({key})/Email/$value")]
    [ODataRoute("People({key})/FirstName/$value")]
    [ODataRoute("People({key})/LastName/$value")]
    [ODataRoute("People({key})/DateOfBirth/$value")]
    [ODataRoute("People({key})/Gender/$value")]
    public IHttpActionResult GetPersonPropertyRawValue([FromODataUri] int key)
    {
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }

      var propertyToGet = Url.Request.RequestUri
          .Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');

      if (!person.HasProperty(propertyToGet))
      {
        return NotFound();
      }

      var propertyValue = person.GetValue(propertyToGet);

      if (propertyValue == null)
      {
        return StatusCode(HttpStatusCode.NoContent);
      }

      // return raw value => ToString()
      return this.CreateOKHttpActionResult(propertyValue.ToString());
    }

    public IHttpActionResult Post(Person person)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      _ctx.People.Add(person);
      _ctx.SaveChanges();

      return Created(person);
    }

    protected override void Dispose(bool disposing)
    {
      _ctx.Dispose();
      base.Dispose(disposing);
    }
  }
}