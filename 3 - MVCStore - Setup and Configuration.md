# CRUD ASP.NET MVC Core Application

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Creating the Projects](#CreatingtheProjects)
* 3. [Creating the Folder Structure](#CreatingtheFolderStructure)
* 4. [Configuring the Application](#ConfiguringtheApplication)
* 5. [Configuring the Razor View Engine](#ConfiguringtheRazorViewEngine)
* 6. [Add the Unit Test Project](#AddtheUnitTestProject)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- understand how to configure services;
- understand how to configure middleware components;
- importing types and the built-in tag helpers in the views;
- configuring the default layout page;

##  2. <a name='CreatingtheProjects'></a>Creating the Projects
1. To create the project, select `New > Project` from the Visual Studio `File` menu and choose the `ASP.NET Core Empty`. 
2. Run the project. Why do you think that we are seeing the "Hello World!" text?
3. Add a new unit testing project, called `MVC.Tests`, by choosing the `xUnit Test Project` template.

##  3. <a name='CreatingtheFolderStructure'></a>Creating the Folder Structure

5. Add the following folders

    |Name|Description |
    | ------------- |-------------|
    Models |This folder will contain the model classes.|
    Controllers | This folder will contain the controller classes.
    Views | This folder holds everything related to views, including individual Razor files, the view start file, and the view imports file.

##  4. <a name='ConfiguringtheApplication'></a>Configuring the Application

6. Open the `Startup` class.
   >The `ConfigureServices` method is used to set up objects, known as services, that can be used throughout the application and that are accessed through  dependency injection.
7. Modify the `ConfigureServices` method in the `Startup` class as follows in order to enable the MVC framework.

    ``` c#
    public void ConfigureServices(IServiceCollection services)
    {
        // !!!! add this line{
        services.AddControllersWithViews();
        // }!!!!
    }
    ```

    >`AddControllersWithViews()` adds the services for controllers to the specified `IServiceCollection`. It will not register services used for pages.

    >This method configures the MVC services for the commonly used features with controllers with views. It combines the effects of `AddMvcCore(IServiceCollection)`, `AddApiExplorer(IMvcCoreBuilder)`, `AddAuthorization(IMvcCoreBuilder)`, `AddCors(IMvcCoreBuilder)`, `AddDataAnnotations(IMvcCoreBuilder)`, `AddFormatterMappings(IMvcCoreBuilder)`, `AddCacheTagHelper(IMvcCoreBuilder)`, `AddViews(IMvcCoreBuilder)`, and `AddRazorViewEngine(IMvcCoreBuilder)`.

    >Documenattion: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.mvcservicecollectionextensions.addcontrollerswithviews

8. Modify the `Configure` method in the `Startup` class as follows in order to enable the MVC framework and some related features that are useful for development.

    > ASP.NET Core receives HTTP requests and passes them along a request pipeline, which is populated with middleware components registered in the `Configure` method. Each middleware component is able to inspect requests, modify them, generate a response, or modify the responses that other components have produced.

    ``` c#
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        // !!!! new/updated code {
        app.UseStatusCodePages();
        app.UseStaticFiles();
        ///}

        app.UseRouting();
        app.UseEndpoints(endpoints =>
            {
                // !!!! new/updated code {
                endpoints.MapDefaultControllerRoute();
                //}
            });
    }
    ```

    |Name|Description |
    | ------------- |-------------|
    `UseDeveloperExceptionPage()` | This extension method displays details of exceptions that occur in the application, which is useful during the development process. It should not be enabled in deployed applications.
    `UseStatusCodePages()` | This extension method adds a simple message to HTTP responses that would not otherwise have a body, such as 404 - Not Found responses.
    `UseStaticFiles()` | This extension method enables support for serving static content from the wwwroot folder.

    > One especially important middleware component provides the endpoint routing feature, which matches HTTP requests to the application features - known as endpoints - and is able to produce responses for them. The endpoint routing feature is added to the request pipeline with the `UseRouting` and `UseEndpoints` methods. To register the MVC Framework as a source of endpoints, the `MapDefaultControllerRoute` method can be called.

##  5. <a name='ConfiguringtheRazorViewEngine'></a>Configuring the Razor View Engine

> The Razor view engine is responsible for processing view files, which have the **.cshtml** extension, to generate HTML responses.

1.  Add the Razor View Imports. Right-click the Views folder, select Add > New Item from the pop-up menu, and select the "Razor View Imports" item from the ASP.NET Core > Web > ASP.NET category.
    
    ```c#
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    ```

    >The `@using` statement will allow us to use the types in the MVCStore.Models namespace in views without needing to refer to the namespace. The `@addTagHelper` statement enables the built-in tag helpers.

2.  Add the Razor View Start.

    ```C#
    @{
        Layout = "_Layout";
    }
    ```

    > The view start file tells Razor to use a layout file in the HTML that it generates, reducing the amount of duplication in views.
3.  Add the Razor Layout. Add a Razor layout file named _Layout.cshtml to the Views/Shared folder, with the content show below.

    ```CSHTML
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>@ViewBag.Title</title>
    </head>
    <body>
        <div>
        @RenderBody()
        </div>
    </body>
    </html>
    ```
    >This file defines a simple HTML document into which the contents of other views will be inserted by the @RenderBody expression.

## Creating the Controller and View
4. Add a class file named `HomeController.cs` in the `Controllers` folder and use it to define the class shown below. This is a minimal controller that contains just enough functionality to produce a response.

    ```C#
    public class HomeController: Controller {
        public IActionResult Index(){
            return View();
        }
    }
    ```
    >The `MapDefaultControllerRoute` method used in the `Startup` class tells ASP.NET Core how to match URLs to controller classes. The configuration applied by that method declares that the `Index` action method defined by the `Home` controller will be used to handle requests.

    >The `Index` action method doesn’t do anything useful yet and just returns the result of calling the `View` method, which is inherited from the `Controller` base class. This result tells ASP.NET Core to render the default view associated with the action method. 
    
5. Create the view, by adding a Razor View file named Index.cshtml to the Views/Home folder with the content shown below.

    ```CSHTML
    <h4>Welcome to MVCStore</h4>
    ```

6.  Run the application. An error message is shown because there are no controllers in the application to handle requests at the moment.

##  6. <a name='AddtheUnitTestProject'></a>Add the Unit Test Project
9. Right-click on the solution item in the Solution Explorer and select Add > New Project from the popup menu. Select xUnit Test Project (.NET Core) from the list of project templates and set the name of the project to MVCStore.Tests. Click OK to create the unit test project.

10. Add a reference towards the `MVCStore` project.

11. Install the Moq NuGet package.