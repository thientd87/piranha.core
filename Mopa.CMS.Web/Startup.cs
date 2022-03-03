using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piranha;
using Piranha.AttributeBuilder;
using Piranha.Manager.Editor;
using Piranha.Data.EF.SQLServer;
using Piranha.AspNetCore.Identity.SQLServer;

namespace Mopa.CMS.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="configuration">The current configuration</param>
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPiranha(options =>
            {
                options.AddRazorRuntimeCompilation = true;
                options.UseCms(o =>
                {
                   
                });
                options.UseManager();
                options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
                options.UseImageSharp();
                options.UseMemoryCache();
                options.UseTinyMCE();
                var connectionString = _config.GetConnectionString("piranha");
                options.UseEF<SQLServerDb>(db => db.UseSqlServer(connectionString));
                options.UseIdentityWithSeed<IdentitySQLServerDb>(db => db.UseSqlServer(connectionString)
                     );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApi api)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Initialize Piranha
            App.Init(api);

            // Build content types
            new ContentTypeBuilder(api)
                .AddAssembly(typeof(Startup).Assembly)
                .Build()
                .DeleteOrphans();

            // Configure Tiny MCE
            EditorConfig.FromFile("editorconfig.json");

            // Middleware setup
            app.UsePiranha(options => {
                options.UseManager();
                options.UseTinyMCE();
                options.UseIdentity();
            });
        }
    }
}
