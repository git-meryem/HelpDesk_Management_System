using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApp.Data;
using UserApp.Models;

namespace UserApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "Name is required";
                return RedirectToAction("Index");
            }

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            var category = await _context.Categories.FindAsync(model.Id);

            if (category == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "Name cannot be empty";
                return RedirectToAction("Index");
            }

            category.Name = model.Name;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            var hasTickets = await _context.Tickets.AnyAsync(t => t.CategoryId == id);

            if (hasTickets)
            {
                TempData["Error"] = "Cannot delete category (used by tickets)";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}