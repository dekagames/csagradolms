using Api.Entities;
using Api.Models;

namespace Api.Controllers;
using Swashbuckle.AspNetCore.Annotations;


[ApiController]
[Route("api/[controller]")]
public class AccountController(UserManager<AppUser> userManager, ILogger<AccountController> logger)
    : ControllerBase
{
    [HttpGet]
    public IActionResult Welcome()
    {
        if(User.Identity == null || !User.Identity.IsAuthenticated)
        {
            logger.LogInformation("User is not authenticated");
            return Unauthorized("User is not authenticated");
        }
        logger.LogInformation("Welcome to the Account API");
        return Ok("Welcome to the Account API");
    }
    
    [SwaggerOperation(Summary = "Registra um novo usuário", Description = "Cria um novo usuário no sistema.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync(RegisterModel model)
    {
        var user = new AppUser
        {
            UserName = model.Email,
            Name = model.Name,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            PasswordHash = model.Password
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if(result.Succeeded)
            return Created("Registration made successfully", user.Id);
            
        return BadRequest("Registration failed");
    }
    
    [Authorize]
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) return BadRequest();
        
        UserProfile userProfile = new()
        {
            Id = currentUser.Id,
            Name = currentUser.Name!,
            Email = currentUser.Email!,
            PhoneNumber = currentUser.PhoneNumber!
        };
        return Ok(userProfile);
    }
}