using AcxiomCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Web.Controllers;

[Authorize(Roles="Admin")]
public class AdminUsersController:Controller
{
    readonly UserManager<ApplicationUser> um;
    readonly RoleManager<IdentityRole> rm;
    public AdminUsersController(UserManager<ApplicationUser> u,RoleManager<IdentityRole> r){um=u;rm=r;}
    public async Task<IActionResult> Index(){var users=await um.Users.OrderBy(u=>u.Email).ToListAsync();var rows=new List<(ApplicationUser u,string role)>();foreach(var u in users)rows.Add((u,(await um.GetRolesAsync(u)).FirstOrDefault()??"-"));return View(rows);}
    public IActionResult Create()=>View(new AdminUserVm());
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserVm vm){
        if(!ModelState.IsValid)return View(vm);
        if(!new[]{"Admin","Manager","SalesExecutive"}.Contains(vm.Role)){ModelState.AddModelError("Role","Invalid role.");return View(vm);}
        var u=new ApplicationUser{UserName=vm.Email,Email=vm.Email,EmailConfirmed=true,FullName=vm.FullName,IsActive=true};
        var r=await um.CreateAsync(u,vm.Password);
        if(!r.Succeeded){foreach(var e in r.Errors)ModelState.AddModelError("",e.Description);return View(vm);}
        await um.AddToRoleAsync(u,vm.Role);
        return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string id){var u=await um.FindByIdAsync(id);if(u==null)return NotFound();u.IsActive=!u.IsActive;await um.UpdateAsync(u);return RedirectToAction(nameof(Index));}
}
