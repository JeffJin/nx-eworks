using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_web_api.Controllers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

[Route("api/common")]
public class CommonDataController : BaseApiController
{
    private readonly IAntiforgery _antiforgery;
    private readonly ICommonDataService _commonDataService;

    public CommonDataController(IAntiforgery antiforgery, IConfiguration configuration,
        ICommonDataService commonDataService, IUserService userService, ILogger logger): base(configuration, userService, logger)
    {
        _antiforgery = antiforgery;
        _commonDataService = commonDataService;
    }

    [HttpGet("xsrf")]
    [IgnoreAntiforgeryToken]
    public IActionResult Get()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return new ObjectResult(new {
            token = tokens.RequestToken,
            tokenName = tokens.HeaderName
        });
    }

    // GET api/common/categories
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IEnumerable<Category>> GetCategories()
    {
        var categories = await _commonDataService.GetCategories();

        return categories;
    }
}
