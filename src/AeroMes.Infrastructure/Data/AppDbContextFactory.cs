using LiteBus.Events.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Moq;

namespace AeroMes.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
        // 1. Setup your development connection string
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AeroMesDb;Trusted_Connection=True;");

        // 2. Mock or pass a dummy version of your mediator since EF doesn't use it for migrations
        var dummyMediator = new Mock<IEventMediator>().Object; 

        return new AppDbContext(optionsBuilder.Options, dummyMediator);
    }
}