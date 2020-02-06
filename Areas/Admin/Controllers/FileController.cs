using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Models.ViewModels;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FileController : Controller
    {
        private readonly IHostingEnvironment _env;
        public FileController(IHostingEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files,string GalleryID)
        {
            try
            {
                foreach (var item in files)
                {
                    var UploadsRootFolder = Path.Combine(_env.WebRootPath, "GalleryFiles");
                    if (!Directory.Exists(UploadsRootFolder))
                        Directory.CreateDirectory(UploadsRootFolder);

                    if (item != null)
                    {
                        string FileExtension = Path.GetExtension(item.FileName);
                        string NewFileName = String.Concat(Guid.NewGuid().ToString(), FileExtension);
                        var path = Path.Combine(UploadsRootFolder, NewFileName);

                        //using (var strem = new FileStream(path, FileMode.Create))
                        //{
                        //    await item.CopyToAsync(strem);
                        //}

                        using (var memory = new MemoryStream())
                        {
                            await item.CopyToAsync(memory);
                            using (var Image = new MagickImage(memory.ToArray()))
                            {
                                Image.Resize(Image.Width / 2, Image.Height / 2);
                                //Image.Border(5);
                                //Image.Posterize(6);
                                //Image.Shade();
                                Image.Quality = 50;
                                Image.Write(path);
                            }
                        }
                        CompressImage(path);
                    }
                }

                //ViewBag.Alert = "آپلود فایل ها با موفقیت انجام شد.";
                //return View();
                return new JsonResult("success");
            }
            catch
            {
                return new EmptyResult();
            }
           
        }


        public IActionResult ImageProcess()
        {
            var FolderPath = $"{_env.WebRootPath}/images/";
            using (var Image = new MagickImage(FolderPath + "avatar-1.png"))
            {
                //Image.Resize(300, 300);
                Image.Quality = 50;
                Image.Write(FolderPath + "output-image-quality.jpg");
                CompressImage(FolderPath + "output-image-quality.jpg");
            }
            return View();
        }

        public void CompressImage(string path)
        {
            var Image = new FileInfo(path);
            var optimizer = new ImageOptimizer();
            optimizer.Compress(Image);
            Image.Refresh();
        }

        [HttpGet]
        public IActionResult UploadLargeFile()
        {
            return View();
        }

        [HttpPost,ValidateAntiForgeryToken]
        public IActionResult UploadLargeFile(UploadLargeFileViewModel ViewModel)
        {
            if(ModelState.IsValid)
            {
                //upload file
            }
            return View();
        }

        public IActionResult SaveImageToPdf()
        {
            var FolderPath = $"{_env.WebRootPath}/images/";
            using (var Image = new MagickImage(FolderPath + "logo-header.png"))
            {
                Image.Write(FolderPath + "PdfImage.pdf");
            }

            var FileStream = new FileStream(FolderPath + "PdfImage.pdf", FileMode.Open, FileAccess.Read);
            return new FileStreamResult(FileStream, "application/pdf");
        }

        [Authorize]
        [Route("{FileName}")]
        public IActionResult Video(string FileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Videos", FileName);
            return PhysicalFile(path, "application/octet-stream");
        }
    }
}