using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using StreamFileUpload.App.Models;

namespace StreamFileUpload.App.Controllers
{
    [Route("ajax-file-upload")]
    public class AjaxFileUploadController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AjaxFileUploadController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        
        [Route("add-file")]
        public IActionResult AddFile()
        {
            return View();
        }

        [Route("add-file")]
        [HttpPost]
        public async Task<IActionResult> SaveFileToPhysicalFolder()
        {
            var boundary = HeaderUtilities.RemoveQuotes(
                MediaTypeHeaderValue.Parse(Request.ContentType).Boundary
            ).Value;

            var reader = new MultipartReader(boundary, Request.Body);

            var section = await reader.ReadNextSectionAsync();

             var formAccumelator = new KeyValueAccumulator();

            while (section != null)
            {
                var hasContentDisposition = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition
                );

                if (hasContentDisposition)
                {
                    if (contentDisposition.DispositionType.Equals("form-data") &&
                    (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                    !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)))
                    {
                        string fileStoragePath = $"{_webHostEnvironment.WebRootPath}/images/";
                        string fileName = Path.GetRandomFileName() + ".jpg";
                        // uploaded files form fileds
                        byte[] fileByteArray;
                        using (var memoryStream = new MemoryStream())
                        {
                            await section.Body.CopyToAsync(memoryStream);
                            fileByteArray = memoryStream.ToArray();
                        }
                        using (var fileStream = System.IO.File.Create(Path.Combine(fileStoragePath,fileName)))
                        {
                            await fileStream.WriteAsync(fileByteArray);
                        }
                    }
                    else
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

                        using(var streamReader = new StreamReader(section.Body,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks:true,
                        bufferSize:1024,
                        leaveOpen:true)){
                            var value = await streamReader.ReadToEndAsync();
                            if(string.Equals(value, "undefined",StringComparison.OrdinalIgnoreCase)){
                                value = string.Empty;
                            }
                            formAccumelator.Append(key, value);
                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }
            var profile = new Profile();
            var formValueProvidere = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumelator.GetResults()),
                CultureInfo.CurrentCulture
            );

            var bindindSuccessfully = await TryUpdateModelAsync(profile,"",formValueProvidere);
            if(ModelState.IsValid){
                // write log to save profile data to database
            }
            

            return Content("Uploaded successfully");
        }

        [Route("modelbinding-add-file")]
        public IActionResult AddFileWithModelBinding()
        {
            return View();
        }
        [Route("modelbinding-add-file")]
        [HttpPost]
        public async Task<IActionResult> SaveFileUploadUsingModelBinding(Profile profile)
        {
            var boundary = HeaderUtilities.RemoveQuotes(
                MediaTypeHeaderValue.Parse(Request.ContentType).Boundary
            ).Value;

            var reader = new MultipartReader(boundary, Request.Body);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDisposition = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition
                );

                if (hasContentDisposition)
                {
                    if (contentDisposition.DispositionType.Equals("form-data") &&
                    (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                    !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)))
                    {
                        string fileStoragePath = $"{_webHostEnvironment.WebRootPath}/images/";
                        string fileName = Path.GetRandomFileName() + ".jpg";
                        // uploaded files form fileds
                        byte[] fileByteArray;
                        using (var memoryStream = new MemoryStream())
                        {
                            await section.Body.CopyToAsync(memoryStream);
                            fileByteArray = memoryStream.ToArray();
                        }
                        using (var fileStream = System.IO.File.Create(Path.Combine(fileStoragePath,fileName)))
                        {
                            await fileStream.WriteAsync(fileByteArray);
                        }
                    }
                }  
                section = await reader.ReadNextSectionAsync();
            }
           
            

            return Content("Uploaded successfully");
        }

    }
}