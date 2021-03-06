﻿using DBSP.ODataV4.AirVinyl.DataAccessLayer;
using DBSP.ODataV4.AirVinyl.Model;
using DBSP.ODataV4.AirVinyl.OwinAPI.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace DBSP.ODataV4.AirVinyl.OwinAPI.Controllers
{
  public class PeopleController : ODataController
  {
    private AirVinylDbContext _ctx = new AirVinylDbContext();

    [EnableQuery(MaxExpansionDepth = 3, MaxSkip = 10, MaxTop = 5, PageSize = 4)]
    public IHttpActionResult Get()
    {
      return Ok(_ctx.People);
    }

    [EnableQuery]
    public IHttpActionResult Get([FromODataUri] int key)
    {
      //var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      //if (person == null)
      //{
      //  return NotFound();
      //}

      //return Ok(person);

      var person = _ctx.People.Where(p => p.PersonId == key);
      if (!person.Any())
      {
        return NotFound();
      }

      return Ok(SingleResult.Create(person));
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

    // Actually the most efficient way.
    [HttpGet]
    [ODataRoute("People({key})/VinylRecords")]
    [EnableQuery]
    public IHttpActionResult GetVinylRecordsForPerson([FromODataUri] int key)
    {
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }
      
      return Ok(_ctx.VinylRecords.Where(v => v.Person.PersonId == key));
    }

    [HttpGet]
    [EnableQuery]
    [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
    public IHttpActionResult GetVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey)
    {
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }

      // queryable, no FirstOrDefault
      var vinylRecords = _ctx.VinylRecords.Where(v => v.Person.PersonId == key
          && v.VinylRecordId == vinylRecordKey);
      if (!vinylRecords.Any())
      {
        return NotFound();
      }

      return Ok(SingleResult.Create(vinylRecords));
    }

    [HttpPost]
    [ODataRoute("People({key})/VinylRecords")]
    public IHttpActionResult CreateVinylRecordForPerson([FromODataUri] int key,
      VinylRecord vinylRecord)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      // does the person exist?
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }

      // link the person to the VinylRecord (also avoids an invalid person
      // key on the passed-in record - key from the URI wins)
      vinylRecord.Person = person;

      // add the VinylRecord
      _ctx.VinylRecords.Add(vinylRecord);
      _ctx.SaveChanges();

      // return the created VinylRecord
      return Created(vinylRecord);
    }

    [HttpPatch]
    [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
    public IHttpActionResult PartiallyUpdateVinylRecordForPerson([FromODataUri] int key,
      [FromODataUri] int vinylRecordKey, Delta<VinylRecord> patch)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      // does the person exist?
      var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (person == null)
      {
        return NotFound();
      }

      // find a matching vinyl record
      var currentVinylRecord = _ctx.VinylRecords
          .FirstOrDefault(p => p.VinylRecordId == vinylRecordKey && p.Person.PersonId == key);

      // return NotFound if the VinylRecord isn't found
      if (currentVinylRecord == null)
      {
        return NotFound();
      }

      // apply patch
      patch.Patch(currentVinylRecord);
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }

    [HttpDelete]
    [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
    public IHttpActionResult DeleteVinylRecordForPerson([FromODataUri] int key,
      [FromODataUri] int vinylRecordKey)
    {
      var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      // find a matching vinyl record
      var currentVinylRecord = _ctx.VinylRecords
          .FirstOrDefault(p => p.VinylRecordId == vinylRecordKey && p.Person.PersonId == key);

      if (currentVinylRecord == null)
      {
        return NotFound();
      }

      _ctx.VinylRecords.Remove(currentVinylRecord);
      _ctx.SaveChanges();

      // return No Content
      return StatusCode(HttpStatusCode.NoContent);
    }

    [HttpGet]
    [ODataRoute("People({key})/Friends")]
    //[ODataRoute("People({key})/VinylRecords")]
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

    public IHttpActionResult Put([FromODataUri] int key, Person person)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      // Alternative: If the person isn't found: Uperset. This must only
      // be used if the responsibility for creating the key isn't at 
      // server-level. in our case, we're using auto-increment fields,
      // so this isn't allowed - code is for illustration purposes only!
      //if (currentPerson == null)
      //{
      //  // the key from the URI is the key we should use
      //  person.PersonId = key;
      //  _ctx.People.Add(person);
      //  _ctx.SaveChanges();
      //  return Created(person);
      //}

      person.PersonId = currentPerson.PersonId;
      _ctx.Entry(currentPerson).CurrentValues.SetValues(person);
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }

    public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> patch)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      //patch.Patch(currentPerson);
      //_ctx.SaveChanges();

      // Adding a workaround
      var id = currentPerson.PersonId;
      patch.Patch(currentPerson);
      currentPerson.PersonId = id;
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }

    public IHttpActionResult Delete([FromODataUri] int key)
    {
      var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      var peopleWithCurrentPersonAsFriend = _ctx.People.Include("Friends")
          .Where(p => p.Friends.Select(f => f.PersonId).AsQueryable().Contains(key));
      foreach (var person in peopleWithCurrentPersonAsFriend.ToList())
      {
        person.Friends.Remove(currentPerson);
      }

      _ctx.People.Remove(currentPerson);
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }


    [HttpPost]
    [ODataRoute("People({key})/Friends/$ref")]
    public IHttpActionResult CreateLinkToFriend([FromODataUri] int key, [FromBody] Uri link)
    {
      var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      var keyOfFriendToAdd = Request.GetKeyValue<int>(link);
      if (currentPerson.Friends.Any(i => i.PersonId == keyOfFriendToAdd))
      {
        return BadRequest($"The person with id {key} is already linked to the person with id {keyOfFriendToAdd}");
      }

      var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);
      if (friendToLinkTo == null)
      {
        return NotFound();
      }

      currentPerson.Friends.Add(friendToLinkTo);
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }


    [HttpPut]
    [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
    public IHttpActionResult UpdateLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey, [FromBody] Uri link)
    {
      var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      var currentFriend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
      if (currentFriend == null)
      {
        return NotFound();
      }

      var keyOfFriendToAdd = Request.GetKeyValue<int>(link);
      if (currentPerson.Friends.Any(i => i.PersonId == keyOfFriendToAdd))
      {
        return BadRequest($"The person with id {key} is already linked to the person with id {keyOfFriendToAdd}");
      }

      var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);
      if (friendToLinkTo == null)
      {
        return NotFound();
      }

      currentPerson.Friends.Remove(currentFriend);
      currentPerson.Friends.Add(friendToLinkTo);
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }


    [HttpDelete]
    [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
    public IHttpActionResult DeleteLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey)
    {
      var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
      if (currentPerson == null)
      {
        return NotFound();
      }

      var friend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
      if (friend == null)
      {
        return NotFound();
      }

      currentPerson.Friends.Remove(friend);
      _ctx.SaveChanges();

      return StatusCode(HttpStatusCode.NoContent);
    }

    protected override void Dispose(bool disposing)
    {
      _ctx.Dispose();
      base.Dispose(disposing);
    }
  }
}