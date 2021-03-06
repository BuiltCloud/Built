﻿using Built.Mongo;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Built.Micro.ImageCloud
{
    /*
     If you want to create a repository for already defined non-entity model

	public class UserRepository : Repository<Entity<User>>
	{
		public UserRepository(string connectionString) : base(connectionString) {}

		//custom method
		public User FindbyUsername(string username)
		{
			return First(i => i.Content.Username == username);
		}
	}

        Usage
Each method has multiple overloads, read method summary for additional parameters

	UserRepository repo = new UserRepository("mongodb://localhost/sample")

	//Get
	User user = repo.Get("58a18d16bc1e253bb80a67c9");

	//Insert
	User item = new User(){
		Username = "username",
		Password = "password"
	};
	repo.Insert(item);

	//Update
	//single property
	repo.Update(item, i => i.Username, "newUsername");

	//multiple property
	//Updater has many methods like Inc, Push, CurrentDate, etc.
	var update1 = Updater.Set(i => i.Username, "oldUsername");
	var update2 = Updater.Set(i => i.Password, "newPassword");
	repo.Update(item, update1, update2);

	//all entity
	item.Username = "someUsername";
	repo.Replace(item);

	//Delete
	repo.Delete(item);

	//Queries - all queries has filter, order and paging features
	var first = repo.First();
	var last = repo.Last();
	var search = repo.Find(i => i.Username == "username");
	var allItems = repo.FindAll();

	//Utils
	var count = repo.Count();
	var any = repo.Any(i => i.Username.Contains("user"));
         */

    /*
     var collection = __database.GetCollection<BsonDocument>("restaurants");
        var keys = Builders<BsonDocument>.IndexKeys.Ascending("cuisine").Ascending("address.zipcode");
        var model = new CreateIndexModel<BsonDocument>(keys);
        await collection.Indexes.CreateOneAsync(model);
        // @code: end

        // @results: start
        using (var cursor = await collection.Indexes.ListAsync())
        {
            var indexes = await cursor.ToListAsync();
            indexes.Should().Contain(index => index["name"] == "cuisine_1_address.zipcode_1");
        }
     */

    public class MaterialRepository : Repository<Material>
    {
        public MaterialRepository(IConfiguration config) : base(config)
        {
            //e.g. "mongodb://username:password@localhost:27017/databaseName"
            //mongodb://host1:27017,host2:27017
            //MongoClientSettings
            //mongodb://host:27017/?replicaSet=rs0&uuidRepresentation=standard
            //mongodb://host:27017,host2:27017/?replicaSet=rs0
            //mongodb://192.168.1.230:27117,192.168.1.230:27217,192.168.1.230:27317/test?replicaSet=rs0
        }

        //custom method
        //public Material FindbyUsername(string username)
        //{
        //    return First(i => i.Username == username);
        //}
    }
}