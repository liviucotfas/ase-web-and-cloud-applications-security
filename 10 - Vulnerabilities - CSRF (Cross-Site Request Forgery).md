# Cross-Site Request Forgery (XSRF/CSRF) Attacks 

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Documentation](#Documentation)
* 3. [Step 1 - Create an application with individual user accounts](#Step1-Createanapplicationwithindividualuseraccounts)
* 4. [Step 2 - Add the CSRF vulnerable webpage](#Step2-AddtheCSRFvulnerablewebpage)
* 5. [Malicious or infected web site](#Maliciousorinfectedwebsite)
* 6. [Attacks](#Attacks)
* 7. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc --> 

##  1. <a name='Objectives'></a>Objectives

##  2. <a name='Documentation'></a>Documentation
Cross-site request forgery (also known as XSRF or CSRF, pronounced see-surf) is an attack against web-hosted apps whereby a malicious web app can influence the interaction between a client browser and a web app that trusts that browser. These attacks are possible because web browsers send some types of authentication tokens automatically with every request to a website. This form of exploit is also known as a one-click attack or session riding because the attack takes advantage of the user's previously authenticated session.

> Recommended further reading: Open Web Application Security Project - OWASP (description of the vulnerability in general): https://www.owasp.org/index.php/Cross-Site_Request_Forgery_(CSRF); CSRF in ASP.NET: https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery

# Vulnerable website - IronBank Web'Banking

##  3. <a name='Step1-Createanapplicationwithindividualuseraccounts'></a>Step 1 - Create an application with individual user accounts

> :octocat: Full source code available, check the `10 - Vulnerabilities - CSRF (Cross-Site Request Forgery)` folder.

1. Create a new "ASP.NET Core Web App (Model-View-Controller)" project named "IronBankWeb".

2. For "Authentication type" choose "Indvidual Accounts".

3. Notice that in the "Data" folder the application already includes the necessary database migrations.

4. Create an SQL Server database (such as "ism-csrf") and update the connection string in `appsettings.json`.

5. Run the application.

6. Choose the "Register" link and create a new account.

##  4. <a name='Step2-AddtheCSRFvulnerablewebpage'></a>Step 2 - Add the CSRF vulnerable webpage

1. Add a new controller, called `BankAccountController`.
2. Decorate the controller using the `[Authorize]` annotation in order to prevent any unauthenticated requests towards the actions on this controller.
3. Add an action called `Transfer` to the `BankAccountController` as follows.

    ```C#
    public IActionResult Transfer()
    {
        return View();
    }
    ```

4. Add a new viewmodel class to the `Models` folder.

    ```C#
    public class TransferViewModel
    {
        [Required]
        public required string DestinationAccount { get; set; }
        [Required]
        public decimal? Amount { get; set; }
    }
    ```

4. Add a new View for the `Transfer` action.

    ```CSHTML
    @model TransferViewModel
    @{
        ViewData["Title"] = "Transfer";
    }

    <h2>Transfer</h2>

    @if(TempData["message"] != null)
    {
        <div class="alert alert-danger">
            @TempData["message"]
        </div>
    }

    <form method="post">
        <label asp-for="DestinationAccount"></label>
        <input asp-for="DestinationAccount" />
        <label asp-for="Amount"></label>
        <input asp-for="Amount" />
        <input type="submit" />
    </form>
    ```

5. Add a `POST` method for the form in the View

    ```C#
    [HttpPost]
    public IActionResult Transfer(TransferViewModel transfer)
    {
        if (ModelState.IsValid)
        {
            TempData["Message"] = $"You have transfered {transfer.Amount} euros to {transfer.DestinationAccount}.";
            return RedirectToAction(nameof(Transfer));
        }
        return View(transfer);
    }
    ```

##  5. <a name='Maliciousorinfectedwebsite'></a>Malicious or infected web site

> :octocat: Full source code available, check the `10 - Vulnerabilities - CSRF (Cross-Site Request Forgery)` folder.

1. Create an `html` file with the following content.

    ```HTML
    <!DOCTYPE html>
    <html>
    <head>
        <title>Malicious or infected web site</title>
    </head>
        <body style="text-align:center;">

            <img src="winner.jpg">

            <h1>Congratulations!</h1>
            <h1>You have won a Samsung Galaxy Fold (2.000 euros)!!!!</h1>
            <form action="http://good-banking-site.com/api/account" method="post">
                <input type="hidden" name="DestinationAccount" value="RO85INGBXXXXXXXXXX">
                <input type="hidden" name="Amount" value="1000000">
                <input type="submit" value="Click to collect your prize!">
            </form>
        </body>
    </html>
    ```

2. You will need to replace "http://good-banking-site.com/api/account" with the actual address of the vulnerable form (ex: https://localhost:5001/BankAccount/Transfer).

##  6. <a name='Attacks'></a>Attacks

1. Try to perform the attack. You will notice that in modern browsers the attack is not working. 
2. Check the cookie policy in Google Chome.. Notice that the `AspNetCore.Identity.Application` cookie has the `SameSite` policy set to `Strict` (or `Lax`). Let's change the `SameSite` policy to `SameSiteMode.None` by adding the following code to the `Main` method of the `Program` class.

    > Documentation for `SameSite`: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie#samesitesamesite-value

    > Potentially valid reasons for setting the `SameSite` policy to `SameSiteMode.None`: https://andrewlock.net/understanding-samesite-cookies/#:~:text=The%20one%20advantage%20of%20SameSite,and%20Lax%20won't%20work.

    ```C#
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.None;
    });
    ```
3. Try to perform the attack again. The attack should work successfully. 
4. Let's prevent the attack by adding the `[ValidateAntiForgeryToken]` annotation to the `Transfer` action, as shown below.

    ```C#
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Transfer(TransferViewModel transfer)
    {
        ....
    }
    ```
5. Check the hidden input in the form and the cookies set by the application.

    > Have you noticed that the token in the cookie and the one in the hidden input are different? You can read more about the reason at: https://stackoverflow.com/questions/20911470/why-is-there-a-difference-in-the-validateantiforgerytoken-cookie-value-and-hidde

##  7. <a name='Bibliography'></a>Bibliography
- Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core: https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?
- OWASP CSRF: https://www.owasp.org/index.php/Cross-Site_Request_Forgery_(CSRF)
