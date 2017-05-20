using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MVCStore.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(
			Configuration["Data:Database:ConnectionString"]));
			services.AddTransient<IProductRepository, EFProductRepository>();

			services.AddIdentity<IdentityUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
						
			app.UseStatusCodePages();
			app.UseStaticFiles();
			app.UseMvc(routes => {
				routes.MapRoute(
				name: "default",
				template: "{controller=Product}/{action=List}/{id?}");
			});
			
			// !!!! add these lines{ 
			SeedData.EnsurePopulated(app);
			// }!!!!
		}
	}
}
