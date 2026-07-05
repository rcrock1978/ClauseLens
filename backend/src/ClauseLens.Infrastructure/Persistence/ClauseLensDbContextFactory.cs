using ClauseLens.Application.Abstractions;
using ClauseLens.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClauseLens.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations. Used by `dotnet ef`.
/// </summary>
public sealed class ClauseLensDbContextFactory : IDesignTimeDbContextFactory<ClauseLensDbContext>
{
    public ClauseLensDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClauseLensDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=ClauseLens;User Id=sa;Password=DevPassword!23;TrustServerCertificate=true")
            .Options;
        return new ClauseLensDbContext(options, new SystemCurrentUser());
    }
}
