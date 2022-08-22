using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Dtos;
using CatalogService.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogServices _catalogService;

        public CatalogController(ICatalogServices catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ItemAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await _catalogService.GetItemsByIdsAsync(ids);
                if (!items.Data.Any())
                    return BadRequest("ids value invalid. Must be comma-separated list of numbers");

                return Ok(items);
            }

            var model = await _catalogService.GetItemsAsync(pageSize, pageIndex);
            return Ok(model);
        }


        [HttpGet]
        [Route("items/{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CatalogItemDto), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CatalogItemDto>> ItemByIdAsync(int id)
        {
            if (id <= 0) return BadRequest();

            var item = await _catalogService.ItemByIdAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }


        [HttpGet]
        [Route("items/withname/{name:minlength(1)}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItemDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItemDto>>> ItemWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            if (!string.IsNullOrEmpty(name) && name.Length > 2)
            {
                var items = await _catalogService.ItemWithNameAsync(name, pageSize, pageIndex);
                return Ok(items);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet]
        [Route("items/type/{catalogTypeId}/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItemDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItemDto>>> ItemByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var items = await _catalogService.ItemByTypeIdAndBrandIdAsync(catalogTypeId, catalogBrandId, pageSize, pageIndex);
            return Ok(items);
        }


        [HttpGet]
        [Route("items/type/all/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItemDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItemDto>>> ItemByBrandIdAsync(int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var items = await _catalogService.ItemByBrandIdAsync(catalogBrandId, pageSize, pageIndex);
            return Ok(items);
        }


        [HttpGet]
        [Route("catalogtypes")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogTypeDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogTypeDto>>> CatalogTypesAsync()
        {
            return Ok(await _catalogService.CatalogTypesAsync());
        }


        [HttpGet]
        [Route("catalogbrands")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogBrandDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogBrandDto>>> CatalogBrandsAsync()
        {
            return Ok(await _catalogService.CatalogBrandsAsync());
        }


        [HttpPut]
        [Route("items")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItemDto productToUpdate)
        {
            if (productToUpdate == null) return BadRequest();

            var status = await _catalogService.UpdateProductAsync(productToUpdate);
            if (status.Data == false)
                return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found." });

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = productToUpdate.Id }, null);
        }


        [HttpPost]
        [Route("items")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItemDto product)
        {
            if (product == null) return BadRequest();
            var status = await _catalogService.CreateProductAsync(product);
            if (status.Errors.Count == 0) return BadRequest();
            return CreatedAtAction(nameof(ItemByIdAsync), new { id = product.Id }, null);
        }


        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            if (id <= 0) return BadRequest();
            var status = await _catalogService.DeleteProductAsync(id);
            if (status.Errors.Count > 0) return BadRequest();
            return NoContent();
        }
    }
}
