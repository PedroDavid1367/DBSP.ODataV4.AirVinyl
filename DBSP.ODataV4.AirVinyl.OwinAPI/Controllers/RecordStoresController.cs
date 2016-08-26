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

    [HttpGet]
    [ODataRoute("RecordStores({key})/AirVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
    public bool IsHighRated([FromODataUri] int key, int minimumRating)
    {
      // get the RecordStore
      var recordStore = _context.RecordStores
          .FirstOrDefault(p => p.RecordStoreId == key
              && p.Ratings.Any()
              && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating);

      return (recordStore != null);
    }

    [HttpGet]
    [ODataRoute("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={personIds})")]
    public IHttpActionResult AreRatedBy([FromODataUri] IEnumerable<int> personIds)
    {
      // get the RecordStores
      var recordStores = _context.RecordStores
          .Where(p => p.Ratings.Any(r => personIds.Contains(r.RatedBy.PersonId)));

      return this.CreateOKHttpActionResult(recordStores);
    }

    [HttpGet]
    [ODataRoute("GetHighRatedRecordStores(minimumRating={minimumRating})")]
    public IHttpActionResult GetHighRatedRecordStores([FromODataUri] int minimumRating)
    {
      // get the RecordStores
      var recordStores = _context.RecordStores
          .Where(p => p.Ratings.Any()
              && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating);

      return this.CreateOKHttpActionResult(recordStores);
    }

    [HttpPost]
    [ODataRoute("RecordStores({key})/AirVinyl.Actions.Rate")]
    public IHttpActionResult Rate([FromODataUri] int key, ODataActionParameters parameters)
    {
      // get the RecordStore
      var recordStore = _context.RecordStores
        .FirstOrDefault(p => p.RecordStoreId == key);

      if (recordStore == null)
      {
        return NotFound();
      }

      // from the param dictionary, get the rating & personid
      int rating;
      int personId;
      object outputFromDictionary;

      if (!parameters.TryGetValue("rating", out outputFromDictionary))
      {
        return NotFound();
      }

      if (!int.TryParse(outputFromDictionary.ToString(), out rating))
      {
        return NotFound();
      }

      if (!parameters.TryGetValue("personId", out outputFromDictionary))
      {
        return NotFound();
      }

      if (!int.TryParse(outputFromDictionary.ToString(), out personId))
      {
        return NotFound();
      }

      // the person must exist
      var person = _context.People
      .FirstOrDefault(p => p.PersonId == personId);

      if (person == null)
      {
        return NotFound();
      }

      // everything checks out, add the rating
      recordStore.Ratings.Add(new Rating() { RatedBy = person, Value = rating });

      // save changes
      if (_context.SaveChanges() > -1)
      {
        // return true
        return this.CreateOKHttpActionResult(true);
      }
      else
      {
        // Something went wrong - we expect our
        // action to return false in that case.
        // The request is still successful, false
        // is a valid response
        return this.CreateOKHttpActionResult(false);
      }
    }

    [HttpPost]
    [ODataRoute("RecordStores/AirVinyl.Actions.RemoveRatings")]
    public IHttpActionResult RemoveRatings(ODataActionParameters parameters)
    {
      // from the param dictionary, get the personid
      int personId;
      object outputFromDictionary;

      if (!parameters.TryGetValue("personId", out outputFromDictionary))
      {
        return NotFound();
      }

      if (!int.TryParse(outputFromDictionary.ToString(), out personId))
      {
        return NotFound();
      }

      // get the RecordStores that were rated by the person with personId
      var recordStoresRatedByCurrentPerson = _context.RecordStores
          .Include("Ratings").Include("Ratings.RatedBy")
          .Where(p => p.Ratings.Any(r => r.RatedBy.PersonId == personId)).ToList();

      // remove those ratings
      foreach (var store in recordStoresRatedByCurrentPerson)
      {
        // get the ratings by the current person
        var ratingsByCurrentPerson = store.Ratings
            .Where(r => r.RatedBy.PersonId == personId).ToList();

        for (int i = 0; i < ratingsByCurrentPerson.Count(); i++)
        {
          store.Ratings.Remove(ratingsByCurrentPerson[i]);
        }
      }

      // save changes
      if (_context.SaveChanges() > -1)
      {
        // return true
        return this.CreateOKHttpActionResult(true);
      }
      else
      {
        // Something went wrong - we expect our
        // action to return false in that case.
        // The request is still successful, false
        // is a valid response
        return this.CreateOKHttpActionResult(false);
      }
    }

    [HttpPost]
    [ODataRoute("RemoveRecordStoreRatings")]
    public IHttpActionResult RemoveRecordStoreRatings(ODataActionParameters parameters)
    {
      // from the param dictionary, get the personid
      int personId;
      object outputFromDictionary;

      if (!parameters.TryGetValue("personId", out outputFromDictionary))
      {
        return NotFound();
      }

      if (!int.TryParse(outputFromDictionary.ToString(), out personId))
      {
        return NotFound();
      }

      // get the RecordStores that were rated by the person with personId
      var recordStoresRatedByCurrentPerson = _context.RecordStores
          .Include("Ratings").Include("Ratings.RatedBy")
          .Where(p => p.Ratings.Any(r => r.RatedBy.PersonId == personId)).ToList();

      // remove those ratings
      foreach (var store in recordStoresRatedByCurrentPerson)
      {
        // get the ratings by the current person
        var ratingsByCurrentPerson = store.Ratings.Where(r => r.RatedBy.PersonId == personId).ToList();
        for (int i = 0; i < ratingsByCurrentPerson.Count(); i++)
        {
          store.Ratings.Remove(ratingsByCurrentPerson[i]);
        }
      }

      // save changes
      if (_context.SaveChanges() > -1)
      {
        // return no content
        return StatusCode(HttpStatusCode.NoContent);
      }
      else
      {
        // something went wrong
        return StatusCode(HttpStatusCode.InternalServerError);
      }
    }

    protected override void Dispose(bool disposing)
    {
      _context.Dispose();
      base.Dispose(disposing);
    }
  }
}