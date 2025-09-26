using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using TTH_Inventory_Mngt.WebApi.CommonServices;
using TTH_Inventory_Mngt.WebApi.DataAccess;

namespace TTH_Inventory_Mngt.WebApi.Public
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures dependency injection and application services.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });
            if (string.IsNullOrEmpty(Configuration["ConnectionStrings:DefaultConnection"]))
            {
                // DynamoDB setup
#if DEBUG
                services.AddSingleton<IAmazonDynamoDB>(sp =>
                {
                    var config = new AmazonDynamoDBConfig
                    {
                        ServiceURL = Configuration["AWS:ServiceURL"],
                        AuthenticationRegion = Configuration["AWS:Region"]
                    };

                    var credentials = new BasicAWSCredentials(
                        Configuration["AWS:AccessKey"],
                        Configuration["AWS:SecretKey"]
                    );

                    return new AmazonDynamoDBClient(credentials, config);
                });
#else
                services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
                services.AddAWSService<IAmazonDynamoDB>();
#endif

                // Use DynamoDB repository
                services.AddTransient<IProductsRepositoryBase, ProductsRepository>();
            }
            else
            {
                // SQL Server setup
                services.AddDbContext<InventoryDbContext>(
                    options => options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"])
                );

                // Use EF repository
                services.AddTransient<IProductsRepositoryBase, ProductsRepositoryEF>();
            }

            // Business services
            services.AddTransient<IProductsServices, ProductsServices>();


            // Add Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TTH Inventory Management API",
                    Version = "v1"
                });
            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(
                        [
                            "http://localhost:4200" // Angular FE
                        ])
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                );
            });
        }

        /// <summary>
        /// Configures middleware pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Enable Swagger in Development
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TTH Inventory Management API v1");
                    c.RoutePrefix = string.Empty; // Swagger at root URL
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            // Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; frame-ancestors 'self'");
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                await next();
            });

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}
