using ChatApp.Endpoint.Helpers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Endpoint.Data
{
    public class ChatAppContext : IdentityDbContext<AppUser>
    {
        public ChatAppContext(DbContextOptions<ChatAppContext> options) : base(options)
        {
        }
    }
}
