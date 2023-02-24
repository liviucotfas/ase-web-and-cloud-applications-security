# MVCStore - Setup and Configuration

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Creating the Projects](#CreatingtheProjects)
* 3. [Creating the Folder Structure](#CreatingtheFolderStructure)
* 4. [Configuring the Application](#ConfiguringtheApplication)
* 5. [Configuring the Razor View Engine](#ConfiguringtheRazorViewEngine)
* 6. [Creating the Controller and View](#CreatingtheControllerandView)
* 7. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- configuring the folder structure;
- configuring services;
- configuring middleware components;
- configuring the Razor View Engine (importing types, layout page);
- creating the defualt controller, action and view.

##  2. <a name='CreatingtheProjects'></a>Creating the Projects
1. To create the project, select `New > Project` from the Visual Studio `File` menu and choose the `ASP.NET Core Empty`. Name the project `MVCStore`. Check the "Do not use top-level statements" checkbox.
2. Run the project. Why do you think that we are seeing the "Hello World!" text?
3. Right-click on the solution item in the Solution Explorer and select **Add > New Project** from the popup menu. Select **xUnit Test Project** from the list of project templates and set the name of the project to MVCStore.Tests. Click OK to create the unit test project. 
4.  Add a reference towards the `MVCStore` project.
5.  Install the Moq NuGet package.

##  3. <a name='CreatingtheFolderStructure'></a>Creating the Folder Structure

5. Add the following folders

    |Name|Description |
    | ------------- |-------------|
    Models |This folder will contain the model classes.|
    Controllers | This folder will contain the controller classes.
    Views | This folder holds everything related to views, including individual Razor files, the view start file, and the view imports file.

##  4. <a name='ConfiguringtheApplication'></a>Configuring the Application

6. Open the `Program` class.
7. Modify the `main` method in the `Program` class as follows in order to enable the MVC framework.

    ``` c#
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllersWithViews();
    var app = builder.Build();
    //app.MapGet("/", () => "Hello World!");
    app.UseStaticFiles();
    app.MapDefaultControllerRoute();
    app.Run();
    ```

    > The `builder.Service` property is used to set up objects, known as services, that can be used throughout the application and that are accessed through dependency injection.

    >`AddControllersWithViews()` adds the services for controllers. This method configures the MVC services for the commonly used features with controllers with views. It combines the effects of `AddMvcCore(IServiceCollection)`, `AddApiExplorer(IMvcCoreBuilder)`, `AddAuthorization(IMvcCoreBuilder)`, `AddCors(IMvcCoreBuilder)`, `AddDataAnnotations(IMvcCoreBuilder)`, `AddFormatterMappings(IMvcCoreBuilder)`, `AddCacheTagHelper(IMvcCoreBuilder)`, `AddViews(IMvcCoreBuilder)`, and `AddRazorViewEngine(IMvcCoreBuilder)`.

    >Documenattion: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.mvcservicecollectionextensions.addcontrollerswithviews

    >The `UseStaticFiles` method enables support for serving static content from the `wwwroot` folder.

    ASP.NET Core receives HTTP requests and passes them along a request pipeline, which is populated with middleware components registered using the app property. Each middleware component is able to inspect requests, modify them, generate a response, or modify the responses that other components have produced. One especially important middleware component provides the endpoint routing feature, which matches HTTP requests to the application features—known as endpoints—able to produce responses for them. The endpoint routing feature is added to the request pipeline automatically, and the `MapDefaultControllerRoute` registers the MVC Framework as a source of endpoints using a default convention for mapping requests to classes and methods.

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
        @await RenderSectionAsync("Scripts", required: false)
    </body>
    </html>
    ```
    >This file defines a simple HTML document into which the contents of other views will be inserted by the @RenderBody expression.

##  6. <a name='CreatingtheControllerandView'></a>Creating the Controller and View
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

6.  Run the application.

##  7. <a name='Bibliography'></a>Bibliography