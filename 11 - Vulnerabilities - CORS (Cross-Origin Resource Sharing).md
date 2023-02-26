#  Vulnerabilities - CORS (Cross-Origin Resource Sharing) Attacks

<!-- vscode-markdown-toc -->
* 1. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc --> 

## Objectives

## Documentation
You can learn more details about this type of attack at https://portswigger.net/web-security/cors . 

## Scenario
In the following we want to focus on "Exploiting XSS via CORS trust relationships" 

> Even "correctly" configured CORS establishes a trust relationship between two origins. If a website trusts an origin that is vulnerable to cross-site scripting (XSS), then an attacker could exploit the XSS to inject some JavaScript that uses CORS to retrieve sensitive information from the site that trusts the vulnerable application. Source: https://portswigger.net/web-security/cors

Let's suppose that we have two applications. The first one includes a webpage where users can find some secret information (ex: their API Key, serial number, etc.) after authenticating themselves. The second application could be a partner website or another website belonging to our organization that needs to be able to perform client-side requests to the first application.

Let's suppose that the attacker has already been able to perform a successful **XSS** attack on a **Vulnerable** website . He could now include the following JavaScript that will be executed for each user that access the **Partner** website.

> We should specify in the fetch method that the credentials should be sent when making the request to the **Attacked** website. Documentation: https://developer.mozilla.org/en-US/docs/Web/API/fetch#credentials

```
 fetch('https://localhost:7093/BankAccount/Transfer', { credentials: "include" })
            .then((response) => response.text())
            .then(html => console.log(html)) // here we could send the data
```

## Step 1 - Create the first application (the one conaining the secret)
1. In an existing or a new application supporting implementing individual user accounts add the following `APITokensController`:

    ```
    [Authorize]
    public class APITokensController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
    ```

2. Corresponding to the `Index` action, add the following view:

    ```
    @{
        ViewData["Title"] = "Authentication Tokens";
    }

    <h2>Authentication Tokens</h2>

    Access Token: BKBEHGIE87274234293
    ```
## Step 2 - Create the second application (that is vulnerable to XSS)
1. Create an application including a View that contains the following JavaScript code (let's suppose that it is there due to an XSS attack). Note: Don't forget to update the port.

    ```
    fetch('https://localhost:7093/APITokens', { credentials: "include" })
            .then((response) => response.text())
            .then(html => console.log(html)) // here we could send the data
    ```

## Step 3 - Performing the attack
1. Check what gets displayed in the console for the second application.
2. Update the `Main` method in the `Program` class as follows for the first application to enable CORS. Note: Don't forget to update the port.
    ```
    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
                            policy =>
                            {
                                policy.WithOrigins("https://localhost:7102")
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                            });
    });
    ```
3. Check what gets displayed in the console for the second application.


##  1. <a name='Bibliography'></a>Bibliography

- Fetch method: https://developer.mozilla.org/en-US/docs/Web/API/fetch#credentials
- ASP.NET CORS settings: https://learn.microsoft.com/en-us/aspnet/core/security
- https://lo-victoria.com/introduction-to-cross-origin-resource-sharing-cors
- https://portswigger.net/web-security/cors
- https://medium.com/@zhaojunemail/sop-cors-csrf-and-xss-simply-explained-with-examples-af6119156726