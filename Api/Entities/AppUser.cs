namespace Api.Entities;

public class AppUser : IdentityUser
{
    public string? Name { get; set; }
}