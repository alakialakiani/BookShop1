using BookShop.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Areas.Api.Services
{
    public interface IjwtService
    {
        Task<string> GenerateTokenAsync(ApplicationUser User);
    }
}
