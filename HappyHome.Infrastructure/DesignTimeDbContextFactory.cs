using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HappyHome.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HappyHomeDbContext>
{
    public HappyHomeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HappyHomeDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=HappyHome;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        return new HappyHomeDbContext(optionsBuilder.Options);
    }
}
