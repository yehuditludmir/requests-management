using RequestsManagement.DAL.Data;
using RequestsManagement.DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RequestsManagement.Tests.Integration
{
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=RequestsDb_Tests;Trusted_Connection=True;TrustServerCertificate=True";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        }

        // יוצר מסד בדיקות ריק ועדכני, פעם אחת לפני כל הבדיקות
        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        public new Task DisposeAsync() => Task.CompletedTask;

        // שומר פניות במסד הבדיקות, כדי שיקבלו Id ו-RowVersion אמיתיים
        public async Task AddRequestsAsync(params Request[] requests)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Requests.AddRange(requests);
            await db.SaveChangesAsync();
        }
    }

    [CollectionDefinition(nameof(ApiCollection))]
    public class ApiCollection : ICollectionFixture<ApiFactory>
    {
    }
}
