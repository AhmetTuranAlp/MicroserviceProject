using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Dtos;
using Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Api.Services.Interfaces
{
    public interface ICatalogServices
    {
        Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> GetItemsAsync(int pageSize, int pageIndex);

        Task<Response<List<CatalogItemDto>>> GetItemsByIdsAsync(string ids);

        List<CatalogItemDto> ChangeUriPlaceHolder(List<CatalogItemDto> items);

        Task<Response<CatalogItemDto>> ItemByIdAsync(int id);

        Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> ItemWithNameAsync(string name, int pageSize, int pageIndex);

        Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> ItemByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, int pageSize, int pageIndex);

        Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> ItemByBrandIdAsync(int? catalogBrandId, int pageSize, int pageIndex);

        Task<Response<List<CatalogTypeDto>>> CatalogTypesAsync();

        Task<Response<List<CatalogBrandDto>>> CatalogBrandsAsync();

        Task<Response<bool>> UpdateProductAsync(CatalogItemDto productToUpdate);

        Task<Response<CatalogItemDto>> CreateProductAsync(CatalogItemDto product);

        Task<Response<NoContent>> DeleteProductAsync(int id);
    }
}
