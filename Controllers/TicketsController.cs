using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using UserApp.Data;
using UserApp.Models;
using UserApp.Hubs; 

namespace UserApp.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IHubContext<TicketHub> _hubContext;

        public TicketsController(AppDbContext context, UserManager<Users> userManager, IHubContext<TicketHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // LISTE AVEC FILTRAGE PAR RÔLE
        public async Task<IActionResult> Index(int? categoryId)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.AssignedTo);

            // Logique de filtrage
            if (User.IsInRole("Admin"))
            {
                // Voit tout, rien à filtrer
            }
            else if (User.IsInRole("Agent"))
            {
                ticketsQuery = ticketsQuery.Where(t => t.AssignedToId == user.Id);
            }
            else
            {
                ticketsQuery = ticketsQuery.Where(t => t.UserId == user.Id);
            }

            if (categoryId.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.CategoryId == categoryId);
            }

            return View(await ticketsQuery.OrderByDescending(t => t.CreatedAt).ToListAsync());
        }

        //  CREATE GET
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Agents = await _userManager.GetUsersInRoleAsync("Agent");
            return View();
        }

        //  CREATE POST 
        [HttpPost]
        public async Task<IActionResult> Create(Ticket model, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Agents = await _userManager.GetUsersInRoleAsync("Agent");
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            model.UserId = user.Id;
            model.Status = "Open";
            model.CreatedAt = DateTime.Now;

            _context.Tickets.Add(model);
            await _context.SaveChangesAsync();

            if (files != null && files.Any())
            {
                await HandleFileUploads(model.Id, files);
            }

            TempData["Success"] = "Ticket created successfully!";
            return RedirectToAction("Index");
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var ticket = _context.Tickets
                .Include(t => t.User)
                .Include(t => t.AssignedTo)
                .Include(t => t.Attachments)
                .Include(t => t.Comments!).ThenInclude(c => c.User)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null) return NotFound();
            return View(ticket);
        }

        //  ADD COMMENT 
        [HttpPost]
        public async Task<IActionResult> AddComment(int ticketId, string content)
        {
            var user = await _userManager.GetUserAsync(User);

            var comment = new Comment
            {
                TicketId = ticketId,
                Content = content,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                user = user.FullName,
                content = content
            });
        }

        //  EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            ViewBag.Agents = await _userManager.GetUsersInRoleAsync("Agent");
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return PartialView("Edit", ticket);
        }

        //  EDIT POST
        [HttpPost]
        public async Task<IActionResult> Edit(Ticket ticket, List<IFormFile> files)
        {
            var existing = await _context.Tickets.FindAsync(ticket.Id);
            if (existing == null) return NotFound();

            existing.Title = ticket.Title;
            existing.Description = ticket.Description;
            existing.Status = ticket.Status;
            existing.AssignedToId = ticket.AssignedToId;
            existing.CategoryId = ticket.CategoryId;

            if (files != null && files.Any())
            {
                await HandleFileUploads(existing.Id, files);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //  DELETE ATTACHMENT
        [HttpPost]
        public IActionResult DeleteAttachment(int id)
        {
            var attachment = _context.Attachments.FirstOrDefault(x => x.Id == id);

            if (attachment == null)
                return Json(new { success = false });

            // supprimer fichier physique
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _context.Attachments.Remove(attachment);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket deleted successfully";

            return RedirectToAction("Index");
        }

        private async Task HandleFileUploads(int ticketId, List<IFormFile> files)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png" && ext != ".jpeg") continue;

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _context.Attachments.Add(new Attachment
                {
                    FileName = file.FileName,
                    FilePath = "/uploads/" + fileName,
                    TicketId = ticketId
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}