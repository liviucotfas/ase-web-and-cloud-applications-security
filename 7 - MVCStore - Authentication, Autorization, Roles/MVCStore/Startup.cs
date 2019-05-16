using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore
{
    public class Startup
    {
        IConfigurationRoot Configuration;
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json").Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            /*services.AddTransient
                <IProductRepository, 
                FakeProductRepository>();*/

            services.AddDbContext<ApplicationDbContext>(
                options =>options.UseSqlServer(
            Configuration["ConnectionStrings:DefaultConnection"]));

			services.AddIdentity<IdentityUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders(); 

			services.AddTransient
                <IProductRepository, 
                EFProductRepository>();
            
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();
            app.UseStaticFiles();
			app.UseAuthentication();
			app.UseMvc(routes => {
                routes.MapRoute(
                name: "default",
                template: "{controller=Product}/{action=List}/{id?}");
            });

			SeedData.EnsurePopulated(app);
			IdentitySeedData.EnsurePopulated(app);
		}
	}
}
