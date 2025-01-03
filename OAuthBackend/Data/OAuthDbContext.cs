using Microsoft.EntityFrameworkCore;
using OAuthBackend.Models;

namespace OAuthBackend.Data
{
    public class OAuthDbContext : DbContext
    {
        public DbSet<UserToken> UserTokens { get; set; }

        public OAuthDbContext(DbContextOptions<OAuthDbContext> options) : base(options) { }
    }
}
