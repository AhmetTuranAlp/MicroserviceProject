using Common.Base;
using Common.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PictureServices.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PictureServices.Controllers
{
    public class PictureController : Controller
    {
        [Route("api/[controller]")]
        [ApiController]
        public class ImageController : CustomBaseController
        {
            [HttpPost]
            public async Task<IActionResult> ImageSave(IFormFile image /*CancellationToken cancellationToken*/)
            {
                //CancellationToken : Kullanıcı kaydetme işlemini iptal ettiğinde burada da iptal edilsin.
                if (image != null && image.Length > 0)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.FileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await image.CopyToAsync(stream/*, cancellationToken*/);

                    var returnPath = image.FileName;

                    PictureDto pictureDto = new() { Url = returnPath };

                    return CreateActionResultInstance(Response<PictureDto>.Success(pictureDto, StaticValue._successReturnModelId));
                }

                return CreateActionResultInstance(Response<PictureDto>.Fail(StaticValue._imageEmpty, StaticValue._badRequest));
            }


            [HttpDelete]
            public IActionResult ImageDelete(string imageUrl)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageUrl);
                if (!System.IO.File.Exists(path))
                {
                    return CreateActionResultInstance(Response<NoContent>.Fail(StaticValue._imageNotFound, StaticValue._notFoundId));
                }

                System.IO.File.Delete(path);

                return CreateActionResultInstance(Response<NoContent>.Success(StaticValue._successReturnNotModelId));
            }

            [HttpGet]
            public async Task<IActionResult> GetImagePath()
            {
                PictureDto pictureDto = new PictureDto();
                pictureDto.Url = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");           
                return CreateActionResultInstance(Response<PictureDto>.Success(pictureDto, StaticValue._successReturnModelId));
            }
        }
    }
}
