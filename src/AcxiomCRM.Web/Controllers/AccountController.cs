using AcxiomCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Web.Controllers;
public class AccountController:Controller
{
    private readonly UserManager<ApplicationUser> _um; private readonly SignInManager<ApplicationUser> _sm;
    public AccountController(UserManager<ApplicationUser> um,SignInManager<ApplicationUser> sm){_um=um;_sm=sm;}
    [AllowAnonymous] public IActionResult Login(string? returnUrl=null)=>View(new LoginVm("","",false));
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginVm vm,string? returnUrl=null){if(!ModelState.IsValid)return View(vm);var u=await _um.FindByEmailAsync(vm.Email);if(u is null||!u.IsActive){ModelState.AddModelError("","Invalid credentials or inactive account.");return View(vm);}var r=await _sm.PasswordSignInAsync(u,vm.Password,vm.RememberMe,lockoutOnFailure:true);if(r.Succeeded){await _um.ResetAccessFailedCountAsync(u);return LocalRedirect(returnUrl??"/Dashboard");}ModelState.AddModelError("",r.IsLockedOut?"Account locked temporarily. Try again later.":"Invalid email or password.");return View(vm);}
    [AllowAnonymous] public IActionResult Register()=>View(new RegisterVm("","","",""));
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterVm vm){if(!ModelState.IsValid)return View(vm);var u=new ApplicationUser{UserName=vm.Email,Email=vm.Email,FullName=vm.FullName,EmailConfirmed=true,IsActive=true};var r=await _um.CreateAsync(u,vm.Password);if(r.Succeeded){await _um.AddToRoleAsync(u,"SalesExecutive");await _sm.SignInAsync(u,false);return RedirectToAction("Index","Dashboard");}foreach(var e in r.Errors)ModelState.AddModelError("",e.Description);return View(vm);}
    [Authorize] public async Task<IActionResult> Logout(){await _sm.SignOutAsync();return RedirectToAction("Login");}
    public IActionResult AccessDenied()=>View();
}
