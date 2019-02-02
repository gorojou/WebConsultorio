using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebMedico.Models;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Web.Security;
using System.Drawing;
using System.Configuration;
using System.Collections.Generic;
using IFarmacias.Models.Response;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace WebMedico.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly string _urlTargetBase = ConfigurationManager.AppSettings["ApiUrl"];

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            bool isauthenticated = System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (!isauthenticated)
                return View();
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult CheckifEmailExists(string email)
        {
            try
            {
                var url = $"{_urlTargetBase}/api/Doctor/VerifyifEmailExists";
                var checkemailJson = new JavaScriptSerializer().Serialize(new
                {
                    email
                });
                using (var client = new HttpClient())
                {
                    var wcfResponse = client.PostAsync(url, new StringContent(checkemailJson, Encoding.UTF8, "application/json")).Result;
                    using (wcfResponse)
                    {
                        if (!wcfResponse.IsSuccessStatusCode)
                            return Json(true, JsonRequestBehavior.AllowGet);
                        
                        var json = wcfResponse.Content.ReadAsStringAsync().Result;
                        bool ifexists = JsonConvert.DeserializeObject<bool>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        return Json(ifexists, JsonRequestBehavior.AllowGet); 
                    }
                }
            }
            catch (Exception ex)
            {
                // No re-lanzar la excepcion, usar un Log.
                throw ex;
            }
        }
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            try
            {
                var url = $"{_urlTargetBase}/api/Doctor/Login";
                ViewBag.Error = string.Empty;
                var loginJson = new JavaScriptSerializer().Serialize(new
                {
                    model.Email,
                    model.Password
                });
                using (var client = new HttpClient())
                {
                    HttpResponseMessage wcfResponse = await client.PostAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                    if (wcfResponse.IsSuccessStatusCode)
                    {
                        var json = wcfResponse.Content.ReadAsStringAsync().Result;
                        var resp = JsonConvert.DeserializeObject<IFarmacias.Models.Response.DoctorResp> (json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (resp.status == IFarmacias.Models.Response.DoctorResponseStatus.SuccessfulAuth)
                        {
                            // Asignar el doctor que devuelve el API
                            IFarmacias.Models.Response.DoctorResponse doctor = resp.response; // <--- Asignarle algo para que no vuelva entrar al Login
                            if (doctor.PHOTO != null)
                                Session["PHOTO"] = doctor.PHOTO;
                            var serializeModel = new CustomPrincipalSerializedModel
                            {
                                IdDoctor = doctor.ID_DOCTOR,
                                Name = doctor.NAME,
                                LastName = doctor.LASTNAME,
                                Email = doctor.EMAIL,
                                AlwaysLoggedIn = model.RememberMe
                            };
                            FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
                            var authTicket = new FormsAuthenticationTicket(1, doctor.EMAIL, DateTime.Now, (model.RememberMe) ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(4), model.RememberMe, JsonConvert.SerializeObject(serializeModel));
                            var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                            authCookie.Expires = authTicket.Expiration;
                            HttpContext.Response.Cookies.Add(authCookie);
                            
                            return RedirectToAction("Index", "Home");
                        }
                        else if (resp.status == IFarmacias.Models.Response.DoctorResponseStatus.EmailDoesntExist)
                        {
                            ViewBag.Error = ResourcesAccount.UserNotFound;// "Usuário não encontrado";
                        }
                        else if (resp.status == IFarmacias.Models.Response.DoctorResponseStatus.IncorrectPassword)
                        {
                            ViewBag.Error = ResourcesAccount.UserNotFound;// "Usuário com esta chave não pôde ser encontrado";
                        }
                        else if (resp.status == IFarmacias.Models.Response.DoctorResponseStatus.AnotherIssue)
                        {
                            ViewBag.Error = ResourcesAccount.AuthenticationIssues;// "Desculpe por algum inconveniente na autenticação";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // No re-lanzar la excepcion, usar un Log.
                throw ex;
            }
            return View();
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects ogi brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag._urlTargetBase = this._urlTargetBase;
            ViewBag.cmbEspecialidad = GetSpecialtyResponseSync();
            return View();
        }

        public Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public ActionResult ImgUpload(HttpPostedFileBase uploadFile)
        {
            if (uploadFile.ContentLength > 0)
            {
                string relativePath = "~/img/" + Path.GetFileName(uploadFile.FileName);
                string physicalPath = Server.MapPath(relativePath);
                uploadFile.SaveAs(physicalPath);
                return View((object)relativePath);
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        // Arsenio: Utilización de Register con ViewModels
        public async Task<ActionResult> Register(DoctorViewModel model)
        {
            try
            {
                ViewBag.cmbEspecialidad = GetSpecialtyResponseSync();
                var url = $"{_urlTargetBase}/api/Doctor/New";
                
                var loginJson = JsonConvert.SerializeObject(new
                {
                    model.NAME,
                    model.LASTNAME,
                    model.EMAIL,
                    model.PASSWORD,
                    model.GENDER,
                    model.ID_COUNTRY,
                    model.SPECIALTY,
                    model.ORDER_NUMBER,
                    model.INIT_DATE_CAREER,
                    model.BIRTH_DATE,
                    model.LANGUAGES,
                    model.TAXPAYER,
                    model.ADDRESS,
                    model.LOCALITY,
                    model.ZIPCODE,
                    model.MOBILE_NUMBER,
                    model.PHOTO,
                    DATECREATED = DateTime.Now,
                    STATUS = true
                });
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var wcfResponse = await client.PostAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));

                    switch (wcfResponse.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            ModelState.AddModelError("Registrado", "");
                            return View();
                        case HttpStatusCode.NotFound:
                            ModelState.AddModelError("NoRegistrado", "");
                            break;

                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.BadRequest:
                            ModelState.AddModelError("NoRegistrado", "");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ErrorGeneral", ex.Message);
            }
            ModelState.AddModelError("NoRegistrado", "");
            return View(model);
        }


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //POST: /Account/RecoveryPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                string Password = string.Empty;
                var url = $"{_urlTargetBase}/api/Doctor/RecoveryPassword";

                string repassJson = new JavaScriptSerializer().Serialize(new
                {
                    Email = model.Email,
                    Password = Password
                });

                using (var client = new HttpClient())
                {

                    HttpResponseMessage wcfResponse = await client.PostAsync(url, new StringContent(repassJson, Encoding.UTF8, "application/json"));

                    switch (wcfResponse.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            ModelState.AddModelError("Enviado", "");
                            break;

                        case HttpStatusCode.NotFound:
                            ModelState.AddModelError("NoEnviado", "");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        //GET: /Account/VerifyRecoveryPassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return View("Error");

                string Password = string.Empty;
                var url = $"{_urlTargetBase}/api/Doctor/VerifyRecoveryPassword";

                string verepassJson = new JavaScriptSerializer().Serialize(new
                {
                    Token = token,
                    Password = Password
                });

                using (var client = new HttpClient())
                {
                    HttpResponseMessage wcfResponse = await client.PostAsync(url, new StringContent(verepassJson, Encoding.UTF8, "application/json"));

                    switch (wcfResponse.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            //return RedirectToAction("ResetPassword", "Account", new { code = "ok" });
                            ResetPasswordViewModel model = new ResetPasswordViewModel();
                            model.token = token;
                            return View(model);
                        case HttpStatusCode.NotFound:
                            return View("Error");
                        case HttpStatusCode.InternalServerError:
                            return View("Error");
                        default:
                            return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        //PUT: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                var url = $"{_urlTargetBase}/api/Doctor/ResetPassword";

                string RstpassJson = new JavaScriptSerializer().Serialize(new
                {
                    Token = model.token,
                    Password = model.Password
                });

                using (var client = new HttpClient())
                {
                    HttpResponseMessage wcfResponse = await client.PutAsync(url, new StringContent(RstpassJson, Encoding.UTF8, "application/json"));

                    switch (wcfResponse.StatusCode)
                    {

                        case HttpStatusCode.OK:
                            ViewBag.Exito = "palavra-passe criada";
                            break;

                        case HttpStatusCode.MethodNotAllowed:
                            ViewBag.Error = "palavra-passe não criada";
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        //// GET: /Account/ResetPassword
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public ActionResult ResetPassword(string code)
        public ActionResult ResetPassword()
        {

            // return code == null ? View("Error") : View();
            return View();

        }



        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[AllowAnonymous]
        public ActionResult LogOff()
        {
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //MySession.Current.Doctor = null;
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        // ¨POST: /Account/MedicalSpecialty
        // Metodo para recuperar del webservice MedicalSpecialty las especialidades disponibles
        public List<object> GetSpecialtyResponseSync()
        {
            List<Object> lista = new List<Object>();
            try
            {
                var queries = new List<EspecialidadResponse>();

                var url = $"{_urlTargetBase}/api/MedicalSpecialty";

                lista.Clear();

                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    var wcfResponse = client.DownloadString(url);
                    queries = JsonConvert.DeserializeObject<List<EspecialidadResponse>>(wcfResponse);
                }
                foreach (IFarmacias.Models.Response.EspecialidadResponse elemento in queries)
                {
                    lista.Add(new { value = elemento.ID_ESPECIALIDAD, text = elemento.ESPECIALIDAD });
                }
            }
            catch (WebException ex)
            {
                //throw;
                lista.Add(new { value = "", text = "" });
            }

            return lista;
        }

        // ¨POST: /Account/Idioms
        // Metodo para devolver la lista de idiomas
        public List<object> GetIdiomsResponseSync()
        {
            List<Object> list = new List<Object>();
            try
            {
                var queries = new List<IdiomResponse>();

                var url = $"{_urlTargetBase}/api/Idioms";

                list.Clear();

                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    var wcfResponse = client.DownloadString(url);
                    queries = JsonConvert.DeserializeObject<List<IdiomResponse>>(wcfResponse);
                }
                foreach (IdiomResponse elemento in queries)
                {
                    list.Add(new { value = elemento.IdIdiom, text = elemento.Idiom });
                }
            }
            catch (WebException ex)
            {
                //throw;
                list.Add(new { value = "", text = "" });
            }

            return list;
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
    }

}