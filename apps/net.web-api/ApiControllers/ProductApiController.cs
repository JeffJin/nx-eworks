using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/products")]
    public class ProductApiController : BaseApiController
    {
        private readonly IProductService _productService;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public ProductApiController(IProductService productService, IUserService userService,
            IConfiguration configuration, ILogger logger) : base(configuration, userService, logger)
        {
            _productService = productService;
        }

        // GET api/products
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<ProductDto>> Get()
        {
            IEnumerable<Product> products = await _productService.FindAll();
            return products.Select(DtoHelper.Convert);
        }

        // GET api/products/2505d672-2a15-4e26-b1de-30d29bfab9de
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ProductDto> GetDetails(Guid id)
        {
            Product product = await _productService.FindProduct(id);

            return DtoHelper.Convert(product);
        }


        // PUT api/products
        [HttpPut]
        public async Task<ProductDto> Put([FromBody] ProductDto dto)
        {
            try
            {
                dto.UpdatedBy = GetCurrentEmail();

                var result = await _productService.UpdateProduct(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update product", ex);
                throw;
            }
        }

        // DELETE api/products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                bool result = _productService.DeleteProduct(id);
                if (!result)
                {
                    return NotFound(id);
                }
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to delete product with id " + id);
                throw;
            }
        }
    }
}
