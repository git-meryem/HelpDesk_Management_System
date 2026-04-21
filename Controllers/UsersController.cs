using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApp.Data;
using UserApp.Models;
using UserApp.Models.ViewModels;
using UserApp.ViewModels;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<Users> _userManager;
    private readonly AppDbContext _context;

    public UsersController(UserManager<Users> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

   
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync();

        var userList = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userList.Add(new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault()
            });
        }

        return View(userList);
    }

 
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Roles = new List<string> { "User", "Agent", "Admin" };
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        ViewBag.Roles = new List<string> { "User", "Agent", "Admin" };

        if (!ModelState.IsValid)
            return View(model);

        var user = new Users
        {
            FullName = model.FullName,
            Email = model.Email,
            UserName = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "User created successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Users model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
            return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;

        await _userManager.UpdateAsync(user);

        TempData["Success"] = "User updated successfully!";
        return RedirectToAction(nameof(Index));
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "You cannot delete an Admin.";
            return RedirectToAction(nameof(Index));
        }

        
        var comments = _context.Comments.Where(c => c.UserId == id);
        _context.Comments.RemoveRange(comments);

     
        var tickets = _context.Tickets.Where(t => t.UserId == id);
        _context.Tickets.RemoveRange(tickets);

        await _context.SaveChangesAsync();

   
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
            TempData["Success"] = "User deleted successfully!";
        else
            TempData["Error"] = "Error deleting user!";

        return RedirectToAction(nameof(Index));
    }



    [HttpPost]
    public async Task<IActionResult> ChangeRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);

        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = "Role updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ===================== RESET PASSWORD =====================
    //[HttpPost]
    //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return Json(new { success = false, message = "Invalid data" });
    //    }

    //    var user = await _userManager.FindByIdAsync(model.UserId);

    //    if (user == null)
    //    {
    //        return Json(new { success = false, message = "User not found" });
    //    }

    //    var checkOld = await _userManager.CheckPasswordAsync(user, model.OldPassword);

    //    if (!checkOld)
    //    {
    //        return Json(new { success = false, message = "Old password incorrect" });
    //    }

    //    var result = await _userManager.ChangePasswordAsync(
    //        user,
    //        model.OldPassword,
    //        model.NewPassword
    //    );

    //    if (result.Succeeded)
    //    {
    //        return Json(new { success = true, message = "Password updated successfully" });
    //    }

    //    return Json(new { success = false, message = "Error updating password" });
    //}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid data";
            return RedirectToAction(nameof(Index));
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
            TempData["Error"] = "Passwords do not match";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(model.UserId);

        if (user == null)
        {
            TempData["Error"] = "User not found";
            return RedirectToAction(nameof(Index));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (result.Succeeded)
        {
            TempData["Success"] = "Password reset successfully";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Error setting password";
        return RedirectToAction(nameof(Index));
    }
}