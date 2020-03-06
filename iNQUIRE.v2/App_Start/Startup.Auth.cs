using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using iNQUIRE.Models;
using System.Configuration;
using Owin.Security.Providers.Yahoo;
using Microsoft.Owin.Security.MicrosoftAccount;

namespace iNQUIRE
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.Use((context, next) =>
            {
                if (context.Request.Headers["x-forwarded-proto"] == "https")
                {
                    context.Request.Scheme = "https";
                }
                return next();
            });

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers

            // Microsoft
            var ms_client_id = ConfigurationManager.AppSettings["MicrosoftClientId"];
            var ms_client_secret = ConfigurationManager.AppSettings["MicrosoftClientSecret"];

            if ((!String.IsNullOrEmpty(ms_client_id)) && (!String.IsNullOrEmpty(ms_client_secret)))
            {
                var mo = new MicrosoftAccountAuthenticationOptions
                {
                    ClientId = ms_client_id,
                    ClientSecret = ms_client_secret,
                };
                mo.Scope.Add("wl.basic");
                mo.Scope.Add("wl.emails");
                app.UseMicrosoftAccountAuthentication(mo);
            }

            //// Twitter
            //var t_consumer_key = ConfigurationManager.AppSettings["TwitterConsumerKey"];
            //var t_consumer_secret = ConfigurationManager.AppSettings["TwitterConsumerSecret"];

            //if ((!String.IsNullOrEmpty(t_consumer_key)) && (!String.IsNullOrEmpty(t_consumer_secret)))
            //{
            //    app.UseTwitterAuthentication(
            //       consumerKey: t_consumer_key,
            //       consumerSecret: t_consumer_secret);
            //}


            // Facebook
            var fb_app_id = ConfigurationManager.AppSettings["FacebookAppId"];
            var fb_app_secret = ConfigurationManager.AppSettings["FacebookAppSecret"];

            if ((!String.IsNullOrEmpty(fb_app_id)) && (!String.IsNullOrEmpty(fb_app_secret)))
            {
                app.UseFacebookAuthentication(
                   appId: fb_app_id,
                   appSecret: fb_app_secret);
            }


            // Google
            var g_client_id = ConfigurationManager.AppSettings["GoogleClientId"];
            var g_client_secret = ConfigurationManager.AppSettings["GoogleClientSecret"];

            if ((!String.IsNullOrEmpty(g_client_id)) && (!String.IsNullOrEmpty(g_client_secret)))
            {
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
                {
                    ClientId = g_client_id,
                    ClientSecret = g_client_secret,
                    Provider = new GoogleOAuth2AuthenticationProvider()
                });
            }


            //// Yahoo
            //var y_client_key = ConfigurationManager.AppSettings["YahooClientKey"];
            //var y_client_secret = ConfigurationManager.AppSettings["YahooClientSecret"];

            //if ((!String.IsNullOrEmpty(y_client_key)) && (!String.IsNullOrEmpty(y_client_secret)))
            //{
            //    app.UseYahooAuthentication(y_client_key, y_client_secret);
            //}
        }
    }
}