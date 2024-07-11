using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vendingMachine.Data;
using vendingMachine.Models;

namespace vendingMachine.Controllers
{
    public class TransactionController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public TransactionController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "Admin")]
        [ActionName("Index")]
        public async Task<IActionResult> TransactionIndex(int pageNo = 1, int pageSize = 10, string sortField = "Id", string sortOrder = "asc")
        {
            try
            {
                int rowCount = await _appDbContext.Transaction.CountAsync();
                int pageCount = rowCount / pageSize;

                if (rowCount % pageSize > 0)
                    pageCount++;

                bool ascending = sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

                IQueryable<TransactionModel> query = _appDbContext.Transaction.AsQueryable();

                if (sortField.Equals("TransactionDate", StringComparison.OrdinalIgnoreCase))
                {
                    query = ascending ? query.OrderBy(p => p.TransactionDate) : query.OrderByDescending(p => p.TransactionDate);
                }
                else
                {
                    query = ascending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                }

                List<TransactionModel> list = await query
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                TransactionResponseModel response = new()
                {
                    transactionData = list,
                    pageSize = pageSize,
                    pageCount = pageCount,
                    pageNo = pageNo,
                    sortField = sortField,
                    sortOrder = sortOrder
                };

                return View("TransactionIndex", response);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("ProductIndex");
            }
        }
    }
}
