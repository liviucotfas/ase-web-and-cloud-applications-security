# Styling, PartialViews

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Applying Bootstrap Styles](#ApplyingBootstrapStyles)
* 3. [Partial Views](#PartialViews)
* 4. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- adding client-side libraries;
- styling with Bootstrap;
- creating and using partial views.

##  2. <a name='ApplyingBootstrapStyles'></a>Applying Bootstrap Styles

1. Add a new folder called `wwwroot` to the project.
2. Add the "Bootstrap" framework to your project by right clicking on the project and chosing "Add" > "Client Side Library". Use "cdnjs" as a provider and search for "twitter" in order to install version 5.x. 
3. Razor layouts provide common content so that it doesn’t have to be repeated in multiple views. Update the `_Layout.cshtml` file in the `Views/Shared` folder to include the Bootstrap CSS stylesheet in the content sent to the browser and define a common header that will be used throughout the application

    ```CSHTML
    <!DOCTYPE html>

    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>@ViewBag.Title</title>

        <!-- !!!! new/updated code { -->
        <link href="/lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
        <!-- } -->
    </head>
    <body>
        <!-- !!!! new/updated code { -->
        <div class="bg-dark text-white p-2">
            <span class="navbar-brand ml-2">MVC STORE</span>
        </div>
        <div class="row m-1 p-1">
            <div id="categories" class="col-3">
                Put something useful here later
            </div>
            <div class="col-9">
                @RenderBody()
            </div>
        </div>
        <!-- } -->
        @await RenderSectionAsync("Scripts", required: false)
    </body>
    </html>
    ```
4. Also update the styling applied to the `Index.cshtml` file.

    ```CSHTML
    @model ProductsListViewModel

    @foreach (var p in Model.Products)
    {
        <div class="card card-outline-primary m-1 p-1">
            <div class="bg-faded p-1">
                <h4>
                    @p.Name
                    <span class="badge badge-pill badge-primary" style="float:right">
                        <small>@p.Price.ToString("c")</small>
                    </span>
                </h4>
            </div>
            <div class="card-text p-1">@p.Description</div>
        </div>
    }

    <div page-model="@Model.PagingInfo" 
        page-action="Index" 
        page-classes-enabled="true"
        page-class="btn" 
        page-class-normal="btn-outline-dark"
        page-class-selected="btn-primary" 
        class="btn-group pull-right m-1">
    </div>
    ```
5. We need to style the buttons generated by the `PageLinkTagHelper` class, but we don’t want to hardwire the Bootstrap classesinto the C# code because it makes it harder to reuse the tag helper elsewhere in the application or change the appearance of the buttons. Instead, we have defined custom attributes on the div element that specify the classes that we require, and these correspond to
properties we added to the tag helper class, which are then used to style the a elements that are produced.

    ```C#
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }
        public PagingInfo? PageModel { get; set; }
        public string? PageAction { get; set; }

        // new/updated code {
        public bool PageClassesEnabled { get; set; } = false;
        public string PageClass { get; set; } = String.Empty;
        public string PageClassNormal { get; set; } = String.Empty;
        public string PageClassSelected { get; set; } = String.Empty;
        //}

        public override void Process(TagHelperContext context,
        TagHelperOutput output)
        {
            if (ViewContext != null && PageModel != null)
            {
                IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
                TagBuilder result = new TagBuilder("div");
                for (int i = 1; i <= PageModel.TotalPages; i++)
                {
                    TagBuilder tag = new TagBuilder("a");
                    tag.Attributes["href"] = urlHelper.Action(PageAction, new { productPage = i });

                     // new/updated code {
                    if (PageClassesEnabled)
                    {
                        tag.AddCssClass(PageClass);
                        tag.AddCssClass(i == PageModel.CurrentPage
                        ? PageClassSelected : PageClassNormal);
                    }
                    //}

                    tag.InnerHtml.Append(i.ToString());
                    result.InnerHtml.AppendHtml(tag);
                }
                output.Content.AppendHtml(result.InnerHtml);
            }
        }
    }
    ```

    > The values of the attributes are automatically used to set the tag helper property values, with the mapping between the HTML attribute name format  (page-class-normal) and the C# property name format (PageClassNormal) taken into account. This allows tag helpers to respond differently based on the attributes of an HTML element, creating a more flexible way to generate content in an ASP.NET Core application.

##  3. <a name='PartialViews'></a>Partial Views

6. Creating a Partial View. We would like to refactor the application to simplify the `Index.cshtml` view. We are going to create a partial view, which is a fragment of content that we can embed into another view, rather like a template. They help reduce duplication when you need the same content to appear in different places in an application. To create the partial view, we added a Razor View called `ProductSummary.cshtml` to the `Views/Shared` folder and added the markup shown below.

    ```CSHTML
    @model Product
    <div class="card card-outline-primary m-1 p-1">
        <div class="bg-faded p-1">
            <h4>
                @Model?.Name
                <span class="badge rounded-pill bg-primary text-white"
                    style="float:right">
                    <small>@Model?.Price.ToString("c")</small>
                </span>
            </h4>
        </div>
        <div class="card-text p-1">@Model?.Description</div>
    </div>
    ```
7. Update the `Index.cshtml` file in the `Views/Home` folder so that it uses the partial view.

    ```CSHTML
    @model ProductsListViewModel
    
    @foreach (var p in Model?.Products ?? Enumerable.Empty<Product>())
    {
        <partial name="ProductSummary" model="p" />
    }

    <div page-model="@Model?.PagingInfo" page-action="Index" page-classes-enabled="true"
        page-class="btn" page-class-normal="btn-outline-dark"
        page-class-selected="btn-primary" class="btn-group pull-right m-1">
    </div>
    ```

    > We have taken the markup that was previously in the `@foreach` expression in the `Index.cshtml` view and moved it to the new partial view. We call the partial view using a partial element, using the name and model attributes to specify the name of the partial view and its view model. Using a partial view allows the same markup to be inserted into any view that needs to display a summary of a product.

##  4. <a name='Bibliography'></a>Bibliography