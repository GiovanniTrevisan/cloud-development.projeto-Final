using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.Data;
using Repository.Interfaces;

[assembly: FunctionsStartup(typeof(SendEmail.Startup))]
namespace SendEmail
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            services.AddDbContext<Context>(options => options.UseSqlServer(builder.GetContext().Configuration.GetConnectionString("MagicDB")));
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<ISalesRecordRepository, SalesRecordRepository>();
            services.AddScoped<SeedingService>();
        }
    }
}
