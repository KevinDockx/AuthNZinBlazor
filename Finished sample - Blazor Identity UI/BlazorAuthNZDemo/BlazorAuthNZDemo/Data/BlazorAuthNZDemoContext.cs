using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlazorAuthNZDemo.Data;

namespace BlazorAuthNZDemo.Data
{
    public class BlazorAuthNZDemoContext(DbContextOptions<BlazorAuthNZDemoContext> options) : IdentityDbContext<BlazorAuthNZDemoUser>(options)
    {
    }
}
