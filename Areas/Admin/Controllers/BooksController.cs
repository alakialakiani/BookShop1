using BookShop.Areas.Admin.Data;
using BookShop.Classes;
using BookShop.Models;
using BookShop.Models.Repository;
using BookShop.Models.UnitOfWork;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [DisplayName("مدیریت اسناد")]
    public class BooksController : Controller
    {
        private readonly IUnitOfWork _UW;
        private readonly IHostingEnvironment _env;
        public BooksController(IUnitOfWork UW, IHostingEnvironment env)
        {
            _UW = UW;
            _env = env;
        }

        [Authorize(Policy =ConstantPolicies.DynamicPermission)]
        [DisplayName("مشاهده اسناد")]
        public IActionResult Index(int page = 1, int row = 10, string sortExpression = "Title", string title = "")
        {
            title = String.IsNullOrEmpty(title) ? "" : title;
            List<int> Rows = new List<int>
            {
                5,10,15,20,50,100
            };

            ViewBag.RowID = new SelectList(Rows, row);
            ViewBag.NumOfRow = (page - 1) * row + 1;
            ViewBag.Search = title;

            var PagingModel = PagingList.Create(_UW.BooksRepository.GetAllBooks(title, "", "", "", "", "", ""), row, page, sortExpression, "Title");
            PagingModel.RouteValue = new RouteValueDictionary
            {
                {"row",row},
                {"title",title }
            };

            ViewBag.Categories = _UW.BooksRepository.GetAllCategories();
            ViewBag.LanguageID = new SelectList(_UW.BaseRepository<Language>().FindAll(), "LanguageName", "LanguageName");
            ViewBag.PublisherID = new SelectList(_UW.BaseRepository<Publisher>().FindAll(), "PublisherName", "PublisherName");
            ViewBag.AuthorID = new SelectList(_UW.BaseRepository<Author>().FindAll().Select(t => new AuthorList { AuthorID = t.AuthorID, NameFamily = t.FirstName + " " + t.LastName }), "NameFamily", "NameFamily");
            ViewBag.TranslatorID = new SelectList(_UW.BaseRepository<Translator>().FindAll().Select(t => new TranslatorList { TranslatorID = t.TranslatorID, NameFamily = t.Name + " " + t.Family }), "NameFamily", "NameFamily");

            return View(PagingModel);
        }


        public IActionResult AdvancedSearch(BooksAdvancedSearch ViewModel)
        {
            ViewModel.Title = String.IsNullOrEmpty(ViewModel.Title) ? "" : ViewModel.Title;
            ViewModel.ISBN = String.IsNullOrEmpty(ViewModel.ISBN) ? "" : ViewModel.ISBN;
            ViewModel.Publisher = String.IsNullOrEmpty(ViewModel.Publisher) ? "" : ViewModel.Publisher;
            ViewModel.Author = String.IsNullOrEmpty(ViewModel.Author) ? "" : ViewModel.Author;
            ViewModel.Translator = String.IsNullOrEmpty(ViewModel.Translator) ? "" : ViewModel.Translator;
            ViewModel.Category = String.IsNullOrEmpty(ViewModel.Category) ? "" : ViewModel.Category;
            ViewModel.Language = String.IsNullOrEmpty(ViewModel.Language) ? "" : ViewModel.Language;
            var Books = _UW.BooksRepository.GetAllBooks(ViewModel.Title, ViewModel.ISBN, ViewModel.Language, ViewModel.Publisher, ViewModel.Author, ViewModel.Translator, ViewModel.Category);
            return View(Books);
        }

        [Authorize(Policy = ConstantPolicies.DynamicPermission)]
        [DisplayName("افزودن سند جدید")]
        public IActionResult Create()
        {
            ViewBag.LanguageID = new SelectList(_UW.BaseRepository<Language>().FindAll(), "LanguageID", "LanguageName");
            ViewBag.PublisherID = new SelectList(_UW.BaseRepository<Publisher>().FindAll(), "PublisherID", "PublisherName");
            ViewBag.AuthorID = new SelectList(_UW.BaseRepository<Author>().FindAll().Select(t => new AuthorList { AuthorID = t.AuthorID, NameFamily = t.FirstName + " " + t.LastName }), "AuthorID", "NameFamily");
            ViewBag.TranslatorID = new SelectList(_UW.BaseRepository<Translator>().FindAll().Select(t => new TranslatorList { TranslatorID = t.TranslatorID, NameFamily = t.Name + " " + t.Family }), "TranslatorID", "NameFamily");

            BooksSubCategoriesViewModel SubCategoriesVM = new BooksSubCategoriesViewModel(_UW.BooksRepository.GetAllCategories(), null);
            BooksCreateEditViewModel ViewModel = new BooksCreateEditViewModel(SubCategoriesVM);
            return View(ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BooksCreateEditViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                UploadFileResult result = new UploadFileResult();
                string NewFileName = null;
                if (ViewModel.File != null)
                {
                    NewFileName = _UW.BooksRepository.CheckFileName(ViewModel.File.FileName);
                    var path = $"{_env.WebRootPath}/BookFiles/{NewFileName}";
                    result = await _UW.BooksRepository.UploadFileAsync(ViewModel.File, path);
                }

                if (result.IsSuccess == true || result.IsSuccess==null)
                {
                    ViewModel.FileName = NewFileName;
                    if (await _UW.BooksRepository.CreateBookAsync(ViewModel))
                        return RedirectToAction("Index");
                    else
                        ViewBag.Error = "در انجام عملیات خطایی رخ داده است.";
                }

                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item);
                    }
                }
            }

            ViewBag.LanguageID = new SelectList(_UW.BaseRepository<Language>().FindAll(), "LanguageID", "LanguageName");
            ViewBag.PublisherID = new SelectList(_UW.BaseRepository<Publisher>().FindAll(), "PublisherID", "PublisherName");
            ViewBag.AuthorID = new SelectList(_UW.BaseRepository<Author>().FindAll().Select(t => new AuthorList { AuthorID = t.AuthorID, NameFamily = t.FirstName + " " + t.LastName }), "AuthorID", "NameFamily");
            ViewBag.TranslatorID = new SelectList(_UW.BaseRepository<Translator>().FindAll().Select(t => new TranslatorList { TranslatorID = t.TranslatorID, NameFamily = t.Name + " " + t.Family }), "TranslatorID", "NameFamily");
            ViewModel.SubCategoriesVM = new BooksSubCategoriesViewModel(_UW.BooksRepository.GetAllCategories(), ViewModel.CategoryID);
            return View(ViewModel);
        }

        [Authorize(Policy = ConstantPolicies.DynamicPermission)]
        [DisplayName("مشاهده جزئیات سند")]
        public IActionResult Details(int id)
        {
            var BookInfo = _UW._Context.Query<ReadAllBook>().Where(b => b.BookID == id).First();
          
            return PartialView(BookInfo);
        }

        [Authorize(Policy = ConstantPolicies.DynamicPermission)]
        [DisplayName("حذف سند")]
        public async Task<IActionResult> Delete(int id)
        {
            var Book =await _UW.BaseRepository<Book>().FindByIDAsync(id);
            if (Book != null)
            {
                var path = $"{_env.WebRootPath}/BookFiles/{Book.File}";
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    Book.File = null;
                }
            
                Book.Delete = true;
                await _UW.Commit();
                return RedirectToAction("Index");
            }

            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Authorize(Policy = ConstantPolicies.DynamicPermission)]
        [DisplayName("ویرایش اطلاعات سند")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            else
            {
                var Book = await _UW.BaseRepository<Book>().FindByIDAsync(id);
                if (Book == null)
                {
                    return NotFound();
                }

                else
                {
                    var ViewModel = (from b in _UW._Context.Books.Include(l => l.Language)
                                     .Include(p => p.Publisher)
                                     where (b.BookID == id)
                                     select new BooksCreateEditViewModel
                                     {
                                         BookID = b.BookID,
                                         Title = b.Title,
                                         ISBN = b.ISBN,
                                         NumOfPages = b.NumOfPages,
                                         Price = b.Price,
                                         Stock = b.Stock,
                                         IsPublish = (bool)b.IsPublish,
                                         LanguageID = b.LanguageID,
                                         PublisherID = b.Publisher.PublisherID,
                                         PublishYear = b.PublishYear,
                                         Summary = b.Summary,
                                         Weight = b.Weight,
                                         PublishDate = b.PublishDate,
                                         ImageByte=b.Image,
                                         FileName=b.File,
                                     }).FirstAsync();

                    int[] AuthorsArray = await (from a in _UW._Context.Author_Books
                                                where (a.BookID == id)
                                                select a.AuthorID).ToArrayAsync();

                    int[] TranslatorsArray = await (from t in _UW._Context.Book_Translators
                                                    where (t.BookID == id)
                                                    select t.TranslatorID).ToArrayAsync();

                    int[] CategoriesArray = await (from c in _UW._Context.Book_Categories
                                                   where (c.BookID == id)
                                                   select c.CategoryID).ToArrayAsync();

                    ViewModel.Result.AuthorID = AuthorsArray;
                    ViewModel.Result.TranslatorID = TranslatorsArray;
                    ViewModel.Result.CategoryID = CategoriesArray;

                    ViewBag.LanguageID = new SelectList(_UW.BaseRepository<Language>().FindAll(), "LanguageID", "LanguageName");
                    ViewBag.PublisherID = new SelectList(_UW.BaseRepository<Publisher>().FindAll(), "PublisherID", "PublisherName");
                    ViewBag.AuthorID = new SelectList(_UW.BaseRepository<Author>().FindAll().Select(t => new AuthorList { AuthorID = t.AuthorID, NameFamily = t.FirstName + " " + t.LastName }), "AuthorID", "NameFamily");
                    ViewBag.TranslatorID = new SelectList(_UW.BaseRepository<Translator>().FindAll().Select(t => new TranslatorList { TranslatorID = t.TranslatorID, NameFamily = t.Name + " " + t.Family }), "TranslatorID", "NameFamily");
                    ViewModel.Result.SubCategoriesVM = new BooksSubCategoriesViewModel(_UW.BooksRepository.GetAllCategories(), CategoriesArray);

                    return View(await ViewModel);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BooksCreateEditViewModel ViewModel)
        {
            ViewBag.LanguageID = new SelectList(_UW.BaseRepository<Language>().FindAll(), "LanguageID", "LanguageName");
            ViewBag.PublisherID = new SelectList(_UW.BaseRepository<Publisher>().FindAll(), "PublisherID", "PublisherName");
            ViewBag.AuthorID = new SelectList(_UW.BaseRepository<Author>().FindAll().Select(t => new AuthorList { AuthorID = t.AuthorID, NameFamily = t.FirstName + " " + t.LastName }), "AuthorID", "NameFamily");
            ViewBag.TranslatorID = new SelectList(_UW.BaseRepository<Translator>().FindAll().Select(t => new TranslatorList { TranslatorID = t.TranslatorID, NameFamily = t.Name + " " + t.Family }), "TranslatorID", "NameFamily");
            ViewModel.SubCategoriesVM = new BooksSubCategoriesViewModel(_UW.BooksRepository.GetAllCategories(), ViewModel.CategoryID);

            if (ModelState.IsValid)
            {
                UploadFileResult result = new UploadFileResult();
                string NewFileName = ViewModel.FileName;
                string path;
                if (ViewModel.File != null)
                {
                    NewFileName = _UW.BooksRepository.CheckFileName(ViewModel.File.FileName);
                    path = $"{_env.WebRootPath}/BookFiles/{NewFileName}";
                    result = await _UW.BooksRepository.UploadFileAsync(ViewModel.File, path);
                }

                if (result.IsSuccess == true || result.IsSuccess == null)
                {
                    if(result.IsSuccess==true)
                    {
                        path = $"{_env.WebRootPath}/BookFiles/{ViewModel.FileName}";
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }

                    ViewModel.FileName = NewFileName;
                    var operationResult = await _UW.BooksRepository.EditBookAsync(ViewModel);
                    if (operationResult.IsSuccess==true)
                    {
                        ViewBag.MsgSuccess = "ذخیره تغییرات با موفقیت انجام شد.";
                        return View(ViewModel);
                    }
                    else
                    {
                        foreach(var item in operationResult.Errors)
                            ModelState.AddModelError("", item);
                        return View(ViewModel);
                    }
                }

                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item);
                    }

                    return View(ViewModel);
                }

            }

            else
            {
                ViewBag.MsgFailed = "اطلاعات فرم نامعتبر است.";
                return View(ViewModel);
            }
        }

        public async Task<IActionResult> SearchByIsbn(string ISBN)
        {
            if (ISBN != null)
            {
                var Book = (from b in _UW._Context.Books
                            where (b.ISBN == ISBN)
                            select new BooksIndexViewModel
                            {
                                Title = b.Title,
                                Author = BookShopContext.GetAllAuthors(b.BookID),
                                Translator = BookShopContext.GetAllTranslators(b.BookID),
                                Category = BookShopContext.GetAllCategories(b.BookID),
                            }).FirstOrDefaultAsync();

                if (Book.Result == null)
                {
                    ViewBag.Msg = "سندی با این شابک پیدا نشد.";
                }

                return View(await Book);
            }

            else
            {
                return View();
            }

        }


        public async Task<IActionResult> Download(int id)
        {
            var Book = await _UW.BaseRepository<Book>().FindByIDAsync(id);
            if (Book == null)
                return NotFound();
            var Path = $"{_env.WebRootPath}/BookFiles/{Book.File}";
            var memory = new MemoryStream();
            using (var stream = new FileStream(Path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, FileExtentions.GetContentType(Path), Book.File);
        }

        public async Task<IActionResult> ViewImage(int id)
        {
            var Book = await _UW.BaseRepository<Book>().FindByIDAsync(id);
            if (Book == null)
                return NotFound();
            var memoryStream = new MemoryStream(Book.Image);
            return new FileStreamResult(memoryStream, "image/png");

        }


        [HttpGet]
        public IActionResult InsertOrUpdateBookImage(int id)
        {
            var Book = _UW._Context.Books.Where(b => b.BookID == id).Select(b => new ImageBookViewModel { BookID = b.BookID, ImageByte = b.Image }).FirstOrDefault();
            if (Book == null)
                ModelState.AddModelError("", "سند با این مشخصات یافت نشد.");

            return PartialView("_InsertOrUpdateBookImage", Book);
        }

        [HttpPost]
        public async Task<IActionResult> InsertOrUpdateBookImage(ImageBookViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                var Book = await _UW.BaseRepository<Book>().FindByIDAsync(ViewModel.BookID);
                using (var memorySteam = new MemoryStream())
                {
                    string FileExtension = Path.GetExtension(ViewModel.Image.FileName);
                    await ViewModel.Image.CopyToAsync(memorySteam);
                    var types = FileExtentions.FileType.Image;
                    bool result = FileExtentions.IsValidFile(memorySteam.ToArray(), types, FileExtension.Replace('.', ' '));
                    if (result)
                    {
                        Book.Image = memorySteam.ToArray();
                        await _UW.Commit();
                        ViewModel.ImageByte = memorySteam.ToArray();
                        TempData["Notifications"] = "آپلود فایل با موفقیت انجام شد.";
                    }
                    else
                        ModelState.AddModelError("", "فایل تصویر سند نامعتبر است.");
                }
            }
            return PartialView("_InsertOrUpdateBookImage", ViewModel);
        }
    }
}