namespace AirVinyl.DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSetUp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.People",
                c => new
                    {
                        PersonId = c.Int(nullable: false, identity: true),
                        Email = c.String(maxLength: 100),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        DateOfBirth = c.DateTimeOffset(nullable: false, precision: 7),
                        Gender = c.Int(nullable: false),
                        NumberOfRecordsOnWishList = c.Int(nullable: false),
                        AmountOfCashToSpend = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.PersonId);
            
            CreateTable(
                "dbo.VinylRecords",
                c => new
                    {
                        VinylRecordId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 150),
                        Artist = c.String(nullable: false, maxLength: 150),
                        CatalogNumber = c.String(maxLength: 50),
                        Year = c.Int(),
                        PressingDetail_PressingDetailId = c.Int(),
                        Person_PersonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VinylRecordId)
                .ForeignKey("dbo.PressingDetails", t => t.PressingDetail_PressingDetailId)
                .ForeignKey("dbo.People", t => t.Person_PersonId, cascadeDelete: true)
                .Index(t => t.PressingDetail_PressingDetailId)
                .Index(t => t.Person_PersonId);
            
            CreateTable(
                "dbo.PressingDetails",
                c => new
                    {
                        PressingDetailId = c.Int(nullable: false, identity: true),
                        Grams = c.Int(nullable: false),
                        Inches = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.PressingDetailId);
            
            CreateTable(
                "dbo.RecordStores",
                c => new
                    {
                        RecordStoreId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        StoreAddress_Street = c.String(maxLength: 200),
                        StoreAddress_City = c.String(maxLength: 100),
                        StoreAddress_PostalCode = c.String(maxLength: 10),
                        StoreAddress_Country = c.String(maxLength: 100),
                        TagsAsString = c.String(),
                        Specialization = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.RecordStoreId);
            
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        RatingId = c.Int(nullable: false, identity: true),
                        Value = c.Int(nullable: false),
                        RatedBy_PersonId = c.Int(nullable: false),
                        RecordStore_RecordStoreId = c.Int(),
                    })
                .PrimaryKey(t => t.RatingId)
                .ForeignKey("dbo.People", t => t.RatedBy_PersonId, cascadeDelete: true)
                .ForeignKey("dbo.RecordStores", t => t.RecordStore_RecordStoreId)
                .Index(t => t.RatedBy_PersonId)
                .Index(t => t.RecordStore_RecordStoreId);
            
            CreateTable(
                "dbo.PersonPersons",
                c => new
                    {
                        Person_PersonId = c.Int(nullable: false),
                        Person_PersonId1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Person_PersonId, t.Person_PersonId1 })
                .ForeignKey("dbo.People", t => t.Person_PersonId)
                .ForeignKey("dbo.People", t => t.Person_PersonId1)
                .Index(t => t.Person_PersonId)
                .Index(t => t.Person_PersonId1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ratings", "RecordStore_RecordStoreId", "dbo.RecordStores");
            DropForeignKey("dbo.Ratings", "RatedBy_PersonId", "dbo.People");
            DropForeignKey("dbo.VinylRecords", "Person_PersonId", "dbo.People");
            DropForeignKey("dbo.VinylRecords", "PressingDetail_PressingDetailId", "dbo.PressingDetails");
            DropForeignKey("dbo.PersonPersons", "Person_PersonId1", "dbo.People");
            DropForeignKey("dbo.PersonPersons", "Person_PersonId", "dbo.People");
            DropIndex("dbo.PersonPersons", new[] { "Person_PersonId1" });
            DropIndex("dbo.PersonPersons", new[] { "Person_PersonId" });
            DropIndex("dbo.Ratings", new[] { "RecordStore_RecordStoreId" });
            DropIndex("dbo.Ratings", new[] { "RatedBy_PersonId" });
            DropIndex("dbo.VinylRecords", new[] { "Person_PersonId" });
            DropIndex("dbo.VinylRecords", new[] { "PressingDetail_PressingDetailId" });
            DropTable("dbo.PersonPersons");
            DropTable("dbo.Ratings");
            DropTable("dbo.RecordStores");
            DropTable("dbo.PressingDetails");
            DropTable("dbo.VinylRecords");
            DropTable("dbo.People");
        }
    }
}
