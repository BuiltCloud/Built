﻿using Built.Micro.ImageCloud.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Built.Micro.ImageCloud.Domain.Services
{
    public interface IMaterialService : IService<Material>
    {
        Task<ObjectId> UploadAsync(string filename, byte[] bytes);
    }

    public class MaterialService : IMaterialService
    {
        public IRepository<Material> Repository { get; }

        public MaterialService(IRepository<Material> materialRepository)
        {
            Repository = materialRepository;
        }

        public async Task<ObjectId> UploadAsync(string filename, byte[] bytes)
        {
            var bucket = new GridFSBucket(Repository.Collection.Database); //这个是初始化gridFs存储的
            ObjectId fileId = await bucket.UploadFromBytesAsync(filename, bytes, new GridFSUploadOptions
            {
                DisableMD5 = false
            });

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var hash = md5.ComputeHash(bytes);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("x2"));
            }
            var md5Str = result.ToString();

            var fileList = bucket.Find(new { md5 = md5Str }.ToBsonDocument()).ToList();
            if (fileList.Any())
            {
            }
            var entity = new Material
            {
                FileId = fileId.ToString(),
                Name = filename,
                MD5 = md5Str
            };
            Repository.Insert(entity);
            return entity.ObjectId;
        }
    };
}