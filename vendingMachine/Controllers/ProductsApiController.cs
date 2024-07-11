using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vendingMachine.Data;
using vendingMachine.Models;

namespace vendingMachine.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public ProductsApiController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetProducts()
        {
            try
            {
                var products = await _appDbContext.Product.ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductModel>> GetProduct(int id)
        {
            try
            {
                var product = await _appDbContext.Product.FindAsync(id);

                if (product == null)
                    return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("purchase/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> PurchaseProduct(int id)
        {
            try
            {
                var product = await _appDbContext.Product.FindAsync(id);

                if (product == null)
                    return NotFound("Product not found.");

                if (product.productQuantity > 0)
                {
                    TransactionModel transaction = new TransactionModel
                    {
                        productId = id,
                        Quantity = 1,
                        TransactionDate = DateTime.UtcNow
                    };

                    _appDbContext.Transaction.Add(transaction);

                    product.productQuantity--;

                    await _appDbContext.SaveChangesAsync();

                    return Ok("Purchase successful.");
                }
                else
                {
                    return BadRequest("Product is out of stock.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool ProductExists(int id)
        {
            return _appDbContext.Product.Any(e => e.Id == id);
        }
    }
}
