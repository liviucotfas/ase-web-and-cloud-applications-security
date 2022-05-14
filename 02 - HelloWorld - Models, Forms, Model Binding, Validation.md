# HelloWorld - Models, Forms, Model Binding, Validation
<!-- vscode-markdown-toc -->
* 1. [Learning Objectives](#LearningObjectives)
* 2. [RSVP Application](#RSVPApplication)
* 3. [Adding a **Model**](#AddingaModel)
* 4. [Adding additional actions to the **Controller**](#AddingadditionalactionstotheController)
* 5. [Linking Action Methods](#LinkingActionMethods)
* 6. [Building the Form](#BuildingtheForm)
* 7. [Receiving Form Data](#ReceivingFormData)
* 8. [Using Model Binding](#UsingModelBinding)
* 9. [Storing Responses](#StoringResponses)
* 10. [Displaying the Responses](#DisplayingtheResponses)
* 11. [Adding Validation](#AddingValidation)
* 12. [Styling the Content](#StylingtheContent)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='LearningObjectives'></a>Learning Objectives
- adding a **Model**
- having multiple **Actions** defined in a **Controller**
- communicating between the **Controller** and the **View** using a **Model**
- generating a link towards an **Action** inside a **View**
- configure `<form>` to submit data to controller actions.
- receiving data from forms using **model binding**
- validating form content

##  2. <a name='RSVPApplication'></a>RSVP Application
Imagine that we want to implement an application that allows people to electronically RSVP (acronym for "Répondez s'il vous plaît"). We would like to have the following features: 
 - a home page that shows information about the event;
 - a form that can be used to RSVP;
 - validation for the RSVP form, which will display a thank-you page;
 - a summary page that shows who is coming to the event.

 > Note: the application will not persisit the answers to any storage.

1. Create a new project named `CourseInvites` using the "ASP.NET Core Web App (Model-View-Controller)" template. Check the "Do not use top-level statements" checkbox.
2. Let's start with a little cleanup. Remove from the `Views` folder the `Home` and `Shared` folders. Remove from the `Models` folder the `ErrorViewModel` class.
3. Modify the `HomeController` as follows.

	```C#
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
		    _logger = logger;
		}

		public IActionResult Index()
		{
		    return View();
		}
	}
	```
4. Add a view for the `Index` action with the following content.

    > Razor view files have the **.cshtml** file extension because they are a mix of C# code and HTML elements.

    ```C#
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>ASP.NET lecture!</title>
    </head>
    <body>
        <div>
            <div>
                We're going to have a lecture on ASP.NET.<br />
            </div>
        </div>
    </body>
    </html>
    ```

##  3. <a name='AddingaModel'></a>Adding a **Model**

4. Right-click on the project item in the Solution Explorer window and select `Add > New Folder` from the popup list and set the name of the folder to `Models`.

    >The **model** is the representation of the real-world objects, processes, and rules that define the subject, known as the domain , of the application. The model, often referred to as a **domain model**, contains the C# objects (known as domain objects ) that make up the universe of the application and the methods that manipulate them. The views and controllers expose the domain to the clients in a consistent manner, and a well-designed MVC application starts with a well-designed model, which is then the focal point as controllers and views are added.


2. Right click on the `Models` folder, select `Add > Class` and create a new class file called `GuestResponse.cs`.

    ``` c#
    public class GuestResponse {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool? WillAttend { get; set;}
    }
    ```
##  4. <a name='AddingadditionalactionstotheController'></a>Adding additional actions to the **Controller**

>A single controller class can define multiple action methods, and the convention is to group related actions together in the same controller.

1. One of the goals of our app is to include an RSVP form. Let's define an action method that can receive requests for that form. Add the `RsvpForm` action listed below to the `HomeController`.

    ```C#
    public IActionResult RsvpForm()
    {
        return View();
    }
    ```

2. Let's add the corresponding view.

    > The **@model** Razor expression is used to create
    a strongly typed view.

    ```HTML
    @model CourseInvites.Models.GuestResponse
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

##  5. <a name='LinkingActionMethods'></a>Linking Action Methods

1. We want to be able to create a link from the `Index` view so that guests can see the RsvpForm view without having to know the URL that targets a specific action method. Let's update de `Index` view as follows.

    > The **asp-action** attribute is an example of a **tag helper attribute**, which is an instruction for Razor that will be performed when the view is rendered. The asp-action attribute is an instruction to add a href attribute to the a element that contains a URL for an action method.

    ```HTML
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>ASP.NET lecture!</title>
    </head>
    <body>
        <div>
            <div>
                We're going to have a lecture on ASP.NET.<br />
            </div>
            <a asp-action="RsvpForm">RSVP Now</a>
        </div>
    </body>
    </html>
    ```
    > You should use the features provided by MVC to generate URLs, rather than hard-code them into your views. When the tag helper created the href attribute for the a element, it inspected the configuration of the application to figure out what the URL should be. This allows the configuration of the application to be changed to support different URL formats without needing to update any views.

2. Run the project and check the `HTML` code that has been generated instead of `asp-action="RsvpForm"`.

##  6. <a name='BuildingtheForm'></a>Building the Form

1. Change the contents of the `RsvpForm.cshtml` file as follows

    > Notice the following tag helper attributes:
    > - `asp-action` on the `form` element;
    > - `asp-for` on the form inputs.

    ```HTML
    @model CourseInvites.Models.GuestResponse
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
        <form asp-action="RsvpForm" method="post">
            <p>
                <label asp-for="Name">Your name:</label>
                <input asp-for="Name" />
            </p>
            <p>
                <label asp-for="Email">Your email:</label>
                <input asp-for="Email" />
            </p>
            <p>
                <label asp-for="Phone">Your phone:</label>
                <input asp-for="Phone" />
            </p>
            <p>
                <label>Will you attend?</label>
                <select asp-for="WillAttend">
                    <option value="">Choose an option</option>
                    <option value="true">Yes, I'll be there</option>
                    <option value="false">No, I can't come</option>
                </select>
            </p>
            <button type="submit">Submit RSVP</button>
        </form>
    </body>
    </html> 
    ```
    > The `@model` expression specifies that the view expects to receive a `GuestResponse` object as its view model.

2. Run the project and check the generated `HTML` code.

    > We have defined a `label` and `input` element for each property of the `GuestResponse` model class. The `asp-for` attribute on the `label` element sets the `value` of the for attribute. The `asp-for` attribute on the `input` element sets the `id` and `name` elements.

    > The `asp-action` tag helper attribute applied to the form element, uses the application’s URL routing configuration to set the `action` attribute to a URL that will target a specific action method.

##  7. <a name='ReceivingFormData'></a>Receiving Form Data

1. Update the HomeController as follows.

    > Handing `GET` and `POST` requests in separate C# methods helps to keep the controller code tidy, since the two methods have different responsibilities. Both action methods are invoked by the same URL, but MVC makes sure that the appropriate method is called, based on whether we are dealing with a GET or POST request.

    ```C#
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet] 
        public IActionResult RsvpForm() {
            return View();
        }

        [HttpPost] 
        public IActionResult RsvpForm(GuestResponse guestResponse) { 
            // TODO: store response from guest 
            return View(); 
        } 
    }
    ```

##  8. <a name='UsingModelBinding'></a>Using Model Binding

**Model binding** is a useful MVC feature whereby incoming data is parsed and the key/value pairs in the HTTP request are used to populate properties of domain model types. Model binding is a powerful and customizable feature that eliminates the grind and toil of dealing with HTTP requests directly and lets you work with C# objects rather than dealing with individual data values sent by the browser. The `GuestResponse` object that is passed as the parameter to the action method is automatically populated with the data from the form fields.

##  9. <a name='StoringResponses'></a>Storing Responses
1. The project will include a simple in-memory repository to store the responses from users. Add a new class file called `Repository.cs` in the `Models`.

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

2. Let's update the `RsvpForm` method on the `HomeController` class in order to store the response.

    ```C#
    [HttpPost]
    public IActionResult RsvpForm(GuestResponse guestResponse) {
        Repository.AddResponse(guestResponse); 
        return View("Thanks", guestResponse); 
    }
    ```

3. Let's add the `Thanks.cshtml` view

    ```HTML
    @model CourseInvites.Models.GuestResponse
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Thanks</title>
    </head>
    <body>
    <p>
            <h1>Thank you, @Model.Name!</h1>
            @if (Model.WillAttend == true) {
                @:It's great that you're coming!
            } else {
                @:Sorry to hear that you can't make it, but thanks for letting us know.
            }
        </p>
        <p>Click <a asp-action="ListResponses">here</a> to see who is coming.</p>
    </body>
    </html>
    ```

##  10. <a name='DisplayingtheResponses'></a>Displaying the Responses

1. Let's add the `ListResponses` action on the `HomeController`

    ```C#
    public IActionResult ListResponses() { 
        return View(Repository.Responses.Where(r => r.WillAttend == true)); 
    } 
    ```
2. Add the corresponding view.

    ```C#
    @model IEnumerable<CourseInvites.Models.GuestResponse>
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Responses</title>
    </head>
    <body>
        <h2>Here is the list of people attending the course</h2>
        <table>
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Phone</th>
                </tr>
            </thead>
            <tbody>
                @foreach (CourseInvites.Models.GuestResponse r in Model) {
                    <tr>
                        <td>@r.Name</td>
                        <td>@r.Email</td>
                        <td>@r.Phone</td>
                    </tr>
                }
            </tbody>
        </table>
    </body>
    </html>
    ```

##  11. <a name='AddingValidation'></a>Adding Validation

1. Update the GuestResponse model class as follows.

    >MVC supports declarative validation rules defined with attributes from the System.ComponentModel.DataAnnotations namespace, meaning that validation constraints are expressed using the standard C# attribute features.

    > Without validation, users could enter nonsense data or even submit an empty form. In an MVC application, you typically apply validation to the domain model rather than in the user interface. This means that you define validation in one place, but it takes effect anywhere in the application that the model class is used. 

    ```C#
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
    ```
    >Notice that a nullable bool was used for the WillAttend property. We did this so that we could apply the Required validation attribute. If we had used a regular bool, the value we received through model binding could be only true or false, and we would not be able to tell whether the user had selected a value. A nullable bool has three possible values: true, false, and null. The browser sends a null value if the user has not selected a value, and this causes the Required attribute to report a validation error. This is a nice example of how MVC elegantly blends C# features with HTML and HTTP. 

2. Update the `RsvpForm` action on the `HomeController` as follows. We check to see whether there has been a validation problem using the `ModelState.IsValid` property in the controller class.

    > `ModelState` is a property of the `Controller` class that provides details of the outcome of the model binding process. If the `ModelState.IsValid` property returns true, then the model binder has not been able to satisfy the validation constraints specified through the attributes on the `GuestResponse` class.

    ```C#
    [HttpPost]
    public IActionResult RsvpForm(GuestResponse guestResponse) {
        if (ModelState.IsValid) { 
            Repository.AddResponse(guestResponse);
             return View("Thanks", guestResponse);
        } else { 
            // there is a validation error 
            return View(); 
        } 
    }
    ```

3. Add a validation summary to the `RsvpForm` view by including the following line.

    > When it renders a view, Razor has access to the details of any validation errors associated with the request, and tag helpers can access the details to display validation errors to the user.

    ```HTML
    <form asp-action="RsvpForm" method="post">
        <div asp-validation-summary="All"></div> 
        ....
    ```

4. Check how the HTML code for the `<input>` fields changes when the fields contain errors.

    > The CSS class `.input-validation-error` is added.

5. Change the formatting of the fields that contain errors in the `/css/styles.css` file.

    > Static content delivered to clients is placed into the `wwwroot` folder and organized by content type so that CSS stylesheets go into the wwwroot/css folder, JavaScript files go into the wwwroot/js folder.

    ```CSS
    .field-validation-error    {color: #f00;}
    .field-validation-valid    { display: none;}
    .input-validation-error    { border: 1px solid #f00; background-color: #fee; }
    .validation-summary-errors { font-weight: bold; color: #f00;}
    .validation-summary-valid  { display: none;}
    ```
6. Reference the stylesheet

    > Notice that the `wwwroot` folder is omitted from the URL. The default configuration for ASP.NET includes support for serving static content, such as images, CSS stylesheets, and JavaScript files, and it maps requests to the `wwwroot` folder automatically.

    ```HTML
    <link rel="stylesheet" href="~/css/site.css" />
    ```

**Assignment (for you to solve)**
1. Use the common "post/redirect/get" pattern for displaying the `Thanks.cshtml` View.
    Hint: use `TempData` to pass information between actions. `Newtonsoft.Json` might be useful.

##  12. <a name='StylingtheContent'></a>Styling the Content

> Bootstrap is already included in the project. If you are not familiar with it check [http://getbootstrap.com/](http://getbootstrap.com/)

1. Style the `Index.cstml` file as follows

    ```HTML
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
        <title>Index</title>
    </head>
    <body>
        <div class="text-center m-2">
            <h3> We're going to have an exciting party!</h3>
            <h4>And YOU are invited!</h4>
            <a class="btn btn-primary" asp-action="RsvpForm">RSVP Now</a>
        </div>
    </body>
    </html>
    ```
2. Style the `RsvpForm.cstml` file as follows

    ```HTML
    @model CourseInvites.Models.GuestResponse
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>RsvpForm</title>
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
        <link rel="stylesheet" href="~/css/site.css" />
    </head>
    <body>
        <form asp-action="RsvpForm" method="post" class="m-2">
            <div asp-validation-summary="All"></div>
            <div class="form-group">
                <label asp-for="Name" class="form-label">Your name:</label>
                <input asp-for="Name" class="form-control" />
            </div>
            <div class="form-group">
                <label asp-for="Email" class="form-label">Your email:</label>
                <input asp-for="Email" class="form-control" />
            </div>
            <div class="form-group">
                <label asp-for="Phone" class="form-label">Your phone:</label>
                <input asp-for="Phone" class="form-control" />
            </div>
            <div class="form-group">
                <label asp-for="WillAttend" class="form-label">
                    Will you attend?
                </label>
                <select asp-for="WillAttend" class="form-control">
                    <option value="">Choose an option</option>
                    <option value="true">Yes, I'll be there</option>
                    <option value="false">No, I can't come</option>
                </select>
            </div>
            <button type="submit" class="btn btn-primary mt-3">Submit RSVP</button>
        </form>
    </body>
    </html>
    ```

 3. Style the `Thanks.cstml` file as follows
  
    ```HTML
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Thanks</title>
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
    </head>
    <body class="text-center">
        <div>
            <h1>Thank you, @Model?.Name!</h1>
            @if (Model?.WillAttend == true)
            {
                @:It's great that you're coming. The drinks are already in the fridge!
            }
            else
            {
                @:Sorry to hear that you can't make it, but thanks for letting us know.
            }
        </div>
        Click
        <a asp-action="ListResponses">here</a> to see who is coming.
    </body>
    </html>
    ```
 4. Style the `ListResponses.cstml` file as follows

    ```HTML
    @model IEnumerable<CourseInvites.Models.GuestResponse>
    @{
        Layout = null;
    }
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>Responses</title>
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css" />
    </head>
    <body>
        <div class="text-center p-2">
            <h2 class="text-center">
                Here is the list of people attending the party
            </h2>
            <table class="table table-bordered table-striped table-sm">
                <thead>
                    <tr><th>Name</th><th>Email</th><th>Phone</th></tr>
                </thead>
                <tbody>
                    @foreach (CourseInvites.Models.GuestResponse r in Model!)
                    {
                        <tr>
                            <td>@r.Name</td>
                            <td>@r.Email</td>
                            <td>@r.Phone</td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </body>
    </html>
    ```
