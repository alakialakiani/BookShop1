using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookShop.Areas.Admin.Data;
using BookShop.Areas.Api.Attributes;
using BookShop.Areas.Api.Classes;
using BookShop.Areas.Api.Services;
using BookShop.Areas.Identity.Data;
using BookShop.Classes;
using BookShop.Models.Repository;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Api.Controllers.v1
{
    //[Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiResultFilter]
    [ApiVersion("1")]
    public class UsersApiController : ControllerBase
    {
        private readonly IApplicationUserManager _userManager;
        private readonly IUsersRepository _usersRepository;
        private readonly IjwtService _jwtService;
        public UsersApiController(IApplicationUserManager userManager, IUsersRepository usersRepository, IjwtService jwtService)
        {
            _userManager = userManager;
            _usersRepository = usersRepository;
            _jwtService = jwtService;
        }

        [HttpGet]
        //[Authorize]
        //[Authorize(Roles ="مدیر سایت")]
        [JwtAuthentication(Policy = ConstantPolicies.DynamicPermission)]
        //[Authorize(Policy = ConstantPolicies.DynamicPermission, AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        public virtual async Task<ApiResult<List<UsersViewModel>>> Get()
        {
            //string UserName = HttpContext.User.Identity.Name;
            //string PhoneNumber = HttpContext.User.FindFirstValue(ClaimTypes.MobilePhone);
            return Ok(await _userManager.GetAllUsersWithRolesAsync());
        }

        [HttpGet("{id}")]
        //[HttpGet("[action]")]
        public virtual async Task<ApiResult<UsersViewModel>> Get(string id)
        {
            var User = await _userManager.FindUserWithRolesByIdAsync(id);
            if (User == null)
                return NotFound();
            else
                return Ok(User);
        }

        [HttpPost("[action]")]
        //[HttpPost]
        public virtual async Task<ApiResult> Register(RegisterBaseViewModel ViewModel)
        {
            var result = await _usersRepository.RegisterAsync(ViewModel);
            if (result.Succeeded)
            {
                return BadRequest("عضویت با موفقیت انجام شد.");
            }

            else
                return BadRequest(result.Errors);
        }

        [HttpPost("[action]")]
        //[HttpPost]
        public virtual async Task<ApiResult<string>> SignIn(SignInBaseViewModel ViewModel)
        {
            var User = await _userManager.FindByNameAsync(ViewModel.UserName);
            if (User == null)
                return BadRequest("نام کاربری یا کلمه عبور شما صحیح نمی باشد.");
            else
            {
                var result = await _userManager.CheckPasswordAsync(User, ViewModel.Password);
                if (result)
                    return Ok(await _jwtService.GenerateTokenAsync(User));
                else
                    return BadRequest("نام کاربری یا کلمه عبور شما صحیح نمی باشد.");
            }

        }
    }
}