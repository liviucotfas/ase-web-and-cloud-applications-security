# Simple Data-Entry Application

<!-- vscode-markdown-toc -->
* 1. [Adding the **Model**](#AddingtheModel)
* 2. [Receiving Form Data](#ReceivingFormData)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

Imagine that we want to implement an application that allows people to electronically RSVP. We would like to have the following features: 
 - a home page that shows information about the event;
 - a form that can be used to RSVP;
 - validation for the RSVP form, which will display a thank-you page;
 - a summary page that shows who is coming to the event.

##  1. <a name='AddingtheModel'></a>Adding a **Model**

1. Right-click on the project item in the Solution Explorer window and select `Add > New Folder` from the popup list and set the name of the folder to `Models`.

    >The **model** is the representation of the real-world objects, processes, and rules that define the subject, known as the domain , of the application. The model, often referred to as a domain model , contains the C# objects (known as domain objects ) that make up the universe of the application and the methods that manipulate them. The views and controllers expose the domain to the clients in a consistent manner, and a well-designed MVC application starts with a well-designed model, which is then the focal point as controllers and views are added.


2. Right click on the `Models` folder, select `Add > Class` and create a new class file called `GuestResponse.cs`.

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
## Creating a Second Action and a Strongly Typed View

>A single controller class can define multiple action methods, and the convention is to group related actions together in the same controller.

1. One of the goals of our app is to include an RSVP form. Let's define an action method that can receive requests for that form. Add the `RsvpForm` action listed bellow to the `HomeController`.

    ```C#
    public ViewResult RsvpForm() { 
        return View(); 
    }
    ```

2. Let's add the corresponding view.

    ```HTML
    @model FirstCoreApplication.Models.GuestResponse
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>RsvpForm</title>
    </head>
    <body>
        <div>
            This is the RsvpForm.cshtml View
        </div>
    </body>
    </html>
    ```

## Linking Action Methods

1. We want to be able to create a link from the `Index` view so that guests can see the RsvpForm view without having to know the URL that targets a specific action method. Let's update de `Index` view as follows.

    > The **asp-action** attribute is an example of a tag helper attribute, which is an instruction for Razor that will be performed when the view is rendered. The asp-action attribute is an instruction to add a href attribute to the a element that contains a URL for an action method.

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
        <p>We're going to have an ASP.NET Core 2 Course!<br />
        </p>
            <a asp-action="RsvpForm">RSVP Now</a> 
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





##  2. <a name='ReceivingFormData'></a>Receiving Form Data

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