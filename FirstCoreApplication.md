# First ASP.NET Core Application

## Creating the project
1. Create the ASP.NET Core Project
    To create the project, select `New > Project` from the Visual Studio `File` menu and select the `Templates > Visual C# > .NET Core` section of the `New Project` dialog window. Select the ASP.NET Core Web Application (.NET Core) item, and enter `FirstCoreApplication` into the Name field.
2. Run the project

## Add a model

3. Right-click on the project item in the Solution Explorer window and select `Add >
New Folder` from the popup list and set the name of the folder to `Models`.
4. Right click on the `Models` folder, select `Add > Class` and create a new class file called `GuestResponse.cs`.

    ``` c#
    namespace FirstCoreApplication.Models {
        public class GuestResponse {
            [Required(ErrorMessage = "Please enter your name")]
            public string Name { get; set; }
            
            [Required(ErrorMessage = "Please enter your email address")]
            [RegularExpression(".+\\@.+\\..+",
            ErrorMessage = "Please enter a valid email address")]
            public string Email { get; set; }
            
            [Required(ErrorMessage = "Please enter your phone number")]
            public string Phone { get; set; }
            
            [Required(ErrorMessage = "Please specify whether you'll attend")]
            public bool? WillAttend { get; set; }
        }
    }
    ```

4. This project includes a simple in-memory repository to store the responses from users.
Add a new class file called `Repository.cs` in the `Models`.

    ``` c#
    public static class Repository
	{
		private static List<GuestResponse> responses = new List<GuestResponse>();
		public static IEnumerable<GuestResponse> Responses
		{
			get
			{
				return responses;
			}
		}
		public static void AddResponse(GuestResponse response)
		{
			responses.Add(response);
		}
	}
     ```

## Controllers

6. This project uses a single controller to select the views displayed to users and to receive form data. Edit the `HomeController.cs` file in the `Controllers` folder to replace the placeholder code provided by Visual Studio with the statements shown bellow.

    ``` c#
    public class HomeController : Controller
    {
        public string Index()
        {
            return "Hello World!";
        }
    }
     ```
7. Run the project and check the result.

## Routes

MVC applications use the ASP.NET routing system, which decides how URLs map to controllers and actions. A route is a rule that is used to decide how a request is handled. 

You can request any of the following URLs, and they will be directed to the Index action on the HomeController:
    
    * /
    * /Home
    * /Home/Index

> This is a good example of benefiting from following conventions implemented by ASP.NET Core MVC. In this case, the convention is that I will have a controller called HomeController and that it will be
the starting point for the MVC application. The default configuration that Visual Studio creates for a new project assumes that I will follow this convention. And since I did follow the convention, I automatically
got support for the URLs in the preceding list. If I had not followed the convention, I would need to modify the configuration to point to whatever controller I had created instead. For this simple example, the default
configuration is all I need.

## Rendering Web Pages

8. Remove the views in the `Views/Home` folder.

9. Change the `HomeController` as follows:

    ```c#
	public class HomeController : Controller
	{
		public ViewResult Index()
		{
			return View("MyView");
		}
	}
    ```

10. Run the application and notice the error.

> Views are stored in the `Views` folder, organized into subfolders. Views that are associated with the `Home` controller, for example, are stored in a folder called
`Views/Home`. Views that are not specific to a single controller are stored in a folder called `Views/Shared`.

> Visual Studio creates the Home and Shared folders automatically when the Web Application template is used
and puts in some placeholder views to get the project started.

11. Change the `HomeController` as follows:

    ```c#
	public class HomeController : Controller
	{
		public ViewResult Index()
		{
			return View();
		}
	}
    ``` 
12. Add the `Index` view
13. Modify the `Index` view as follows

    ```c#
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Index</title>
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
    </head>
    <body>
        <div class="text-center">
            <h3>We're going to have an exciting party!</h3>
            <h4>And you are invited</h4>
            <a class="btn btn-primary" asp-action="RsvpForm">RSVP Now</a>
        </div>
    </body>
    </html>
    ```