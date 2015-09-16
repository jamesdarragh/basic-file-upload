using Sabio.Web.Models.Requests;
using Sabio.Web.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Sabio.Data;
using Sabio.Web.Enums;
using Sabio.Web.Services.Interfaces;

namespace Sabio.Web.Services
{
    public class MediaService : BaseService, IMediaService
    {
        public string baseUrl = "https://*BucketNameHere*.s3.amazonaws.com/";

        public int UploadPicture(ImageModel model, string currentUser)
        {
            int mediaId = 0; 
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Medias_Insert"
               , inputParamMapper: delegate(SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", currentUser);
                   paramCollection.AddWithValue("@Path", model.Path);
                   paramCollection.AddWithValue("@MediaType", model.Media);
                   paramCollection.AddWithValue("@UserType", model.User);
                   paramCollection.AddWithValue("@Title", model.Title);
                   paramCollection.AddWithValue("@ContentType", model.ContentType);

                   SqlParameter p = new SqlParameter("@MediaId", System.Data.SqlDbType.Int);
                   p.Direction = System.Data.ParameterDirection.Output;

                   paramCollection.Add(p);

                   

               }, returnParameters: delegate(SqlParameterCollection param)
               {
                   mediaId = Convert.ToInt32(param["@MediaId"].Value);
               }
               );
            return mediaId;
            
        }
        public void Update(ImageModel model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Medias_UpdateDescription",
                inputParamMapper: delegate(SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", model.ImageId);
                    if(model.Description==null)
                    {
                        paramCollection.AddWithValue("@Description", System.DBNull.Value);
                    }
                    else
                    {
                        paramCollection.AddWithValue("@Description", model.Description);
                    }
                    paramCollection.AddWithValue("@Title", model.Title);
                }, returnParameters: delegate(SqlParameterCollection param)
                {}
				);
        }

        //return a specific image
        public List<Media> GetPictureById(int selectedImage)
        {
            List<Media> mediaList = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Medias_Select"
               , inputParamMapper: delegate(SqlParameterCollection paramCollection)
               {
                    paramCollection.AddWithValue("@Id", selectedImage);                                    
               }
            , map: delegate(IDataReader reader, short set)
            {
                Media p = new Media();
                int startingIndex = 0; //startingOrdinal

                p.ImageId = reader.GetSafeInt32(startingIndex++);
                p.Path = reader.GetSafeString(startingIndex++);
                p.DateCreated = reader.GetSafeDateTime(startingIndex++);
                p.MediaEnum = reader.GetSafeEnum<MediaType>(startingIndex++);
                p.User = reader.GetSafeEnum<UserType>(startingIndex++);
                p.Description = reader.GetSafeString(startingIndex++);
                p.Title = reader.GetSafeString(startingIndex++);
                p.ContentType = reader.GetSafeString(startingIndex++);
                p.FullPath = baseUrl + p.Path;


                if (mediaList == null)
                {
                    mediaList = new List<Media>();
                }

                mediaList.Add(p);
            }
               );
            return mediaList;
            
        }

		//returns all Media for the user who is currently logged in
        public List<Media> GetPictureByCurrentUser()
        {
            List<Media> mediaList = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Medias_SelectImages"
               , inputParamMapper: delegate(SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", UserService.GetCurrentUserId());
               }
            , map: delegate(IDataReader reader, short set)
            {
                Media p = new Media();
                int startingIndex = 0; //startingOrdinal

                p.ImageId = reader.GetSafeInt32(startingIndex++);
                p.Path = reader.GetSafeString(startingIndex++);
                p.DateCreated = reader.GetSafeDateTime(startingIndex++);
                p.MediaEnum = reader.GetSafeEnum<MediaType>(startingIndex++);
                p.User = reader.GetSafeEnum<UserType>(startingIndex++);
                p.Description = reader.GetSafeString(startingIndex++);
                p.Title = reader.GetSafeString(startingIndex++);
                p.ContentType = reader.GetSafeString(startingIndex++);
                p.FullPath = baseUrl + p.Path;


                if (mediaList == null)
                {
                    mediaList = new List<Media>();
                }

                mediaList.Add(p);
            }
               );
            return mediaList;

        }

        //return all images for a selected user
        public List<BusinessEntity> GetOrganizationMedia(Guid selectedUser)
        {
            List<BusinessEntity> mediaList = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Medias_SelectOrgImages"
               , inputParamMapper: delegate(SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Uid", selectedUser);
               }
            , map: delegate(IDataReader reader, short set)
            {
                BusinessEntity user = new BusinessEntity();
                int startingIndex = 0; //startingOrdinal

                user.Id = reader.GetSafeInt32(startingIndex++);
                user.Uid = reader.GetGuid(startingIndex++);
                user.Created = reader.GetDateTime(startingIndex++);
                user.Name = reader.GetSafeString(startingIndex++);
                user.ContactName = reader.GetSafeString(startingIndex++);
                user.Phone = reader.GetSafeString(startingIndex++);
                user.AddressId = reader.GetSafeInt32(startingIndex++);
                user.Url = reader.GetSafeString(startingIndex++);
                user.Slug = reader.GetSafeString(startingIndex++);
                user.ExternalCustomerId = reader.GetSafeString(startingIndex++);
                user.SubscriptionId = reader.GetSafeInt32(startingIndex++);

                user.Media = new Media();
                user.Media.ImageId = reader.GetSafeInt32(startingIndex++);
                user.Media.Path = reader.GetSafeString(startingIndex++);
                user.Media.DateCreated = reader.GetSafeDateTime(startingIndex++);
                user.Media.MediaEnum = reader.GetSafeEnum<MediaType>(startingIndex++);
                user.Media.User = reader.GetSafeEnum<UserType>(startingIndex++);
                user.Media.Description = reader.GetSafeString(startingIndex++);
                user.Media.Title = reader.GetSafeString(startingIndex++);
                user.Media.ContentType = reader.GetSafeString(startingIndex++);
                user.Media.FullPath = baseUrl + user.Media.Path;

                if (mediaList == null)
                {
                    mediaList = new List<BusinessEntity>();
                }

                mediaList.Add(user);
            }
               );
            return mediaList;
        }

        public List<BusinessEntity> GetMerchantMedia(Guid selectedUser)
        {
            List<BusinessEntity> mediaList = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Medias_SelectMerchantImages"
               , inputParamMapper: delegate(SqlParameterCollection paramCollection)
               {
                   //  this is where your input params go. it works the same way as with ExecuteNonQuery. 
                   paramCollection.AddWithValue("@Uid", selectedUser);
               }
            , map: delegate(IDataReader reader, short set)
            {
                BusinessEntity user = new BusinessEntity();
                int startingIndex = 0; //startingOrdinal

                user.Id = reader.GetSafeInt32(startingIndex++);
                user.Uid = reader.GetGuid(startingIndex++);
                user.Created = reader.GetDateTime(startingIndex++);
                user.Name = reader.GetSafeString(startingIndex++);
                user.ContactName = reader.GetSafeString(startingIndex++);
                user.Phone = reader.GetSafeString(startingIndex++);
                user.AddressId = reader.GetSafeInt32(startingIndex++);
                user.Url = reader.GetSafeString(startingIndex++);
                user.Slug = reader.GetSafeString(startingIndex++);
                user.ExternalCustomerId = reader.GetSafeString(startingIndex++);
                user.SubscriptionId = reader.GetSafeInt32(startingIndex++);

                user.Media = new Media();
                user.Media.ImageId = reader.GetSafeInt32(startingIndex++);
                user.Media.Path = reader.GetSafeString(startingIndex++);
                user.Media.DateCreated = reader.GetSafeDateTime(startingIndex++);
                user.Media.MediaEnum = reader.GetSafeEnum<MediaType>(startingIndex++);
                user.Media.User = reader.GetSafeEnum<UserType>(startingIndex++);
                user.Media.Description = reader.GetSafeString(startingIndex++);
                user.Media.Title = reader.GetSafeString(startingIndex++);
                user.Media.ContentType = reader.GetSafeString(startingIndex++);
                user.Media.FullPath = baseUrl + user.Media.Path;

                if (mediaList == null)
                {
                    mediaList = new List<BusinessEntity>();
                }

                mediaList.Add(user);
            }
               );
            return mediaList;
        }

        public void DeleteById(int id)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Medias_DeleteById",
                inputParamMapper: delegate(SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", id);

                }, returnParameters: delegate(SqlParameterCollection param)
                {
                });
        }
    }
}