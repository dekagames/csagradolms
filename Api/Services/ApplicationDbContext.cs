using Api.Entities;

namespace Api.Services;

public class ApplicationDbContext: IdentityDbContext<AppUser>
{
     public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
     {
          
     }
}