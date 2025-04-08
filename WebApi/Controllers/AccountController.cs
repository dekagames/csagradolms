using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(UserManager<IdentityUser> userManager, ILogger<AccountController> logger)
    {
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Welcome()
    {
        if(User.Identity == null || !User.Identity.IsAuthenticated)
        {
            _logger.LogInformation("User is not authenticated");
            return Unauthorized("User is not authenticated");
        }
        _logger.LogInformation("Welcome to the Account API");
        return Ok("Welcome to the Account API");
    }
    
    [Authorize]
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return BadRequest();
        
        UserProfile userProfile = new()
        {
            Id = currentUser.Id,
            Name = currentUser.UserName!,
            Email = currentUser.Email!,
            PhoneNumber = currentUser.PhoneNumber!
        };
        return Ok(userProfile);
    }
}