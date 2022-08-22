using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Api.Dtos
{
    public class CatalogItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureFileName { get; set; }
        public string PictureUri { get; set; }
        public int CatalogTypeId { get; set; }
        public CatalogTypeDto CatalogTypeDto { get; set; }
        public int CatalogBrandId { get; set; }
        public CatalogBrandDto CatalogBrandDto { get; set; }
        public int AvailableStock { get; set; }
        public bool OnReorder { get; set; }
    }
}
