# CRUD ASP.NET MVC Core Application

## Create the project
1. To create the project, select `New > Project` from the Visual Studio `File` menu and select the `Templates > Visual C# > .NET Core` section of the `New Project` dialog window. 
2. Select the ASP.NET Core Web Application (.NET Core) item, and enter `MVCStore` into the Name field.
3. Run the project

## Adding the NuGet Packages 

4. Add the following NuGet packages

    |Name|Description |
    | ------------- |-------------|
    Microsoft.AspNetCore.Mvc | This package contains ASP.NET Core MVC and provides access to essential features such as controllers and Razor views.|
    Microsoft.AspNetCore.StaticFiles | This package provides support for serving static files, such as images, JavaScript, and CSS, from the wwwroot folder. |
    Microsoft.VisualStudio.Web.BrowserLink| This package provides support for automatically reloading the browser when files in the project change, which can be a useful feature|

## Creating the Folder Structure

5. Add the following folders

    |Name|Description |
    | ------------- |-------------|
    Models |This folder will contain the model classes.|
    Controllers | This folder will contain the controller classes.
    Views | This folder holds everything related to views, including individual Razor files, the view start file, and the view imports file.

## Configuring the Application

6. Modify the `Startup` class as follows

    ``` c#
    public class Startup
    {
         public void ConfigureServices(IServiceCollection services)
        {
			// !!!! add this line{ 
			services.AddMvc();
			// }!!!!
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
			app.UseMvcWithDefaultRoute();
			// }!!!!

			app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
    ```

    |Name|Description |
    | ------------- |-------------|
    UseDeveloperExceptionPage() | This extension method displays details of exceptions that occur in the application, which is useful during the development process. It should not be enabled in deployed applications.
    UseStatusCodePages() |This extension method adds a simple message to HTTP responses that would not otherwise have a body, such as 404 - Not Found responses.
    UseStaticFiles() |This extension method enables support for serving static content from the wwwroot folder.
    UseMvcWithDefaultRoute() | This extension method enables ASP.NET Core MVC with a default configuration.

7. Add the MVC View Imports Page. 
    
    Right-click the Views folder, select Add > New Item from the pop-up menu, and select the MVC View Imports Page item from the ASP.NET Core > Web > ASP.NET category
    
    ``` c#
    @using MVCStore.Models
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    ```

## Add the Unit Test Project
8. Right-click on the solution item in the Solution Explorer and select Add > New Project from the popup menu. Select xUnit Test Project (.NET Core) from the list of project templates and set the name of the project to MVCStore.Tests. Click OK to create the unit test project.

9. Run the application