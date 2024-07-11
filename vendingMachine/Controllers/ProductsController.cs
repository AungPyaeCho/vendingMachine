using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vendingMachine.Data;
using vendingMachine.Models;

namespace vendingMachine.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext appDbContext, ILogger<ProductsController> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        // Allow anyone to view the product list
        [AllowAnonymous]
        [ActionName("ProductIndexUser")]
        public async Task<IActionResult> ProductIndexUser(int pageNo = 1, int pageSize = 10, string sortField = "Id", string sortOrder = "asc")
        {
            try
            {
                var (list, pageCount) = await GetSortedProducts(pageNo, pageSize, sortField, sortOrder);

                ProductResponseModel response = new()
                {
                    productData = list,
                    pageSize = pageSize,
                    pageCount = pageCount,
                    pageNo = pageNo,
                    sortField = sortField,
                    sortOrder = sortOrder
                };

                return View("ProductIndexUser", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for user view.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("ProductIndexUser");
            }
        }

        // Allow anyone to purchase products
        [AllowAnonymous]
        [HttpPost]
        [ActionName("Purchase")]
        public async Task<IActionResult> PurchaseProduct(int productId)
        {
            try
            {
                var product = await _appDbContext.Product.FindAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                if (product.productQuantity > 0)
                {
                    using (var transaction = _appDbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            TransactionModel transactionLog = new TransactionModel
                            {
                                productId = productId,
                                Quantity = 1,
                                TransactionDate = DateTime.UtcNow
                            };

                            _appDbContext.Transaction.Add(transactionLog);

                            product.productQuantity--;

                            await _appDbContext.SaveChangesAsync();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError(ex, "Error processing purchase transaction.");
                            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Product is out of stock.");
                }

                return RedirectToAction("ProductIndexUser");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing product.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return RedirectToAction("ProductIndexUser");
            }
        }

        // Only allow admins to access these actions
        [Authorize(Roles = "Admin")]
        [ActionName("ProductIndex")]
        public async Task<IActionResult> ProductIndex(int pageNo = 1, int pageSize = 10, string sortField = "Id", string sortOrder = "asc")
        {
            try
            {
                var (list, pageCount) = await GetSortedProducts(pageNo, pageSize, sortField, sortOrder);

                ProductResponseModel response = new()
                {
                    productData = list,
                    pageSize = pageSize,
                    pageCount = pageCount,
                    pageNo = pageNo,
                    sortField = sortField,
                    sortOrder = sortOrder
                };

                return View("ProductIndex", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for admin view.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("ProductIndex");
            }
        }

        [Authorize(Roles = "Admin")]
        [ActionName("Edit")]
        public IActionResult ProductEdit(int id)
        {
            try
            {
                var item = _appDbContext.Product.FirstOrDefault(x => x.Id == id);
                if (item == null)
                {
                    return Redirect("/Products/ProductIndex");
                }
                return View("ProductEdit", item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing product.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("ProductEdit");
            }
        }

        [Authorize(Roles = "Admin")]
        [ActionName("Create")]
        public IActionResult ProductCreate()
        {
            return View("ProductCreate");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ActionName("Save")]
        public async Task<IActionResult> ProductSave(ProductModel productModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _appDbContext.Product.Add(productModel);
                    await _appDbContext.SaveChangesAsync();
                    return RedirectToAction("ProductIndex");
                }
                else
                {
                    return View("ProductCreate", productModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("ProductCreate", productModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ActionName("Update")]
        public async Task<IActionResult> ProductUpdate(int id, ProductModel productModel)
        {
            try
            {
                var item = await _appDbContext.Product.FirstOrDefaultAsync(x => x.Id == id);
                if (item == null)
                {
                    return Redirect("/Products/ProductIndex");
                }

                item.productName = productModel.productName;
                item.productPrice = productModel.productPrice;
                item.productQuantity = productModel.productQuantity;

                await _appDbContext.SaveChangesAsync();

                return RedirectToAction("ProductIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("ProductEdit", productModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> ProductDelete(int id)
        {
            try
            {
                var item = await _appDbContext.Product.FirstOrDefaultAsync(x => x.Id == id);
                if (item == null)
                {
                    return Redirect("/Products");
                }

                _appDbContext.Product.Remove(item);
                await _appDbContext.SaveChangesAsync();

                return RedirectToAction("ProductIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product.");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return RedirectToAction("ProductIndex");
            }
        }

        private async Task<(List<ProductModel>, int)> GetSortedProducts(int pageNo, int pageSize, string sortField, string sortOrder)
        {
            int rowCount = await _appDbContext.Product.CountAsync();
            int pageCount = rowCount / pageSize;

            if (rowCount % pageSize > 0)
                pageCount++;

            bool ascending = sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

            IQueryable<ProductModel> query = _appDbContext.Product.AsQueryable();

            switch (sortField.ToLower())
            {
                case "productname":
                    query = ascending ? query.OrderBy(p => p.productName) : query.OrderByDescending(p => p.productName);
                    break;
                case "productprice":
                    query = ascending ? query.OrderBy(p => p.productPrice) : query.OrderByDescending(p => p.productPrice);
                    break;
                case "productquantity":
                    query = ascending ? query.OrderBy(p => p.productQuantity) : query.OrderByDescending(p => p.productQuantity);
                    break;
                default:
                    query = ascending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                    break;
            }

            List<ProductModel> list = await query
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (list, pageCount);
        }
    }
}
