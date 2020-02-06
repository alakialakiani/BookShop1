using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Areas.Api.Classes;
using BookShop.Areas.Api.Services;
using BookShop.Areas.Identity.Data;
using BookShop.Models.Repository;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Api.Controllers.v2
{
    //[Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class UsersApiController : v1.UsersApiController
    {
        public UsersApiController(IApplicationUserManager userManager, IUsersRepository usersRepository, IjwtService jwtService)
            :base(userManager,usersRepository,jwtService)
        {

        }

        public override Task<ApiResult> Register(RegisterBaseViewModel ViewModel)
        {
            return base.Register(ViewModel);
        }

        public override Task<ApiResult<List<UsersViewModel>>> Get()
        {
            return base.Get();
        }

        public override Task<ApiResult<UsersViewModel>> Get(string id)
        {
            return base.Get(id);
        }

        public override Task<ApiResult<string>> SignIn(SignInBaseViewModel ViewModel)
        {
            return base.SignIn(ViewModel);
        }
    }
}