using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Areas.Api.Classes;
using BookShop.Models;
using BookShop.Models.UnitOfWork;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Api.Controllers.v1
{
    //[Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    [ApiResultFilter]
    public class BooksApiController : ControllerBase
    {
        private readonly IUnitOfWork _UW;
        public BooksApiController(IUnitOfWork UW)
        {
            _UW = UW;
        }

        [HttpGet]
        public ApiResult<List<BooksIndexViewModel>> GetBooks()
        {
            return Ok(_UW.BooksRepository.GetAllBooks("", "", "", "", "", "", ""));
        }

        [HttpPost]
        public async Task<ApiResult> CreateBook(BooksCreateEditViewModel ViewModel)
        {
            if (await _UW.BooksRepository.CreateBookAsync(ViewModel))
            {
                return Ok();
            }

            else
            {
                return BadRequest("در انجام عملیات خطایی رخ داده است.");
            }
        }

        [HttpPut]
        public async Task<ApiResult> EditBook(BooksCreateEditViewModel ViewModel)
        {
            var result = await _UW.BooksRepository.EditBookAsync(ViewModel);
            if (result.IsSuccess==true)
                return Ok();
            else
                return BadRequest("در انجام عملیات خطایی رخ داده است.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var Book = await _UW.BaseRepository<Book>().FindByIDAsync(id);
            if (Book != null)
            {
                Book.Delete = true;
                await _UW.Commit();
                return Ok();
            }

            else
            {
                return NotFound();
            }
        }
    }
}