using Microsoft.EntityFrameworkCore;
using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using System;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace ServerApp.DAL.Seed
{
    public static class SeedData
    {
        public static string image_default = "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";

        public static async Task SeedAsync(ShopDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {   
                // Seed Images
                if (!context.Images.Any())
                {
                    context.Images.AddRange(
                        new Image
                        {
                            Name = "Image 1",
                            ImageData = Convert.FromBase64String(image_default),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Image
                        {
                            Name = "Image 2",
                            ImageData = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAUB"), // Dữ liệu Base64 khác
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    );
                    for (int i = 1; i <= 44; i++)
                    {
                        context.Images.Add(new Image
                        {
                            Name = $"Product {i}",
                            ImageData = Convert.FromBase64String(image_default),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    await context.SaveChangesAsync();
                }
                // Kiểm tra bảng Brands trước khi thêm sản phẩm
                if (!context.Brands.Any())
                {
                    context.Brands.AddRange(
                    new Brand { 
                            Name = "Samsung",
                            ImageId = 1,
                        IsActive = true 
                        },
                        new Brand { 
                            Name = "Applesgsgsfdg",
                            ImageId = 2,
                            IsActive = true 
                        }
                    );
                    for (int i = 1; i <= 10; i++)
                    {
                        context.Brands.Add(new Brand
                        {
                            Name = $"Brand {i}",
                            ImageId = i+2,
                            IsActive = true
                        });
                    }
                    await context.SaveChangesAsync();  // Đảm bảo đã lưu Brands trước khi thêm Products
                }

                // Thêm sản phẩm với BrandId hợp lệ
                if (!context.Products.Any())
                {
                    Random random = new Random();
                    for (int i = 1; i <= 15; i++)
                    {
                        context.Products.Add(new Product
                        {
                            Name = $"Product {i}",
                            Description = $"Description for Product {i}",
                            Price = 10000000+i*1000* random.Next(1, 10),
                            OldPrice = 10500000,
                            StockQuantity = 5*i* random.Next(1, 10),
                            BrandId = random.Next(1, 10),
                            ImageId = i+13,
                            Manufacturer = "Manufacturer 8",
                            IsActive = true,
                             Colors  = "Purple",
                            Discount = 12,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                    await context.SaveChangesAsync();
                }

                // Seed SpecificationType
                if (!context.SpecificationTypes.Any())
                {
                    context.SpecificationTypes.AddRange(
                        new SpecificationType { Name = " Pin " },
                        new SpecificationType { Name = "ScreenSize" },
                        new SpecificationType { Name = "RAM" },
                        new SpecificationType { Name = "Storage" }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed ProductSpecifications
                if (!context.ProductSpecifications.Any())
                {
                    context.ProductSpecifications.AddRange(
                        new ProductSpecification { ProductId = 1, SpecificationTypeId = 1, Value = "Black" },
                        new ProductSpecification { ProductId = 1, SpecificationTypeId = 2, Value = "6,5" },
                        new ProductSpecification { ProductId = 2, SpecificationTypeId = 1, Value = "White" },
                        new ProductSpecification { ProductId = 2, SpecificationTypeId = 3, Value = "8GB" },
                        new ProductSpecification { ProductId = 2, SpecificationTypeId = 4, Value = "128" }
                    );
                    await context.SaveChangesAsync();
                }
               

                // Commit transaction nếu mọi thứ thành công
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback transaction nếu xảy ra lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static void EnableIdentityInsert(ShopDbContext context, string tableName, bool enable)
        {
            var rawSql = enable
                ? $"SET IDENTITY_INSERT {tableName} ON;"
                : $"SET IDENTITY_INSERT {tableName} OFF;";
            context.Database.ExecuteSqlRaw(rawSql);
        }
    }
}
