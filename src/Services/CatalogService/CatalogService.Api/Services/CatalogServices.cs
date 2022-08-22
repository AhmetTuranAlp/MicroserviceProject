using AutoMapper;
using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Dtos;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using CatalogService.Api.Services.Interfaces;
using Common.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Api.Services
{
    public class CatalogServices : ICatalogServices
    {
        private readonly CatalogContext _catalogContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IMapper _mapper;

        public CatalogServices(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> catalogSettings, IMapper mapper)
        {
            _catalogContext = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
            _catalogSettings = catalogSettings.Value;
            catalogContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            _mapper = mapper;
        }

        public async Task<Response<List<CatalogBrandDto>>> CatalogBrandsAsync()
        {
            var model = await _catalogContext.CatalogBrands.ToListAsync();
            return Response<List<CatalogBrandDto>>.Success(_mapper.Map<List<CatalogBrandDto>>(model), StaticValue._successReturnModelId);
        }

        public async Task<Response<List<CatalogTypeDto>>> CatalogTypesAsync()
        {
            var model = await _catalogContext.CatalogTypes.ToListAsync();
            return Response<List<CatalogTypeDto>>.Success(_mapper.Map<List<CatalogTypeDto>>(model), StaticValue._successReturnModelId);
        }

        public List<CatalogItemDto> ChangeUriPlaceHolder(List<CatalogItemDto> items)
        {
            var baseUri = _catalogSettings.PicBaseUrl;
            foreach (var item in items)
            {
                if (item != null)
                    item.PictureUri = baseUri + item.PictureFileName;
            }
            return items;
        }

        public async Task<Response<CatalogItemDto>> CreateProductAsync(CatalogItemDto product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price
            };

            _catalogContext.CatalogItems.Add(item);
            var status = await _catalogContext.SaveChangesAsync();
            if (status > 0)
                return Response<CatalogItemDto>.Success(_mapper.Map<CatalogItemDto>(item), StaticValue._successReturnModelId);
            else
                return Response<CatalogItemDto>.Fail(StaticValue._productNotFound, StaticValue._notFoundId);
        }

        public async Task<Response<NoContent>> DeleteProductAsync(int id)
        {
            var product = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                _catalogContext.CatalogItems.Remove(product);
                var status = await _catalogContext.SaveChangesAsync();
                if (status <= 0)
                    return Response<NoContent>.Fail(StaticValue._productNotFound, StaticValue._notFoundId);

            }
            return Response<NoContent>.Success(StaticValue._successReturnNotModelId);
        }

        public async Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> GetItemsAsync(int pageSize, int pageIndex)
        {
            var totalItems = await _catalogContext.CatalogItems.LongCountAsync();
            var itemsOnPage = _mapper.Map<List<CatalogItemDto>>(await _catalogContext.CatalogItems
                .OrderBy(x => x.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync());

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return Response<PaginatedItemsViewModel<CatalogItemDto>>.Success(new PaginatedItemsViewModel<CatalogItemDto>(pageIndex, pageSize, totalItems, _mapper.Map<List<CatalogItemDto>>(itemsOnPage)), StaticValue._successReturnModelId);
        }

        public async Task<Response<List<CatalogItemDto>>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), value: x));
            if (!numIds.All(x => x.Ok))
            {
                return Response<List<CatalogItemDto>>.Success(new List<CatalogItemDto>(), StaticValue._successReturnModelId);
            }

            var idsToSelect = numIds.Select(x => x.value);
            var items = _mapper.Map<List<CatalogItemDto>>(await _catalogContext.CatalogItems.Where(x => idsToSelect.Contains(x.Id)).ToListAsync());
            items = ChangeUriPlaceHolder(items);
            return Response<List<CatalogItemDto>>.Success(items, StaticValue._successReturnModelId);
        }

        public async Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> ItemByBrandIdAsync(int? catalogBrandId, int pageSize, int pageIndex)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

            if (catalogBrandId.HasValue)
                root = root.Where(x => x.CatalogBrandId == catalogBrandId);

            var totalItems = await root.LongCountAsync();

            var itemsOnPage = _mapper.Map<List<CatalogItemDto>>(await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync());

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return Response<PaginatedItemsViewModel<CatalogItemDto>>.Success(new PaginatedItemsViewModel<CatalogItemDto>(pageIndex, pageSize, totalItems, _mapper.Map<List<CatalogItemDto>>(itemsOnPage)), StaticValue._successReturnModelId);
        }

        public async Task<Response<CatalogItemDto>> ItemByIdAsync(int id)
        {
            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);
            var baseUri = _catalogSettings.PicBaseUrl;
            if (item != null)
            {
                item.PictureUri = baseUri + item.PictureFileName;
                return Response<CatalogItemDto>.Success(_mapper.Map<CatalogItemDto>(item), StaticValue._successReturnModelId);
            }

            return null;
        }

        public async Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> ItemByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, int pageSize, int pageIndex)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;
            root = root.Where(x => x.CatalogTypeId == catalogTypeId);

            if (catalogBrandId.HasValue)
                root = root.Where(x => x.CatalogBrandId == catalogBrandId);

            var totalItems = await root.LongCountAsync();

            var itemsOnPage = _mapper.Map<List<CatalogItemDto>>(await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync());

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return Response<PaginatedItemsViewModel<CatalogItemDto>>.Success(new PaginatedItemsViewModel<CatalogItemDto>(pageIndex, pageSize, totalItems, _mapper.Map<List<CatalogItemDto>>(itemsOnPage)), StaticValue._successReturnModelId);
        }

        public async Task<Response<PaginatedItemsViewModel<CatalogItemDto>>> ItemWithNameAsync(string name, int pageSize, int pageIndex)
        {
            var totalItems = await _catalogContext.CatalogItems.Where(x => x.Name.StartsWith(name)).LongCountAsync();

            var itemsOnPage = _mapper.Map<List<CatalogItemDto>>(await _catalogContext.CatalogItems
                .OrderBy(x => x.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync());

            itemsOnPage = ChangeUriPlaceHolder(itemsOnPage);

            return Response<PaginatedItemsViewModel<CatalogItemDto>>.Success(new PaginatedItemsViewModel<CatalogItemDto>(pageIndex, pageSize, totalItems, _mapper.Map<List<CatalogItemDto>>(itemsOnPage)), StaticValue._successReturnModelId);
        }

        public async Task<Response<bool>> UpdateProductAsync(CatalogItemDto productToUpdate)
        {
            var catalogItems = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == productToUpdate.Id);
            if (catalogItems == null) return Response<bool>.Success(false, StaticValue._successReturnModelId);

            var oldPrice = catalogItems.Price;
            var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;

            catalogItems = _mapper.Map<CatalogItem>(productToUpdate);
            _catalogContext.CatalogItems.Update(catalogItems);
            var status = await _catalogContext.SaveChangesAsync();

            if (status > 0) return Response<bool>.Success(true, StaticValue._successReturnModelId);
            else return Response<bool>.Success(false, StaticValue._successReturnModelId);
        }
    }
}
