﻿using MongoDB.Driver;
using System;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Built.Mongo
{
    /// <typeparam name="T">The type to get the collection of.</typeparam>
    internal class Database<T> where T : IEntity
    {
        private Database()
        {
        }

        /// <summary>
        /// Creates and returns a MongoCollection from specified type
        /// </summary>
        /// <param name="config">Configuration interface</param>
        /// <returns></returns>
        internal static IMongoCollection<T> GetCollection(IConfiguration config)
        {
            return GetCollectionFromConnectionString(GetDefaultConnectionString(config));
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        internal static IMongoCollection<T> GetCollectionFromConnectionString(string connectionString)
        {
            return GetCollectionFromConnectionString(connectionString, GetCollectionName());
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the connectionstring name and collection name
        /// </summary>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        internal static IMongoCollection<T> GetCollectionFromConnectionString(string connectionString, string collectionName)
        {
            return GetCollectionFromUrl(new MongoUrl(connectionString), collectionName);
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and url.
        /// </summary>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <returns>Returns a MongoCollection from the specified type and url.</returns>
        internal static IMongoCollection<T> GetCollectionFromUrl(MongoUrl url)
        {
            return GetCollectionFromUrl(url, GetCollectionName());
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and url.
        /// </summary>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and url.</returns>
        internal static IMongoCollection<T> GetCollectionFromUrl(MongoUrl url, string collectionName)
        {
            return GetDatabaseFromUrl(url).GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Creates and returns a MongoDatabase from the specified url.
        /// </summary>
        /// <param name="url">The url to use to get the database from.</param>
        /// <returns>Returns a MongoDatabase from the specified url.</returns>
        private static IMongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            return client.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        #region Collection Name

        /// <summary>
        /// Determines the collection name for T and assures it is not empty
        /// </summary>
        /// <returns>Returns the collection name for T.</returns>
        internal static string GetCollectionName()
        {
            string collectionName;
            collectionName = typeof(T).GetTypeInfo().BaseType.Equals(typeof(object)) ?
                                      GetCollectionNameFromInterface() :
                                      GetCollectionNameFromType();

            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }
            return collectionName.ToLowerInvariant();
        }

        /// <summary>
        /// Determines the collection name from the specified type.
        /// </summary>
        /// <returns>Returns the collection name from the specified type.</returns>
        private static string GetCollectionNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = CustomAttributeExtensions.GetCustomAttribute<CollectionNameAttribute>(typeof(T).GetTypeInfo());

            return att?.Name ?? typeof(T).Name;
        }

        /// <summary>
        /// Determines the collectionname from the specified type.
        /// </summary>
        /// <returns>Returns the collectionname from the specified type.</returns>
        private static string GetCollectionNameFromType()
        {
            Type entitytype = typeof(T);
            string collectionname;

            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = CustomAttributeExtensions.GetCustomAttribute<CollectionNameAttribute>(typeof(T).GetTypeInfo());
            if (att != null)
            {
                // It does! Return the value specified by the CollectionName attribute
                collectionname = att.Name;
            }
            else
            {
                collectionname = entitytype.Name;
            }

            return collectionname;
        }

        #endregion Collection Name

        #region Database Name

        /// <summary>
        /// Determines the Database name for T and assures it is not empty
        /// </summary>
        /// <returns>Returns the Database name for T.</returns>
        internal static string GetDatabaseName()
        {
            string databaseName = typeof(T).GetTypeInfo().BaseType.Equals(typeof(object)) ?
                                      GetDatabaseNameFromInterface() :
                                      GetDatabaseNameFromType();

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new Exception("GetDatabaseName null");
            }
            return databaseName.ToLowerInvariant();
        }

        /// <summary>
        /// Determines the Database name from the specified type.
        /// </summary>
        /// <returns>Returns the Database name from the specified type.</returns>
        private static string GetDatabaseNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = CustomAttributeExtensions.GetCustomAttribute<DatabaseNameAttribute>(typeof(T).GetTypeInfo());

            return att?.Name ?? typeof(T).Name;
        }

        /// <summary>
        /// Determines the Database name from the specified type.
        /// </summary>
        /// <returns>Returns the Database name from the specified type.</returns>
        private static string GetDatabaseNameFromType()
        {
            Type entitytype = typeof(T);
            string name;

            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = CustomAttributeExtensions.GetCustomAttribute<DatabaseNameAttribute>(typeof(T).GetTypeInfo());
            if (att != null)
            {
                // It does! Return the value specified by the CollectionName attribute
                name = att.Name;
            }
            else
            {
                name = entitytype.Name;
            }

            return name;
        }

        #endregion Database Name

        #region Connection Name

        /// <summary>
        /// Determines the connection name for T and assures it is not empty
        /// </summary>
        /// <returns>Returns the connection name for T.</returns>
        private static string GetConnectionName()
        {
            string connectionName;
            connectionName = typeof(T).GetTypeInfo().BaseType.Equals(typeof(object)) ?
                                      GetConnectionNameFromInterface() :
                                      GetConnectionNameFromType();

            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = typeof(T).Name;
            }
            return connectionName.ToLowerInvariant();
        }

        /// <summary>
        /// Determines the connection name from the specified type.
        /// </summary>
        /// <returns>Returns the connection name from the specified type.</returns>
        private static string GetConnectionNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a ConnectionName attribute
            var att = CustomAttributeExtensions.GetCustomAttribute<ConnectionNameAttribute>(typeof(T).GetTypeInfo());
            return att?.Name ?? typeof(T).Name;
        }

        /// <summary>
        /// Determines the connection name from the specified type.
        /// </summary>
        /// <returns>Returns the connection name from the specified type.</returns>
        private static string GetConnectionNameFromType()
        {
            Type entitytype = typeof(T);
            string collectionname;

            // Check to see if the object (inherited from Entity) has a ConnectionName attribute
            var att = CustomAttributeExtensions.GetCustomAttribute<ConnectionNameAttribute>(typeof(T).GetTypeInfo());
            if (att != null)
            {
                // It does! Return the value specified by the ConnectionName attribute
                collectionname = att.Name;
            }
            else
            {
                if (typeof(Entity).GetTypeInfo().IsAssignableFrom(entitytype))
                {
                    // No attribute found, get the basetype
                    while (!entitytype.GetTypeInfo().BaseType.Equals(typeof(Entity)))
                    {
                        entitytype = entitytype.GetTypeInfo().BaseType;
                    }
                }
                collectionname = entitytype.Name;
            }

            return collectionname;
        }

        #endregion Connection Name

        #region Connection String

        /// <summary>
        /// Retrieves the default connectionstring from the App.config or Web.config file.
        /// </summary>
        /// <returns>Returns the default connectionstring from the App.config or Web.config file.</returns>
        internal static string GetDefaultConnectionString(IConfiguration config)
        {
            return ConfigurationExtensions.GetConnectionString(config, GetConnectionName());
        }

        #endregion Connection String
    }

    internal class Database
    {
        private string DatabaseName { get; }
        public IMongoClient Client { get; }

        public MongoUrl Url { get; }

        /// <summary>
        /// mongodb://username:password@localhost:27017/databaseName
        /// mongodb://username:password@localhost:27017
        /// mongodb://localhost:27017
        /// mongodb://localhost:27017/databaseName
        /// </summary>
        public Database(string connectionString, string databaseName = "") : this(new MongoUrl(connectionString), databaseName)
        {
        }

        public Database(MongoUrl url, string databaseName = "")
        {
            Url = url;
            DatabaseName = string.IsNullOrWhiteSpace(databaseName) ? (string.IsNullOrWhiteSpace(Url.DatabaseName) ? string.Empty : Url.DatabaseName) : databaseName;
            Client = new MongoClient(Url);
        }

        internal IMongoDatabase GetDatabase(string databaseName = "")
        {
            if (!string.IsNullOrWhiteSpace(databaseName))
                return Client.GetDatabase(databaseName);
            else if (!string.IsNullOrWhiteSpace(DatabaseName))
                return Client.GetDatabase(DatabaseName);
            else
                throw new ArgumentNullException("not databaseName");
        }

        internal IMongoCollection<T> GetCollection<T>(string databaseName = "") where T : IEntity
        {
            if (string.IsNullOrWhiteSpace(databaseName) && string.IsNullOrWhiteSpace(DatabaseName))
                databaseName = Database<T>.GetDatabaseName();
            return GetDatabase(databaseName).GetCollection<T>(Database<T>.GetCollectionName());
        }
    }
}