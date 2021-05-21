# HelloWorld - Controllers, Actions, Views

<!-- vscode-markdown-toc -->
* 1. [Learning Objectives](#LearningObjectives)
* 2. [Creating the project](#Creatingtheproject)
* 3. [Controller](#Controller)
* 4. [Routes](#Routes)
* 5. [Views - Rendering Web Pages](#Views-RenderingWebPages)
* 6. [Views - Adding Dynamic Output](#Views-AddingDynamicOutput)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='LearningObjectives'></a>Learning Objectives
- creating ASP.NET Core MVC project
- adding controllers
- working with actions that return a string
- working with actions that return a View
- communicating between the Controller and the View using the ViewBag
- understanding routes

##  2. <a name='Creatingtheproject'></a>Creating the project
1. Create the ASP.NET Core Project.
    To create the project, select `New > Project` from the Visual Studio `File` menu and select the `Templates > Visual C# > .NET Core` section of the `New Project` dialog window. Select the `ASP.NET Core Web Application (Model-View-Controller)` item, and enter `FirstCoreApplication` into the Name field.
2. Run the project

##  3. <a name='Controller'></a>Controller

3. This project uses a single controller to select the views displayed to users and to receive form data. Edit the `HomeController.cs` file in the `Controllers` folder to replace the placeholder code provided by Visual Studio with the statements shown below.

    ``` c#
    public class HomeController : Controller
    {
        public string Index()
        {
            return "Hello World!";
        }
    }
     ```
4. Run the project and check the result.

##  4. <a name='Routes'></a>Routes

MVC applications use the ASP.NET routing system, which decides how URLs map to controllers and actions. A route is a rule that is used to decide how a request is handled. 

You can request any of the following URLs, and they will be directed to the Index action on the HomeController:
    
    * /
    * /Home
    * /Home/Index

> This is a good example of benefiting from following conventions implemented by ASP.NET Core MVC. In this case, the convention is that I will have a controller called HomeController and that it will be
the starting point for the MVC application. The default configuration that Visual Studio creates for a new project assumes that I will follow this convention. And since I did follow the convention, I automatically
got support for the URLs in the preceding list. If I had not followed the convention, I would need to modify the configuration to point to whatever controller I had created instead. For this simple example, the default
configuration is all I need.

##  5. <a name='Views-RenderingWebPages'></a>Views - Rendering Web Pages
 
The output from the current application is just the string Hello World. In order to return an HTML response to a browser request, we need a **View**.

5. Remove all the files in the `Views/Home` folder.

    > Views are stored in the `Views` folder, organized into subfolders. Views that are associated with the `Home` controller, for example, are stored in a folder called `Views/Home`. Views that are not specific to a single controller are stored in a folder called `Views/Shared`.

    > Visual Studio creates the Home and Shared folders automatically when the  Web Application template is used and puts in some placeholder views to get the project started.

6. Change the `HomeController` as follows:

    ```c#
	public class HomeController : Controller
	{
		public ViewResult Index()
		{
			return View("MyView");
		}
	}
    ```

7. Run the application and notice the error.

    > Views are stored in the `Views` folder, organized into subfolders. Views that are associated with the `Home` controller, for example, are stored in a folder called `Views/Home`. Views that are not specific to a single controller are stored in a folder called `Views/Shared`.

    > Visual Studio creates the Home and Shared folders automatically when the Web Application template is used and puts in some placeholder views to get the project started.

8. Change the `HomeController` as follows:

    ```c#
	public class HomeController : Controller
	{
		public ViewResult Index()
		{
			return View();
		}
	}
    ``` 
9. Add the `Index` view
    >Razor view files have the `.cshtml` file extension because they are a mix of `C#` code and `HTML` elements.

10. Modify the `Index` view as follows

    ```HTML
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Index</title>
    </head>
    <body>
        <div>
            Hello World (from the view)
        </div>
    </body>
    </html>
    ```
11. Run the application. The view should be displayed.

##  6. <a name='Views-AddingDynamicOutput'></a>Views - Adding Dynamic Output

> One way to pass data from the controller to the view is by using the ViewBag object, which is a member of the Controller base class. ViewBag is a dynamic object to which you can assign arbitrary properties, making those values available in whatever view is subsequently rendered. 

12. Modify the `HomeController` as follows.

    ```C#
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            int hour = DateTime.Now.Hour;
            ViewBag.Greeting = hour < 12 ? "Good Morning" : "Good Afternoon";
            return View();
        }
    }
    ```

13. Modify the `Index` view as follows.

    ```HTML
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Index</title>
    </head>
    <body>
        <div>
            @ViewBag.Greeting World (from the view)
        </div>
    </body>
    </html>
    ```
