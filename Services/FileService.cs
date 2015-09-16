using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using Sabio.Web.Services.Interfaces;

namespace Sabio.Web.Services
{
    public class FileService : IFileService
    {

        private string _Prefix = "*Folder_Name*/"; //Optional
        private string _BucketName = *your_bucket_name*;
        private RegionEndpoint _Region = RegionEndpoint.USEast1;

		//returns a client, allowing you to access Amazon
        public IAmazonS3 GetS3Client()
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            
            IAmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client(appConfig["AWSAccessKey"], appConfig["AWSSecretKey"], _Region);
            
            return s3Client;
        }

		//Pushes file to Amazon S3 with public read permissions
        public bool UploadFile(string localFile,string fileName, string contentType)
        {
            IAmazonS3 client = GetS3Client();

            var result = false;
            try
            {
                var request = new TransferUtilityUploadRequest
                {
                    BucketName = _BucketName,
                    Key = _Prefix+fileName,
                    FilePath = localFile,
                    StorageClass = S3StorageClass.Standard,
                    CannedACL = S3CannedACL.PublicRead,
                    ContentType = contentType
                };
                var fileTransferUtility = new TransferUtility(client);
                fileTransferUtility.Upload(request);
                //PutObjectResponse response2 = client.PutObject(request);
                
                result = true;
            }
            catch
            {
                return result;
            }
            

            return result;
        }
		
		//Deletes a file
        public void Delete(string fileName)
        {
            IAmazonS3 client = GetS3Client();
            var request = new DeleteObjectRequest
            {
                BucketName = _BucketName,
                Key = _Prefix + fileName
            };
            client.DeleteObject(request);
        }

    }
}