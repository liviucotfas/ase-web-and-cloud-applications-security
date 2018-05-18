# First ASP.NET Core **MVC** Application

## Creating the project
1. Create the ASP.NET Core Project.
    To create the project, select `New > Project` from the Visual Studio `File` menu and select the `Templates > Visual C# > .NET Core` section of the `New Project` dialog window. Select the ASP.NET Core Web Application (.NET Core) item, and enter `FirstCoreApplication` into the Name field.
2. Run the project

## Controller

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
 
  > The output from the current application is just the string Hello World. In order to return an HTML response to a browser request, we need a **View**.

8. Remove all the files in the `Views/Home` folder.

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

    > Views are stored in the `Views` folder, organized into subfolders. Views that are associated with the `Home` controller, for example, are stored in a folder called `Views/Home`. Views that are not specific to a single controller are stored in a folder called `Views/Shared`.

    > Visual Studio creates the Home and Shared folders automatically when the Web Application template is used and puts in some placeholder views to get the project started.

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
    >Razor view files have the cshtml file extension because they are a mix of C# code and HTML elements.

13. Modify the `Index` view as follows

    > The **asp-action** attribute is an example of a tag helper attribute, which is an instruction for Razor that will be performed when the view is rendered. The asp-action attribute is an instruction to add a href attribute to the a element that contains a URL for an action method.

    ```
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
            <h3>We're going to have an ASP.NET Core 2 Course!</h3>
            <h4>And you are invited</h4>
            <a class="btn btn-primary" asp-action="RsvpForm">RSVP Now</a>
        </div>
    </body>
    </html>
    ```
    > You should use the features provided by MVC to generate URLs, rather than hard-code them into your views. When the tag helper created the href attribute for the a element, it inspected the configuration of the application to figure out what the URL should be. This allows the configuration of the application to be changed to support different URL formats without needing to update any views.

    > Bootstrap is already included in the project. If you are not familiar with it check [http://getbootstrap.com/](http://getbootstrap.com/)

14. Add the following action to the `HomeController`

    ```c#
    [HttpGet]
    public ViewResult RsvpForm() {
        return View();
    }
    ```
15. Add the corresponding view

    > The **@model** Razor expression is used to create
    a strongly typed view.

    > Notice the `asp-action` on the `form` element

    ```
    @model FirstCoreApplication.Models.GuestResponse
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>RsvpForm</title>
        <link rel="stylesheet" href="/css/styles.css" />
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
    </head>
    <body>
        <div class="panel panel-success">
            <div class="panel-heading text-center"><h4>RSVP</h4></div>
            <div class="panel-body">
                <form class="p-a-1" asp-action="RsvpForm" method="post">
                    <div asp-validation-summary="All"></div>
                    <div class="form-group">
                        <label asp-for="Name">Your name:</label>
                        <input class="form-control" asp-for="Name" />
                    </div>
                    <div class="form-group">
                        <label asp-for="Email">Your email:</label>
                        <input class="form-control" asp-for="Email" />
                    </div>
                    <div class="form-group">
                        <label asp-for="Phone">Your phone:</label>
                        <input class="form-control" asp-for="Phone" />
                    </div>
                    <div class="form-group">
                        <label>Will you attend?</label>
                        <select class="form-control" asp-for="WillAttend">
                            <option value="">Choose an option</option>
                            <option value="true">Yes, I'll be there</option>
                            <option value="false">No, I can't come</option>
                        </select>
                    </div>
                    <div class="text-center">
                        <button class="btn btn-primary" type="submit">
                            Submit RSVP
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </body>
    </html>
    ```

16. Run the project and check the generated `HTML` code

    > We have defined a `label` and `input` element for each property of the `GuestResponse` model class. The `asp-for` attribute on the `label` element sets the `value` of the for attribute. The `asp-for` attribute on the `input` element sets the `id` and `name` elements.


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

    > Views are stored in the `Views` folder, organized into subfolders. Views that are associated with the `Home` controller, for example, are stored in a folder called `Views/Home`. Views that are not specific to a single controller are stored in a folder called `Views/Shared`.

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
    >Razor view files have the cshtml file extension because they are a mix of C# code and HTML elements.

13. Modify the `Index` view as follows

    > The **asp-action** attribute is an example of a tag helper attribute, which is an instruction for Razor that will be performed when the view is rendered. The asp-action attribute is an instruction to add a href attribute to the a element that contains a URL for an action method.

    ```
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
            <h3>We're going to have an ASP.NET Core Course!</h3>
            <h4>And you are invited</h4>
            <a class="btn btn-primary" asp-action="RsvpForm">RSVP Now</a>
        </div>
    </body>
    </html>
    ```
    > You should use the features provided by MVC to generate URLs, rather than hard-code them into your views. When the tag helper created the href attribute for the a element, it inspected the configuration of the application to figure out what the URL should be. This allows the configuration of the application to be changed to support different URL formats without needing to update any views.

    > Bootstrap is already included in the project. If you are not familiar with it check [http://getbootstrap.com/](http://getbootstrap.com/)

14. Add the following action to the `HomeController`

    ```c#
    [HttpGet]
    public ViewResult RsvpForm() {
        return View();
    }
    ```
15. Add the corresponding view

    > The **@model** Razor expression is used to create
    a strongly typed view.

    > Notice the `asp-action` on the `form` element

    ```
    @model FirstCoreApplication.Models.GuestResponse
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>RsvpForm</title>
        <link rel="stylesheet" href="/css/styles.css" />
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
    </head>
    <body>
        <div class="panel panel-success">
            <div class="panel-heading text-center"><h4>RSVP</h4></div>
            <div class="panel-body">
                <form class="p-a-1" asp-action="RsvpForm" method="post">
                    <div asp-validation-summary="All"></div>
                    <div class="form-group">
                        <label asp-for="Name">Your name:</label>
                        <input class="form-control" asp-for="Name" />
                    </div>
                    <div class="form-group">
                        <label asp-for="Email">Your email:</label>
                        <input class="form-control" asp-for="Email" />
                    </div>
                    <div class="form-group">
                        <label asp-for="Phone">Your phone:</label>
                        <input class="form-control" asp-for="Phone" />
                    </div>
                    <div class="form-group">
                        <label>Will you attend?</label>
                        <select class="form-control" asp-for="WillAttend">
                            <option value="">Choose an option</option>
                            <option value="true">Yes, I'll be there</option>
                            <option value="false">No, I can't come</option>
                        </select>
                    </div>
                    <div class="text-center">
                        <button class="btn btn-primary" type="submit">
                            Submit RSVP
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </body>
    </html>
    ```

16. Run the project and check the generated `HTML` code

    > We have defined a `label` and `input` element for each property of the `GuestResponse` model class. The `asp-for` attribute on the `label` element sets the `value` of the for attribute. The `asp-for` attribute on the `input` element sets the `id` and `name` elements.


## Adding the **Model**

3. Right-click on the project item in the Solution Explorer window and select `Add > New Folder` from the popup list and set the name of the folder to `Models`.

    >The **model** is the representation of the real-world objects, processes, and rules that define the subject, known as the domain , of the application. The model, often referred to as a domain model , contains the C# objects (known as domain objects ) that make up the universe of the application and the methods that manipulate them. The views and controllers expose the domain to the clients in a consistent manner, and a well-designed MVC application starts with a well-designed model, which is then the focal point as controllers and views are added.


4. Right click on the `Models` folder, select `Add > Class` and create a new class file called `GuestResponse.cs`.

    >MVC supports declarative validation rules defined with attributes from the System.ComponentModel.DataAnnotations namespace, meaning that validation constraints are expressed using the standard C# attribute features.

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





## Receiving Form Data

16. Add the following action to the `HomeController`

    > Handing GET and POST requests in separate C# methods helps to keep the controller code tidy, since the two methods have different responsibilities. Both action methods are invoked by the same URL, but MVC makes sure that the appropriate method is called, based on whether we are dealing with a GET or POST request.

    We check to see whether there has been a validation problem using the `ModelState.IsValid` property in the controller class.

    ```c#
    [HttpPost]
    public ViewResult RsvpForm(GuestResponse guestResponse)
    {
        if (ModelState.IsValid)
        {
            Repository.AddResponse(guestResponse);
            return View("Thanks", guestResponse);
        }
        else
        {
            // there is a validation error
            return View();
        }
    }
    ```

    > **model binding** is a useful MVC feature whereby incoming data is parsed and the key/value pairs in the HTTP request are used to populate properties of domain model types. Model binding is a powerful and customizable feature that eliminates the grind and toil of dealing with HTTP requests directly and lets you work with C# objects rather than dealing with individual data values sent by the browser. The GuestResponse object that is passed as the parameter to the action method is automatically populated with the data from the form fields.

17. Add the `Thanks.cshtml` file in the `Views/Home` folder

    ```
    @model FirstCoreApplication.Models.GuestResponse
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Thanks</title>
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
    </head>
    <body class="text-center">
        <p>
            <h1>Thank you, @Model.Name!</h1>
            @if (Model.WillAttend == true)
            {
                @:It's great that you're coming.
            }
            else
            {
                @:Sorry to hear that you can't make it, but thanks for letting us know.
            }
        </p>
        Click <a class="nav-link" asp-action="ListResponses">here</a>
        to see who is coming.
    </body>
    </html>
    ```

18. Add the following action to the `HomeController`

    ```C#
    public ViewResult ListResponses() {
        return View(Repository.Responses.Where(r => r.WillAttend == true));
    }
    ```

19. Add the `ListResponses.cshtml` file in the `Views/Home` folder

    ```C#
    @model System.Collections.Generic.IEnumerable<FirstCoreApplication.Models.GuestResponse>
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
        <title>Responses</title>
    </head>
    <body>
        <div class="panel-body">
            <h2>Here is the list of people attending the ASP.NET Core course</h2>
            <table class="table table-sm table-striped table-bordered">
                <thead>
                    <tr><th>Name</th><th>Email</th><th>Phone</th></tr>
                </thead>
                <tbody>
                    @foreach (FirstCoreApplication.Models.GuestResponse r in Model)
                    {
                        <tr><td>@r.Name</td><td>@r.Email</td><td>@r.Phone</td></tr>
                    }
                </tbody>
            </table>
        </div>
    </body>
    </html>
    ```