using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class ProductService : IProductService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public ProductService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Guid> AddProduct(ProductDto dto)
        {
            using (var context = _dbContextFactory.Create())
            {
                Product model = DtoHelper.Convert(dto);
                var user = await context.Users.Where(u => u.Email == dto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    model.CreatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.CreatedBy);
                }
                var result = await context.Products.AddAsync(model);
                await context.SaveChangesAsync();
                return result.Entity.Id;
            }
        }

        public bool DeleteProduct(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.Products.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Products.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Product> FindProduct(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Products.Where(v => v.Id == id)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Images)
                    .Include(x => x.Videos)
                    .Include(x => x.ProductCategory).ThenInclude(x => x.Category)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<IEnumerable<Product>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Product> results;
                if (isDescending)
                {
                    if (orderBy == "Title")
                    {
                        results = await dbContext.Products.OrderByDescending(x => x.Title)
                            .Skip(skip)
                            .Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Images)
                            .Include(x => x.Videos)
                            .Include(x => x.ProductCategory).ThenInclude(x => x.Category)
                            .ToListAsync();
                    }
                    else
                    {
                        results = await dbContext.Products.OrderByDescending(x => x.CreatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Images)
                            .Include(x => x.Videos)
                            .Include(x => x.ProductCategory).ThenInclude(x => x.Category)
                            .ToListAsync();
                    }
                }
                else
                {
                    if (orderBy == "Title")
                    {
                        results = await dbContext.Products.OrderBy(x => x.Title)
                            .Skip(skip)
                            .Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Images)
                            .Include(x => x.Videos)
                            .Include(x => x.ProductCategory).ThenInclude(x => x.Category)
                            .ToListAsync();
                    }
                    else
                    {
                        results = await dbContext.Products.OrderBy(x => x.CreatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Images)
                            .Include(x => x.Videos)
                            .Include(x => x.ProductCategory).ThenInclude(x => x.Category)
                            .ToListAsync();
                    }
                }

                return results;
            };
        }

        public async Task<Product> UpdateProduct(ProductDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Products.FindAsync(dto.Id);
                if (temp == null)
                {
                    throw new ProductNotFoundException(dto.Id);
                }
                temp.Title = dto.Title;
                temp.Description = dto.Description;
                temp.Price = dto.Price;
                temp.Brand = dto.Brand;
                temp.Inventory = dto.Inventory;
                temp.UpdatedOn = DateTimeOffset.UtcNow;
                var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == dto.UpdatedBy);
                if (user != null)
                {
                    temp.UpdatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.UpdatedBy);
                }

                //TODO update images and videos
                await dbContext.SaveChangesAsync();
                return temp;
            }
        }
    }
}
