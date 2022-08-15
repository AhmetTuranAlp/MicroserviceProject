using CatalogService.Api.Core.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>().WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timespan, retry, ctx) =>
                 {
                     logger.LogWarning(exception, "[{prefix}] Exception [ExceptionType] with message {Message} detected on attempt {retry} or ");
                 });

            var setupDirPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "SendFiles");
            var picturePath = "Pics";
            await policy.ExecuteAsync(() => ProccessSeeding(context, setupDirPath, picturePath, logger));
        }

        private async Task ProccessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }
            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }
            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirPath, context));
                await context.SaveChangesAsync();

                GetCatalogPictures(setupDirPath, picturePath);
            }
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrand()
            {
                return new List<CatalogBrand>()
                {
                    new CatalogBrand(){Brand = "Azure"},
                    new CatalogBrand(){Brand = ".NET"},
                    new CatalogBrand(){Brand = "Microservice"},
                    new CatalogBrand(){Brand = "RabbitMQ"},
                    new CatalogBrand(){Brand = "SQL Server"},
                    new CatalogBrand(){Brand = "MongoDB"},
                    new CatalogBrand(){Brand = "Other"}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogBrands.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogBrand();
            }

            var fileContent = File.ReadAllLines(fileName);
            var list = fileContent.Select(x => new CatalogBrand()
            {
                Brand = x.Trim('"')
            }).Where(x => x != null);

            return list ?? GetPreconfiguredCatalogBrand();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreconfiguredCatalogType()
            {
                return new List<CatalogType>()
                {
                    new CatalogType(){Type = "Yazılım"},
                    new CatalogType(){Type = "Donanım"},
                    new CatalogType(){Type = "Cloud"}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogType();
            }

            var fileContent = File.ReadAllLines(fileName);
            var list = fileContent.Select(x => new CatalogType()
            {
                Type = x.Trim('"')
            }).Where(x => x != null);

            return list ?? GetPreconfiguredCatalogType();
        }

        public IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfiguredItems()
            {
                return new List<CatalogItem>()
                {
                    new CatalogItem{CatalogTypeId = 3,CatalogBrandId = 2,AvailableStock =100,Description ="ServiceBus Imp.",Name ="ServiceBus Imp.",Price =1000,PictureFileName = "1.png",},
                    new CatalogItem{CatalogTypeId = 2,CatalogBrandId = 3,AvailableStock =100,Description ="ServiceBus Imp.",Name ="ServiceBus Imp.",Price =1000,PictureFileName = "1.png",},
                    new CatalogItem{CatalogTypeId = 2,CatalogBrandId = 4,AvailableStock =100,Description ="ServiceBus Imp.",Name ="ServiceBus Imp.",Price =1000,PictureFileName = "1.png",}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogItems.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredItems();
            }

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(x => x.Type, x => x.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(x => x.Brand, x => x.Id);

            var fileContent = File.ReadAllLines(fileName)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new CatalogItem()
                {
                    CatalogTypeId = catalogTypeIdLookup[x[0]],
                    CatalogBrandId = catalogBrandIdLookup[x[0]],
                    Description = x[2].Trim('"').Trim(),
                    Name = x[3].Trim('"').Trim(),
                    Price = Decimal.Parse(x[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    PictureFileName = x[5].Trim('"').Trim(),
                    AvailableStock = string.IsNullOrEmpty(x[6]) ? 0 : int.Parse(x[6]),
                    OnReorder = Convert.ToBoolean(x[7])
                });
            return fileContent;
        }

        public void GetCatalogPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";
            if (picturePath != null)
            {
                DirectoryInfo directory = new DirectoryInfo(picturePath);
                foreach (var file in directory.GetFiles())
                {
                    file.Delete();
                }

                string zipFileCatalogPictures = Path.Combine(contentPath, "CatalogItems.zip");
                ZipFile.ExtractToDirectory(zipFileCatalogPictures, picturePath);
            }
        }
    }
}
