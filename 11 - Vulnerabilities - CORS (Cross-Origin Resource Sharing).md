#  Vulnerabilities - CORS (Cross-Origin Resource Sharing) Attacks

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Documentation](#Documentation)
* 3. [Scenario](#Scenario)
* 4. [Step 1 - Create the first application (the one conaining the secret)](#Step1-Createthefirstapplicationtheoneconainingthesecret)
* 5. [Step 2 - Create the second application (that is vulnerable to XSS)](#Step2-CreatethesecondapplicationthatisvulnerabletoXSS)
* 6. [Step 3 - Performing the attack](#Step3-Performingtheattack)
* 7. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc --> 

##  1. <a name='Objectives'></a>Objectives

##  2. <a name='Documentation'></a>Documentation
You can learn more details about this type of attack at https://portswigger.net/web-security/cors . 

##  3. <a name='Scenario'></a>Scenario
In the following we want to focus on "Exploiting XSS via CORS trust relationships" 

> Even "correctly" configured CORS establishes a trust relationship between two origins. If a website trusts an origin that is vulnerable to Cross-Site Scripting (XSS), then an attacker could exploit the XSS to inject some JavaScript that uses CORS to retrieve sensitive information from the site that trusts the vulnerable application. Source: https://portswigger.net/web-security/cors .

Let's suppose that we have two applications. The first one includes a webpage where users can find some secret information (ex: their API Key, serial number, etc.) after authenticating themselves. The second application could be a partner website or another website belonging to our organization that needs to be able to perform client-side requests to the first application.

Let's suppose that the attacker has already been able to perform a successful **XSS** attack on a **Vulnerable** website . He could now include the following JavaScript that will be executed for each user that access the **Partner** website.

> We should specify in the fetch method that the credentials should be sent when making the request to the **Attacked** website. Documentation: https://developer.mozilla.org/en-US/docs/Web/API/fetch#credentials

```
 fetch('https://localhost:7093/BankAccount/Transfer', { credentials: "include" })
            .then((response) => response.text())
            .then(html => console.log(html)) // here we could send the data
```

##  4. <a name='Step1-Createthefirstapplicationtheoneconainingthesecret'></a>Step 1 - Create the first application (the one conaining the secret)
1. In an existing or a new application implementing individual user accounts add the following `APITokensController` controller:

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
##  5. <a name='Step2-CreatethesecondapplicationthatisvulnerabletoXSS'></a>Step 2 - Create the second application (that is vulnerable to XSS)
1. Create an application including a page (`html`, `Razor`, etc.) that contains the following JavaScript code (let's suppose that it is there due to an **XSS** attack). Note: Don't forget to update the port.

    ```
    fetch('https://localhost:7093/APITokens', { credentials: "include" })
            .then((response) => response.text())
            .then(html => console.log(html)) // here we could send the data
    ```

##  6. <a name='Step3-Performingtheattack'></a>Step 3 - Performing the attack
1. Check what gets displayed in the console for the second application.
2. Update the `Main` method in the `Program` class as follows for the first application to enable **CORS**. 
   > Note: Don't forget to update the port. You should run the applciationb using a webserver (e.g.,"Live Server" in VS Code, etc.)

    First, call `AddCors`.
    ```
    string corsPolicyForSecondDomain = "AllowSecondDomain";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: corsPolicyForSecondDomain,
                            policy =>
                            {
                                policy.WithOrigins("https://localhost:7102")
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                            });
    });
    ```
    Second, call `UseCors`.

    ```
    app.UseRouting();

    app.UseCors(corsPolicyForSecondDomain);

    app.UseAuthentication();
    ```
3. Check what gets displayed in the console for the second application.


##  7. <a name='Bibliography'></a>Bibliography

- Fetch method: https://developer.mozilla.org/en-US/docs/Web/API/fetch#credentials
- ASP.NET CORS settings: https://learn.microsoft.com/en-us/aspnet/core/security
- https://lo-victoria.com/introduction-to-cross-origin-resource-sharing-cors
- https://portswigger.net/web-security/cors
- https://medium.com/@zhaojunemail/sop-cors-csrf-and-xss-simply-explained-with-examples-af6119156726