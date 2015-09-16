using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.IO;
using Sabio.Web.Models.Requests;
using Sabio.Web.Enums;
using Sabio.Web.Services;
using Sabio.Web.Services.Interfaces;
using Sabio.Web.Models.Responses;
using Sabio.Web.Domain;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/fileUpload")]
    public class FileUploadApiController : ApiController
    {
        //  create a private property to attach the service when it is injected - just like angular
        private IMediaService _MediaService;
        private IFileService _fileService;

    //  we tell Unity to give us an instance of IRequestTokenService by putting it here in the constructor signature
    //  Unity knows which service to give us (in this case RequestTokenService) because we tell it in App_Start/UnityConfig.cs
        public FileUploadApiController(IMediaService MediaService, IFileService FileService)
        {
            _MediaService = MediaService;
            _fileService = FileService;
        }
        
        [Route(""), HttpPost]
        public HttpResponseMessage Post()
        {
            HttpResponseMessage result = null;
            var currentUser = UserService.GetCurrentUserId();
            
            List<Media> existingImages = new List<Media>();
            existingImages = _MediaService.GetPictureByCurrentUser();
            if (existingImages == null || existingImages.Count<Media>() < 5)
            {
                
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        var fileType = postedFile.ContentType;
                        var filePath = HttpContext.Current.Server.MapPath("~/PhotoDemo/" + postedFile.FileName); //sets filepath for server storage
                        postedFile.SaveAs(filePath);           //saves file to specified path
                        docfiles.Add(filePath);

                        ImageModel model = new ImageModel       //create model for INSERT 
                        {
                              //AppUserId taken from the Id of Organization
                            Path = Guid.NewGuid()+postedFile.FileName,
                            Media = MediaType.EntityImage,      //Uses Enum to specify type of media/image
                            User = SessionService.getCurrentUserType(),       // change when we can access login information
                            Title = postedFile.FileName,
                            ContentType = fileType
                        };
                        var id = _MediaService.UploadPicture(model, currentUser);
                        _fileService.UploadFile(filePath, model.Path, fileType);
                    }
                    
                    result = Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                {
                    ErrorResponse noFiles = new ErrorResponse("Cannot have more than 5 images");
                    result = Request.CreateResponse(HttpStatusCode.BadRequest, noFiles);
                }
            }
            else
            {
                ErrorResponse tooMany = new ErrorResponse("Cannot have more than 5 images");
                result = Request.CreateResponse(HttpStatusCode.BadRequest, tooMany);
            }
            return result;
        }
        
        [Route(""),HttpPut]
        public HttpResponseMessage PutDescription(ImageModel image)
        {
            HttpResponseMessage result = null;
            if(ModelState.IsValid)
            {
                if (image.Title == null || image.Title == "")
                {
                    List<Media> existingImage = _MediaService.GetPictureById(image.ImageId);
                    image.Title = existingImage[0].Title;
                }
                if (image.Description == null || image.Description == "")
                {
                    List<Media> existingImage = _MediaService.GetPictureById(image.ImageId);
                    image.Description = existingImage[0].Description;
                }

                _MediaService.Update(image);
                ItemResponse<bool> response = new ItemResponse<bool>();
                response.Item = true;
                result = Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            return result;
        }
        
        //Maybe needed for public profile
        [Route(""),HttpGet]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage result = null;

            ItemsResponse<Media> imageList = new ItemsResponse<Media>();
            imageList.Items = _MediaService.GetPictureByCurrentUser();

            result = Request.CreateResponse(HttpStatusCode.OK, imageList);

            return result;
        }
        
		//Deletes a previously uploaded image
        [Route("{imageId:int}"),HttpDelete]
        public HttpResponseMessage DeleteMedia(int imageId)
        {
            HttpResponseMessage result = null;

            List<Media> doomedItem = _MediaService.GetPictureById(imageId);
            _MediaService.DeleteById(imageId);
            _fileService.Delete(doomedItem[0].Path);

            ItemResponse<bool> deleteConfirm = new ItemResponse<bool>();
            deleteConfirm.Item = true;
            result = Request.CreateResponse(HttpStatusCode.OK, deleteConfirm.Item);
            return result;
        }
    }
}
