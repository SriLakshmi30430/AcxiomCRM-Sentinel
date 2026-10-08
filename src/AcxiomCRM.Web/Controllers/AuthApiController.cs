using AcxiomCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Web.Controllers;
[ApiController]
[Route("api/v1/auth")]
public class AuthApiController:ControllerBase
{
    readonly UserManager<ApplicationUser> um; readonly SignInManager<ApplicationUser> sm;
    public AuthApiController(UserManager<ApplicationUser> u,SignInManager<ApplicationUser> s){um=u;sm=s;}
    [AllowAnonymous,HttpPost("login")]
    public async Task<IActionResult> Login(LoginApiDto dto){if(!ModelState.IsValid)return ValidationProblem(ModelState);var u=await um.FindByEmailAsync(dto.Email);if(u==null||!u.IsActive)return Unauthorized(new{message="Invalid credentials."});var r=await sm.PasswordSignInAsync(u,dto.Password,false,true);if(!r.Succeeded)return Unauthorized(new{message=r.IsLockedOut?"Account temporarily locked.":"Invalid credentials."});return Ok(new{message="Authenticated",user=new{u.Id,u.Email,u.FullName}});}
    [Authorize,HttpPost("logout")] public async Task<IActionResult> Logout(){await sm.SignOutAsync();return Ok(new{message="Logged out"});}
}
