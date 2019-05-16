using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MVCStore.Model;

namespace MVCStore
{
    public class Startup
    {
		public void ConfigureServices(IServiceCollection services)
		{
			// !!!! add this line{ 
			services.AddTransient<IProductRepository, FakeProductRepository>();
			// }!!!!
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

			// !!!! add these lines{ 
			app.UseStatusCodePages();
			app.UseStaticFiles();
			app.UseMvc(routes => {
				routes.MapRoute(
				name: "default",
				template: "{controller=Product}/{action=List}/{id?}");
			});
			// }!!!!
        }
    }
}
