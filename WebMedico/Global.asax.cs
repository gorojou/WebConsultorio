using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebMedico.Models;

namespace WebMedico
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null)
                {
                    var doctor = JsonConvert.DeserializeObject<CustomPrincipalSerializedModel>(authTicket.UserData);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        var newUser = new CustomPrincipal(authTicket.Name)
                        {
                            IdDoctor = doctor.IdDoctor,
                            Name = doctor.Name,
                            LastName = doctor.LastName,
                            Email = doctor.Email,
                            AlwaysLoggedIn = doctor.AlwaysLoggedIn
                        };
                        HttpContext.Current.User = newUser;
                    }
                    else
                    {
                        if (doctor.AlwaysLoggedIn)
                        {
                            //reset authentication ticket
                            //create new cookie
                            doctor.Name = "Cookie recriado para login";
                            FormsAuthentication.SetAuthCookie(doctor.Email, doctor.AlwaysLoggedIn);
                            var authTicket2 = new FormsAuthenticationTicket(1, doctor.Email, DateTime.Now, (doctor.AlwaysLoggedIn) ? DateTime.Now.AddDays(5) : DateTime.Now.AddMinutes(20), doctor.AlwaysLoggedIn, JsonConvert.SerializeObject(doctor));
                            var encryptedTicket2 = FormsAuthentication.Encrypt(authTicket2);
                            var authCookie2 = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket2);
                            authCookie2.Expires = DateTime.Now.AddMinutes(1);
                            HttpContext.Current.Response.Cookies.Add(authCookie2);
                        }
                    }
                }
            }
            else {
                //this should redirect to login page
            }
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
    }
}