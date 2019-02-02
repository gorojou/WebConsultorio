using IFarmacias.Models;
using IFarmacias.Models.Enums;
using IFarmacias.Models.Request;
using IFarmacias.Models.Response;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebMedico.Models;
using WebMedico.Models.Consults;

namespace WebMedico.Controllers
{
    [System.Web.Mvc.Authorize]
    public class HomeController : Controller
    {
        //PARAMETERS FOR SESSION TIME OUT
        private string millisecWarning = System.Configuration.ConfigurationManager.AppSettings["MilliSecWarning"];

        private string millisecRedir = System.Configuration.ConfigurationManager.AppSettings["MilliSecRedir"];
        private string titleSession = System.Configuration.ConfigurationManager.AppSettings["TitleSession"];
        private string messageSession = System.Configuration.ConfigurationManager.AppSettings["MessageSession"];
        private string keepAliveButton = System.Configuration.ConfigurationManager.AppSettings["KeepAliveButton"];
        private string logoutButton = System.Configuration.ConfigurationManager.AppSettings["LogoutButton"];
        private string actionName = System.Configuration.ConfigurationManager.AppSettings["ActionName"];
        private string controllerName = System.Configuration.ConfigurationManager.AppSettings["ControllerName"];
        private string keepAlive = System.Configuration.ConfigurationManager.AppSettings["KeepAlive"];
        private string ignoreUserActivity = System.Configuration.ConfigurationManager.AppSettings["ignoreUserActivity"];
        private string countdownBar = System.Configuration.ConfigurationManager.AppSettings["CountdownBar"];
        private string countdownMessage = System.Configuration.ConfigurationManager.AppSettings["CountdownMessage"];
        private string countdownSmart = System.Configuration.ConfigurationManager.AppSettings["CountdownSmart"];

        /// <summary>
        /// API Base Uri
        /// </summary>
        private readonly string _urlTargetBase = ConfigurationManager.AppSettings["ApiUrl"].ToString();

        public async Task<ActionResult> Index(int? skip, int? take)
        {
            //var queries = new List<QueriesViewModel>();
            //return View(queries);

            //if (MySession.Current.Doctor == null)
            //    return RedirectToAction("Login", "Account");

            /*
             * PARA SALTARME EL LOGIN
             */
            if (skip == null)
                skip = 0;

            if (take == null || take == 0)
                take = 10;

            var idDoctor = ((CustomPrincipal)HttpContext.User).IdDoctor;
            var inProgressUrl =
                $"{_urlTargetBase}/api/consultas/{idDoctor}/inprogress";
            var openUrl =
                $"{_urlTargetBase}/api/consultas/open?skip=0&take={take}";

            var inProgressResultList = new List<ConsultasResponse>();
            var result = await PerformGetConsultasAsync(inProgressUrl);
            if (result != null)
                inProgressResultList.AddRange(result);

            var openResultList = new List<ConsultasResponse>();
            result = await PerformGetConsultasAsync(openUrl);
            if (result != null)
                openResultList.AddRange(result);

            // Get session time out parameters
            SessionTimeOutConfig timeOutConfig = GetTimeOutConfigValues();

            return View(new ConsultaDashBoardViewModel
            {
                InProgressList = inProgressResultList,
                OpenList = openResultList,
                Take = take.Value,
                Skip = 0,
                SessionConfig = timeOutConfig
            });
        }

        // MED-99 SignalR
        /// <summary>
        /// Method to list all consultations
        /// </summary>
        /// <returns>partial view</returns>
        public async Task<ActionResult> GetConsultationDashBoard()
        {
            //for default, we will retrieve only 10 elements.
            const int take = 10;
            var idDoctor = ((CustomPrincipal)HttpContext.User).IdDoctor;
            var inProgressUrl =
                $"{_urlTargetBase}/api/consultas/{idDoctor}/inprogress";
            var openUrl =
                $"{_urlTargetBase}/api/consultas/open?skip=0&take={take}";

            var inProgressResultList = new List<ConsultasResponse>();
            var result = await PerformGetConsultasAsync(inProgressUrl);
            if (result != null)
                inProgressResultList.AddRange(result);

            var openResultList = new List<ConsultasResponse>();
            result = await PerformGetConsultasAsync(openUrl);
            if (result != null)
                openResultList.AddRange(result);

            // Get session time out parameters
            SessionTimeOutConfig timeOutConfig = GetTimeOutConfigValues();

            return PartialView("~/Views/Home/_ConsultaDashBoard.cshtml", new ConsultaDashBoardViewModel
            {
                InProgressList = inProgressResultList,
                OpenList = openResultList,
                Take = take,
                Skip = 0,
                SessionConfig = timeOutConfig
            });
        }

        // MED-78 Consume the Methods for list Notifications
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> ListNotifications(string idDoctor)
        {
            var result = string.Empty;
            var url = $"{_urlTargetBase}/api/Notifications?idDoctor={idDoctor}";
            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    result = wcfResponse.Content.ReadAsStringAsync().Result;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // MED-79 Consume the Methods for list Messages
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> ListMessages(string idDoctor)
        {
            var result = string.Empty;
            string url = $"{_urlTargetBase}/api/Messages?idDoctor={idDoctor}";
            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    result = wcfResponse.Content.ReadAsStringAsync().Result;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Para hacer assumir la consulta. Es PUT: /api/Consultas
        public async Task<ActionResult> AssumeConsultation(string idConsulta)
        {
            try
            {
                var url = $"{_urlTargetBase}/api/Consultas/Assume";
                var loginJson = new JavaScriptSerializer().Serialize(new
                {
                    IdConsultas = idConsulta,
                    IdDoctor = ((CustomPrincipal)HttpContext.User).IdDoctor,
                    Description = "",
                    Status = ConsultationStatus.InProgress
                });

                using (var client = new HttpClient())
                {
                    var wcfResponse = await client.PutAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                    if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                    {
                        //Call to signalr method to dashboard update
                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<Hubs.DashBoardHub>();
                        hubContext.Clients.All.refreshTable();

                        var json = wcfResponse.Content.ReadAsStringAsync().Result;

                        var obj = JsonConvert.DeserializeObject(json);

                        return Json(obj, JsonRequestBehavior.AllowGet);
                    }
                }

                throw new Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<ActionResult> ConsultDetail(string idConsulta)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Patient";
            ViewBag.ModelDoctor = HttpContext.User as CustomPrincipal;
            // Get session time out parameters
            SessionTimeOutConfig timeOutConfig = GetTimeOutConfigValues();

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<BasicInfoViewModel>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    details.IdConsult = idConsulta;
                    details.SessionConfig = timeOutConfig;
                   
                    return View(details);
                }
            }
            return View();
        }

        public async Task<ActionResult> MantenerConsult(int idConsult, string Notes, string Nome, string Nif, string Morada, string Cidade,
           string CodigoPostal, string CodigoPais, string TipoPedido)
        {
            var url = $"{_urlTargetBase}/api/Consultas/ChangeStatusConsultation";

            string file = Server.MapPath("~/Content/Assets/json/ConsultType.json");
            string Json = System.IO.File.ReadAllText(file);
            List<ConsultInfo> _consultas = JsonConvert.DeserializeObject<List<ConsultInfo>>(Json);
            var consulta = _consultas.Where(x => x.ReferenceCode == TipoPedido).Select(x => x).FirstOrDefault();

            ChangeStatusConsultRequest request = new ChangeStatusConsultRequest()
            {
                IdConsultas = idConsult,
                NomeApelido = Nome,
                Nif = Nif,
                Morada = Morada,
                Localidade = Cidade,
                Zipcode = CodigoPostal,
                CountryIsoCode = CodigoPais,
                Status = ConsultationStatus.KeepInvoiced,
                ConsultInfo = consulta,
                Note = Notes
            };
            var requestJson = new JavaScriptSerializer().Serialize(request);

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<ChangeStatusConsultResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return PartialView("_ModalInvoiceResult", details);
                }
            }

            throw new Exception();
        }

        public async Task<ActionResult> ArchivarConsult(int idConsult, string Nome, string Nif, string Morada, string Cidade,
           string CodigoPostal, string CodigoPais, string TipoPedido)
        {
            var url = "localhost/api/Consultas/ChangeStatusConsultation";

            string file = Server.MapPath("~/Content/Assets/json/ConsultType.json");
            string Json = System.IO.File.ReadAllText(file);
            List<ConsultInfo> _consultas = JsonConvert.DeserializeObject<List<ConsultInfo>>(Json);
            var consulta = _consultas.Where(x => x.ReferenceCode == TipoPedido).Select(x => x).FirstOrDefault();

            ChangeStatusConsultRequest request = new ChangeStatusConsultRequest()
            {
                IdConsultas = idConsult,
                NomeApelido = Nome,
                Nif = Nif,
                Morada = Morada,
                Localidade = Cidade,
                Zipcode = CodigoPostal,
                CountryIsoCode = CodigoPais,
                Status = ConsultationStatus.Invoiced,
                ConsultInfo = consulta
            };
            var requestJson = new JavaScriptSerializer().Serialize(request);

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<ChangeStatusConsultResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return PartialView("_ModalInvoiceResult", details);
                }
            }
            throw new Exception();
        }

        public async Task<ActionResult> CancelConsult(string idConsult)
        {
            var url = "localhost/api/Consultas/ChangeStatusConsultation";
            var obJson = new JavaScriptSerializer().Serialize(new
            {
                IdConsultas = idConsult,
                Status = ConsultationStatus.Open
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<Hubs.DashBoardHub>();
                    hubContext.Clients.All.refreshTable();

                    return RedirectToAction("Index", "Home");
                }
            }

            throw new Exception();
        }

        public PartialViewResult _TabSoap(string idConsulta)
        {
            var url = "localhost/api/Consultas/ChangeStatusConsultation";
            
            var obJson = new
            {
                IdConsultas = 1,
                Status = 2
            };

            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(url, .GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var wcfResponse = await client.PutAsync(url, new StringContent(serializedModel, Encoding.UTF8, "application/json")); ;
                    var details = JsonConvert.DeserializeObject<SoapResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabSoap", details);
                }
            }
            return PartialView("_TabSoap");
        }

        [HttpPost]
        public async Task<JsonResult> SetTabSoap(string idConsulta, SoapResponse model)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Soap";

            var serializedModel = new JavaScriptSerializer().Serialize(model);

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PutAsync(url, new StringContent(serializedModel, Encoding.UTF8, "application/json")); ;
                return Json(new { IsSuccess = wcfResponse.IsSuccessStatusCode });
            }
        }

        public PartialViewResult _TabAnexos(string idConsulta)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Attachments";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<AttachmentInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabAnexos", details);
                }
            }
            var attachments = new List<AttachmentInfoResponse>();
            return PartialView("_TabAnexos", attachments);
        }

        public ActionResult _TabReceitas(string idConsulta)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Recipes";

            ViewBag.idConsulta = idConsulta;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<RecipesResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return PartialView("_TabReceitas", details);
                }
            }

            var receitas = new List<RecipesResponse>();
            return PartialView("_TabReceitas", receitas);
        }

        public PartialViewResult _TabMCDT(string idConsulta)
        {
            ViewBag.idConsulta = idConsulta;
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Mcdt";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<McdtResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabMCDT", details);
                }
            }
            return PartialView("_TabMCDT");
        }

        public PartialViewResult _TabMedicacao(string idConsulta)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Medication";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<MedicationInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabMedicacao", details);
                }
            }
            return PartialView("_TabMedicacao");
        }

        public PartialViewResult _TabSoapHistorico(string idConsultaHistorico)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsultaHistorico}/Soap";

            ViewBag.idConsultaHistorico = idConsultaHistorico;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<SoapResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabSoapHistorico", details);
                }
                else
                {
                    var details = new SoapResponse() { Evaluation = "n/d", Objective = "n/d", Plan = "n/d", Subjective = "n/d", IdSoap = 0, Date = DateTime.Parse("01/01/1970") };
                    return PartialView("_TabSoapHistorico", details);
                }
            }
            return PartialView("_TabSoapHistorico");
        }

        public PartialViewResult _TabAnexosHistorico(string idConsultaHistorico)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsultaHistorico}/Attachments";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<AttachmentInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return PartialView("_TabAnexosHistorico", details);
                }
            }
            var attachments = new List<AttachmentInfoResponse>();
            return PartialView("_TabAnexosHistorico", attachments);
        }

        public ActionResult _TabReceitasHistorico(string idConsultaHistorico)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsultaHistorico}/Recipes";

            ViewBag.idConsultaHistorico = idConsultaHistorico;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<RecipesResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return PartialView("_TabReceitasHistorico", details);
                }
            }

            var receitas = new List<RecipesResponse>();
            return PartialView("_TabReceitasHistorico", receitas);
        }

        public PartialViewResult _TabReceitasProdutosHistorico(string idConsultaHistorico, string idRecipe)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsultaHistorico}/Recipes/{idRecipe}";

            ViewBag.idConsultaHistorico = idConsultaHistorico;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return PartialView("_TabReceitasProdutosHistorico", model);
                }
            }
            return PartialView("_TabReceitasProdutosHistorico");
        }

        public PartialViewResult _TabMCDTHistorico(string idConsultaHistorico)
        {
            ViewBag.idConsultaHistorico = idConsultaHistorico;
            var url = $"{_urlTargetBase}/api/Consultas/{idConsultaHistorico}/Mcdt";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<McdtResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabMCDTHistorico", details);
                }
            }
            return PartialView("_TabMCDTHistorico");
        }

        public PartialViewResult _TabMCDTProdutosHistorico(string idConsultaHistorico, int idMcdt)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsultaHistorico}/Mcdt/{idMcdt}";
            ViewBag.idConsultaHistorico = idConsultaHistorico;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabMCDTProdutosHistorico", model);
                }
            }
            return PartialView("_TabMCDTProdutosHistorico");
        }

        public PartialViewResult _TabMotivoHistorico(PatientHistoryInfoResponse item)
        {
            ViewBag.idConsultaHistorico = item.IdConsulta;
            var details = item;
            return PartialView("_TabMotivoHistorico", details);
        }

        #region TabParametros

        public PartialViewResult _TabParametros(string idConsulta)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Parameters";

            ViewBag.idConsulta = idConsulta;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<ParametersInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["parametersList"] = details;
                    return PartialView("_TabParametros", details);
                }
            }

            return PartialView("_TabParametros");
        }

        [HttpGet]
        public PartialViewResult parameterInfo(string idConsulta, int idParameter, string startDate, string endDate)
        {
            var details = GetDataParameter(idConsulta, idParameter, startDate, endDate);

            return PartialView("_InfoParameter", details);
        }

        private List<ParametersDetailsResponse> GetDataParameter(string idConsulta, int idParameter, string startDate, string endDate)
        {
            ViewBag.idConsulta = idConsulta;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            ViewBag.idParameter = idParameter;

            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/ParametersDetails/{idParameter}/From/{startDate}/To/{endDate}";

            if (Session["parametersList"] != null)
            {
                var p = Session["parametersList"] as List<ParametersInfoResponse>;

                if (p != null)
                {
                    ViewBag.ParameterName = p.SingleOrDefault(s => s.IdParameter == idParameter).ParameterName;
                }
            }

            switch ((string)ViewBag.ParameterName)
            {
                case "Colesterol"://Colesterol
                    ViewBag.LabelsDs = "\"Total\",\"Trigliceridos\",\"HDL\",\"LDL\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Colesterol Total", "Trigliceridos", "HDL", "LDL", "Notas" };
                    break;

                case "Glicemia"://Glicemia
                    ViewBag.LabelsDs = "\"Valor\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Valor", "Contexto", "Notas" };
                    break;

                case "Insulina"://Insulina
                    ViewBag.LabelsDs = "\"UI\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "UI", "Insulina", "Contexto", "Notas" };
                    break;

                case "Peso e Medidas"://Peso e Medidas
                    ViewBag.LabelsDs = "\"Peso\",\"IMC\",\"Peito\",\"Cintura\",\"Ancas\",\"Bra.esq\",\"Bra.dto\",\"P.esq\",\"P.dta\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Peso", "IMC", "Peito", "Cintura", "Ancas", "Bra esq", "Bra dto", "P.esq", "P.dta", "Notas" };
                    break;

                case "Tensao Arterial"://Tensao Arterial
                    ViewBag.LabelsDs = "\"Min\",\"Max\",\"Bpm\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Min", "Max", "Bpm", "Notas" };
                    break;

                default://Outro (Default)
                    ViewBag.LabelsDs = "\"Valor\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Valor", "Notas" };
                    break;
            }

            List<ParametersDetailsResponse> details = null;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    details = JsonConvert.DeserializeObject<List<ParametersDetailsResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    if (details.Count == 0) return details;

                    string cad = "";
                    string labelsCad = "";

                    var dataSets = new List<string>();

                    switch ((string)ViewBag.ParameterName)
                    {
                        case "Colesterol"://Colesterol
                            foreach (var item in details)
                            {
                                labelsCad += "\"" + item.MedicationDate.Value.ToShortDateString() + " " + item.MedicationDate.Value.ToShortTimeString() + "\",";

                                cad = item.ColesterolTotal.ToString().Replace(',', '.') + "," + item.ColesterolTrigliceridos.ToString().Replace(',', '.') + "," + item.ColesterolHdl.ToString().Replace(',', '.') + "," + item.ColesterolLdl.ToString().Replace(',', '.');
                                dataSets.Add(cad);
                            }
                            break;

                        case "Glicemia":
                            foreach (var item in details)
                            {
                                labelsCad += "\"" + item.MedicationDate.Value.ToShortDateString() + " " + item.MedicationDate.Value.ToShortTimeString() + "\",";
                                cad = item.GlicemiaValue.ToString().Replace(',', '.');
                                dataSets.Add(cad);
                            }
                            break;

                        case "Insulina":
                            foreach (var item in details)
                            {
                                labelsCad += "\"" + item.MedicationDate.Value.ToShortDateString() + " " + item.MedicationDate.Value.ToShortTimeString() + "\",";
                                cad = item.InsulinUI.ToString().Replace(',', '.');
                                dataSets.Add(cad);
                            }
                            break;

                        case "Peso e Medidas":
                            foreach (var item in details)
                            {
                                labelsCad += "\"" + item.MedicationDate.Value.ToShortDateString() + " " + item.MedicationDate.Value.ToShortTimeString() + "\",";
                                cad = item.WmWeight.ToString().Replace(',', '.') + "," + item.WmBmi.ToString().Replace(',', '.') + "," + item.WmChest.ToString().Replace(',', '.') + "," + item.WmWaist.ToString().Replace(',', '.') + "," + item.WmHips.ToString().Replace(',', '.') + "," + item.WmLeftArm.ToString().Replace(',', '.') + "," + item.WmRightArm.ToString().Replace(',', '.') + "," + item.WmLeftLeg.ToString().Replace(',', '.') + "," + item.WmRightLeg.ToString().Replace(',', '.');
                                dataSets.Add(cad);
                            }
                            break;

                        case "Tensao Arterial":
                            foreach (var item in details)
                            {
                                labelsCad += "\"" + item.MedicationDate.Value.ToShortDateString() + " " + item.MedicationDate.Value.ToShortTimeString() + "\",";
                                cad = item.BloodPressureMin.ToString().Replace(',', '.') + "," + item.BloodPressureMax.ToString().Replace(',', '.') + "," + item.BloodPressureHeartRate.ToString().Replace(',', '.');
                                dataSets.Add(cad);
                            }
                            break;

                        default:
                            foreach (var item in details)
                            {
                                labelsCad += "\"" + item.MedicationDate.Value.ToShortDateString() + " " + item.MedicationDate.Value.ToShortTimeString() + "\",";
                                cad = item.ValueDefault.ToString().Replace(',', '.');
                                dataSets.Add(cad);
                            }
                            break;
                    }
                    labelsCad = labelsCad.Substring(0, labelsCad.Length - 1);
                    ViewBag.LabelsGraph = labelsCad;
                    ViewBag.DataGraph = dataSets;
                }
            }

            SettingParamGraph();

            return details;
        }

        private string GetColor(string label)
        {
            var labels = new Dictionary<string, string>
            {
                { "Total","rgba(255, 99, 132, 0.4)"},
                { "Trigliceridos","rgba(54, 162, 235, 0.4)"},
                { "HDL","rgba(255, 206, 86, 0.4)"},
                { "LDL","rgba(75, 192, 192, 0.4)"},
                { "UI","rgba(153, 102, 255, 0.4)"},
                { "Insulina","rgba(255, 159, 64, 0.4)"},
                { "Peso","rgba(128,128,0,0.4)"},
                { "IMC","rgba(255,0,255,0.4)"},
                { "Peito","rgba(0,128,128,0.4)"},
                { "Cintura","rgba(128,0,0,0.4)"},
                { "Bra.esq","rgba(0,255,255,0.4)"},
                { "Bra.dto","rgba(0,0,128,0.4)"},
                { "P.esq","rgba(192,192,192,0.4)"},
                { "P.dta","rgba(128,128,128,0.4)"},
                { "Min","rgba(255,0,0,0.4)"},
                { "Max","rgba(0,255,0,0.4)"},
                { "Bpm","rgba(128,0,128,0.4)"},
                { "Valor","rgba(0,0,255,0.4)"}
            };

            var color = labels.SingleOrDefault(l => l.Key == label).Value;

            if (color == null)
            {
                return getRandomColor();
            }
            return color;
        }

        private string getRandomColor()
        {
            var letters = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

            var color = "#";
            for (var i = 0; i < 6; i++)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                //color += letters[(int)Math.Floor(random.NextDouble() * 16)];
                color += letters[random.Next(0, 15)];
            }
            return color;
        }

        private void SettingParamGraph()
        {
            var dataset = ViewBag.DataGraph as List<string>;
            int col = 0;

            string[] dataGraf = new string[dataset[0].Split(',').Length];
            string[] arrLabel = ((string)ViewBag.LabelsDs).Split(',');

            foreach (var ds in dataset)
            {
                for (int i = 0; i < ds.Split(',').Length; i++)
                {
                    dataGraf[i] += ds.Split(',')[i] + ",";
                }
            }

            for (int i = 0; i < dataGraf.Length; i++)
            {
                dataGraf[i] = dataGraf[i].Substring(0, dataGraf[i].Length - 1);
            }

            string cadJS = "";
            int j = 0;
            foreach (var g in dataGraf)
            {
                string label = arrLabel[j].Replace("\"", "");
                string color = GetColor(label);
                cadJS += "{";
                cadJS += "label: \"" + label + "\",";
                cadJS += "fill: false,";
                cadJS += "lineTension: 0.1,";
                cadJS += "backgroundColor:\"" + color + "\",";
                cadJS += "borderColor: \"" + color + "\",";
                cadJS += "borderCapStyle: \'butt\',";
                cadJS += "borderDash: [],";
                cadJS += "borderDashOffset: 0.0,";
                cadJS += "borderJoinStyle: \'miter\',";
                //cadJS += "pointBorderColor: \"rgba(75,192,192,1)\",";
                cadJS += "pointBackgroundColor: \"#fff\",";
                cadJS += "pointBorderWidth: 1,";
                cadJS += "pointHoverRadius: 5,";
                //cadJS += "pointHoverBackgroundColor: \"rgba(75,192,192,1)\",";
                //cadJS += "pointHoverBorderColor: \"rgba(220,220,220,1)\",";
                cadJS += "pointHoverBorderWidth: 2,";
                cadJS += "pointRadius: 1,";
                cadJS += "pointHitRadius: 10,";
                cadJS += "type: \'line\',";
                cadJS += "data: [" + g + "],";
                cadJS += "spanGaps: false,";
                cadJS += "},";
                j++;
            }
            cadJS = cadJS.Substring(0, cadJS.Length - 1);
            ViewBag.cadJS = cadJS;
        }

        public PartialViewResult _ModalViewParametroInfo(string idConsulta, int idParameter, string startDate, string endDate)
        {
            var details = GetDataParameter(idConsulta, idParameter, startDate, endDate);

            return PartialView("_ModalParametroInfo", details);
        }

        public PartialViewResult _ModalParametroColesterolInfo(string idPatient)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idPatient}/ParametersColesterol ";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<ParameterColesterolResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_ModalParametroColesterolInfo", details);
                }
            }

            return null;
        }

        #endregion TabParametros

        public PartialViewResult _TabHistorico(string idConsulta)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/PatientHistory";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<PatientHistoryInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabHistorico", details);
                }
            }
            return PartialView("_TabHistorico");
        }

        #region Modals

        public PartialViewResult _ModalAddReceita(string idConsulta)
        {
            ViewBag.ApiUrl = _urlTargetBase;
            ViewBag.idConsulta = idConsulta;
            Session["ReceitaProducts"] = null;
            return PartialView("_ModalReceita", new RecipesResponse());
        }

        public PartialViewResult _ModalViewHistorico(PatientHistoryInfoResponse item)
        {
            ViewBag.idConsultaHistorico = item.IdConsulta;
            //TODO: check the model to pass here
            return PartialView("_ModalHistorico", item);
        }

        public PartialViewResult _ModalReceitaTable(bool isReadonly, bool isAddingNewRow = false)
        {
            ViewBag.isReadonly = isReadonly;
            if (Session["ReceitaProducts"] == null)
                Session["ReceitaProducts"] = new List<RecipeProductViewModel>();

            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];
            ViewBag.AddingNewRow = isAddingNewRow;
            return PartialView("_ModalReceitaTable", model);
        }

        [HttpPost]
        public ActionResult _ModalReceitaTableSet(bool isReadonly, List<RecipeProductViewModel> model = null)
        {
            string buttonValue = Request["submitButton"];
            bool _isAddingNewRow = false;
            if (buttonValue == "add")
            {
                _isAddingNewRow = true;
            }
            else if (buttonValue == "check")
            {
                _isAddingNewRow = false;
            }
            Session["ReceitaProducts"] = model;
            return RedirectToAction("_ModalReceitaTable", new { isReadonly = false, isAddingNewRow = _isAddingNewRow });
        }

        public ActionResult _ModalReceitaTableCancelAddRow()
        {
            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];
            model.Remove(model.Last());
            ViewBag.isReadonly = false;
            ViewBag.AddingNewRow = false;
            return PartialView("_ModalReceitaTable", model);
        }

        public ActionResult _ModalReceitaTableDeleteRow(bool isReadonly, int index)
        {
            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];
            model.RemoveAt(index);
            Session["ReceitaProducts"] = model;
            ViewBag.isReadonly = false;
            ViewBag.AddingNewRow = false;
            return RedirectToAction("_ModalReceitaTable", new { isReadonly });
        }

        public ActionResult _ModalReceitaTableEditRow(int _index, bool _isAddingNewRow)
        {
            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];
            if (_isAddingNewRow)
            {
                model.Remove(model.Last());
            }
            ViewBag.isReadonly = true;
            ViewBag.Index = _index;
            ViewBag.isReadonly = false;
            ViewBag.AddingNewRow = false;
            return PartialView("_ModalReceitaTable", model);
        }

        public PartialViewResult _ModalAddMcdt(string idConsulta)
        {
            ViewBag.ApiUrl = _urlTargetBase;
            ViewBag.idConsulta = idConsulta;
            Session["McdtExame"] = null;
            return PartialView("_ModalMCDT", new McdtResponse());
        }

        public PartialViewResult _ModalMcdtTable(bool isReadonly, bool isAddingNewRow = false)
        {
            ViewBag.isReadonly = isReadonly;

            if (Session["McdtExame"] == null)
                Session["McdtExame"] = new List<McdtExamsResponse>();

            var model = (List<McdtExamsResponse>)Session["McdtExame"];
            ViewBag.AddingNewRow = isAddingNewRow;
            return PartialView("_ModalMcdtTable", model);
        }

        [HttpPost]
        public ActionResult _ModalMcdtTableSet(bool isReadonly, List<McdtExamsResponse> model = null)
        {
            string buttonValue = Request["submitButton"];
            bool _isAddingNewRow = false;
            if (buttonValue == "add")
            {
                _isAddingNewRow = true;
            }
            else if (buttonValue == "check")
            {
                _isAddingNewRow = false;
            }
            Session["McdtExame"] = model;
            return RedirectToAction("_ModalMcdtTable", new { isReadonly = false, isAddingNewRow = _isAddingNewRow });
        }

        public ActionResult _ModalMcdtTableCancelAddRow()
        {
            var model = (List<McdtExamsResponse>)Session["McdtExame"];
            model.Remove(model.Last());
            ViewBag.isReadonly = false;
            ViewBag.AddingNewRow = false;
            return PartialView("_ModalMcdtTable", model);
        }

        public ActionResult _ModalMcdtTableDeleteRow(bool isReadonly, int index)
        {
            var model = (List<McdtExamsResponse>)Session["McdtExame"];
            model.RemoveAt(index);
            Session["McdtExame"] = model;
            //Session["McdtExamsCount"] = (model == null) ? 0 : model.Count();
            string a = "a";
            return RedirectToAction("_ModalMcdtTable", new { isReadonly });
        }

        public ActionResult _ModalMcdtTableEditRow(int _index, bool _isAddingNewRow)
        {
            var model = (List<McdtExamsResponse>)Session["McdtExame"];
            if (_isAddingNewRow)
            {
                model.Remove(model.Last());
            }
            ViewBag.Index = _index;
            ViewBag.isReadonly = false;
            ViewBag.AddingNewRow = false;
            return PartialView("_ModalMcdtTable", model);
        }

        public PartialViewResult _ModalEditReceita(string idConsulta, string idReceita)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Recipes/{idReceita}";
            ViewBag.ApiUrl = _urlTargetBase;
            ViewBag.idConsulta = idConsulta;
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["ReceitaProducts"] = RecipeProductViewModel.ParseList(model.RecipeProducts);

                    return PartialView("_ModalReceita", model);
                }
            }
            return PartialView("_ModalReceita");
        }

        public PartialViewResult _ModalEditMcdt(string idConsulta, int idMcdt)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Mcdt/{idMcdt}";
            ViewBag.ApiUrl = _urlTargetBase;
            ViewBag.idConsulta = idConsulta;
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["McdtExame"] = model.McdtExams;
                    return PartialView("_ModalMCDT", model);
                }
            }
            return PartialView("_ModalMCDT");
        }

        public PartialViewResult _ModalViewReceita(string idConsulta, string idReceita)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Recipes/{idReceita}";
            ViewBag.ApiUrl = _urlTargetBase;
            ViewBag.idConsulta = idConsulta;
            ViewBag.ReadOnly = true;
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["ReceitaProducts"] = RecipeProductViewModel.ParseList(model.RecipeProducts);

                    return PartialView("_ModalReceita", model);
                }
            }
            return PartialView("_ModalReceita");
        }

        public PartialViewResult _ModalViewMcdt(string idConsulta, string idMcdt)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Mcdt/{idMcdt}";
            ViewBag.ApiUrl = _urlTargetBase;
            ViewBag.idConsulta = idConsulta;
            ViewBag.ReadOnly = true;
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["McdtExame"] = model.McdtExams;
                    return PartialView("_ModalMCDT", model);
                }
            }
            return PartialView("_ModalMCDT");
        }

        #endregion Modals

        #region Receitas

        /// <summary>
        /// Download recipe file. Trello 1.8.3
        /// </summary>
        /// <param name="idConsulta"></param>
        /// <param name="idRecipes"></param>
        /// <returns></returns>
        public FileResult DownloadRecipe(string idConsulta, string idRecipes)
        {
            string contentType = "application/pdf";// "application/force-download";
            var urlFileInfo = $"{_urlTargetBase}/api/Consultas/{idConsulta}/RecipesFile/{idRecipes}";
            RecipesResponse RecipesResponse = new RecipesResponse();
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(urlFileInfo).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    RecipesResponse = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
            }
            return File(RecipesResponse.RecipeImage, contentType, RecipesResponse.RecipeImageName);
        }

        public async Task<ActionResult> AddEditReceita(RecipesResponse model, string idConsulta)
        {
            try
            {
                string RecipeImageIdStr = Request["RecipeImageId"].ToString();
                Guid? RecipeImageId;
                if (RecipeImageIdStr == string.Empty)
                {
                    RecipeImageId = null;
                }
                else
                {
                    try
                    {
                        string newfileid = RecipeImageIdStr;
                        newfileid = newfileid.ToString().Replace('"', ' ').Trim();
                        newfileid = newfileid.Replace(@"\", string.Empty);
                        Guid helperfileid = new Guid(newfileid);
                        RecipeImageId = helperfileid;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                var urlAdd = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Recipes/New";
                var urlEdit = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Recipes/{model.IdRecipes}";
                var products = RecipeProductViewModel.GetBaseList((List<RecipeProductViewModel>)Session["ReceitaProducts"]);
                model.RecipeImageId = RecipeImageId;
                model.RecipeProducts = products;
                var serializereceita = new JavaScriptSerializer().Serialize(model);
                int IdRecipes = 0;
                try
                {
                    using (var client = new HttpClient())
                    {
                        HttpResponseMessage wcfResponse;

                        if (model.IdRecipes > 0)
                            wcfResponse = await client.PutAsync(urlEdit, new StringContent(serializereceita, Encoding.UTF8, "application/json"));
                        else
                            wcfResponse = await client.PostAsync(urlAdd, new StringContent(serializereceita, Encoding.UTF8, "application/json"));

                        if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                        {
                            var json = wcfResponse.Content.ReadAsStringAsync().Result;

                            RecipesResponse reciperesp = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                            IdRecipes = reciperesp.IdRecipes;
                            //Success Response
                        }
                    }
                    return RedirectToAction("_TabReceitas", new { idConsulta });
                }
                catch (Exception ex)
                {
                    return RedirectToAction("_TabReceitas", new { idConsulta });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActionResult> DeleteReceita(string idConsulta, string idReceita)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Recipes/{idReceita}";

            using (var client = new HttpClient())
            {
                HttpResponseMessage wcfResponse = await client.DeleteAsync(url);
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("_TabReceitas", new { idConsulta });
                }
            }
            return RedirectToAction("_TabReceitas", new { idConsulta });
        }

        /// <summary>
        /// Download mcdt file. Trello 1.8.4
        /// </summary>
        /// <param name="idConsulta"></param>
        /// <param name="idMcdt"></param>
        /// <returns></returns>
        public FileResult DownloadMcdt(string idConsulta, string idMcdt)
        {
            string contentType = "application/pdf";//"application/force-download";
            var urlFileInfo = $"{_urlTargetBase}/api/Consultas/{idConsulta}/McdtFile/{idMcdt}";
            McdtResponse mcdtResponse = new McdtResponse();
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(urlFileInfo).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    mcdtResponse = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
            }
            return File(mcdtResponse.McdtImage, contentType, mcdtResponse.McdtImageName);
        }

        public IEnumerable<byte[]> SplitByteArray(byte[] value, int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
                countOfArray++;
            for (int i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();
            }
        }

        public async Task<ActionResult> AddEditMcdt(McdtResponse model, string idConsulta)
        {
            try
            {
                string McdtImageIdStr = Request["McdtImageId"].ToString();
                Guid? McdtImageId;
                if (McdtImageIdStr == string.Empty)
                {
                    McdtImageId = null;
                }
                else
                {
                    try
                    {
                        string newfileid = McdtImageIdStr;
                        newfileid = newfileid.ToString().Replace('"', ' ').Trim();
                        newfileid = newfileid.Replace(@"\", string.Empty);
                        Guid helperfileid = new Guid(newfileid);
                        McdtImageId = helperfileid;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                string urlAdd = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Mcdt/New";
                string urlEdit = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Mcdt/{model.IdMcdt}";
                model.McdtExams = (List<McdtExamsResponse>)Session["McdtExame"];
                model.McdtImageId = McdtImageId;
                var serializemcdt = new JavaScriptSerializer().Serialize(model);
                int idMcdt = 0;
                using (var client = new HttpClient())
                {
                    //McdtResponse
                    HttpResponseMessage wcfResponse;
                    if (model.IdMcdt > 0)
                        wcfResponse = await client.PutAsync(urlEdit, new StringContent(serializemcdt, Encoding.UTF8, "application/json"));
                    else
                        wcfResponse = await client.PostAsync(urlAdd, new StringContent(serializemcdt, Encoding.UTF8, "application/json"));

                    if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                    {
                        var json = wcfResponse.Content.ReadAsStringAsync().Result;
                        McdtResponse mcdtresp = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        idMcdt = mcdtresp.IdMcdt;
                        //Success Response
                    }
                }
                return RedirectToAction("_TabMCDT", new { idConsulta });//with Errors
            }
            catch (Exception e)
            {
                return RedirectToAction("_TabMCDT", new { idConsulta });//with Errors
            }
        }

        public async Task<ActionResult> DeleteMcdt(string idConsulta, string idMcdt)
        {
            var url = $"{_urlTargetBase}/api/Consultas/{idConsulta}/Mcdt/{idMcdt}";

            using (var client = new HttpClient())
            {
                HttpResponseMessage wcfResponse = await client.DeleteAsync(url);
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("_TabMCDT", new { idConsulta });
                }
            }
            return RedirectToAction("_TabMCDT", new { idConsulta });
        }

        #endregion Receitas

        public async Task<JsonResult> SearchProducts(string code, string name, int? skip, int? take)
        {
            var url = $"{_urlTargetBase}/api/Consultas/Recipes/Products";
            var input = new JavaScriptSerializer().Serialize(new
            {
                code,
                name,
                skip = skip ?? 0,
                take = take ?? 0
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(input, Encoding.UTF8, "application/json"));
                using (wcfResponse)
                {
                    if (!wcfResponse.IsSuccessStatusCode)
                        return Json(new { });

                    var json = await wcfResponse.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<List<ProductsResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return Json(model.Select(x => $"<input type='hidden' value='{x.Code}' />{x.MedicationName}"), JsonRequestBehavior.AllowGet);
                }
            }
        }

        public async Task<JsonResult> SearchMcdts(string code, string name)
        {
            var url = $"{_urlTargetBase}/api/Consultas/Mcdt/Support";
            var SnsCode = code;
            var ConvCode = code;
            var Description = name;

            var input = new JavaScriptSerializer().Serialize(new
            {
                SnsCode,
                ConvCode,
                Description
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(input, Encoding.UTF8, "application/json"));
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<List<IFarmacias.Models.Response.McdtSupportResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return Json(model.Select(x => $"<input type='hidden' value='{x.SnsCode}' />{x.Description}"), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { });
        }

        public async Task<JsonResult> FixChatMember(string Identity, string RoomName)
        {
            var url = $"{_urlTargetBase}/api/Twilio/FixChatChannelMembers";

            var input = new JavaScriptSerializer().Serialize(new
            {
                Identity,
                RoomName
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(input, Encoding.UTF8, "application/json"));
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<TwilioFixChatMembersResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return Json(model);
                }
            }

            return null;
        }

        /// <summary>
        /// GetMessagesNotifications
        /// </summary>
        /// <returns></returns>
        public PartialViewResult GetMessagesNotifications()
        {
            //this don't make sense the library is on the client side, why we are fetching info from the web api?
            try
            {
                var url = $"{_urlTargetBase}/api/Twilio/GetUserChannels";
                using (var client = new HttpClient())
                {
                    var objectJson = new JavaScriptSerializer().Serialize(
                        new IFarmacias.Models.Request.TwilioGetUserChannelsRequest
                        {
                            UserIdentity = "dr" + ((CustomPrincipal)HttpContext.User).IdDoctor.ToString(),
                            Limit = 10000,
                            PageSize = 10000,
                            //não percebo o que serve este parametro ListofConsultationStatus
                            ListofConsultationStatus = new List<ConsultationStatus>() { ConsultationStatus.InProgress, ConsultationStatus.Open, ConsultationStatus.Invoiced, ConsultationStatus.Closed, ConsultationStatus.KeepConsultation, ConsultationStatus.KeepInvoiced }
                            //TODO: Paging
                        }
                    );
                    var wcfResponse = client.PostAsync(url, new StringContent(objectJson, Encoding.UTF8, "application/json")).Result;
                    if (wcfResponse.IsSuccessStatusCode)
                    {
                        var json = wcfResponse.Content.ReadAsStringAsync().Result;
                        var details = JsonConvert.DeserializeObject<List<TwilioUserChannelsResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        return PartialView("_PartialMessageNotifications", details);
                    }
                }
            }
            catch (Exception ex)
            {
                return PartialView("_PartialMessageNotifications", null);
            }
            return PartialView("_PartialMessageNotifications", null);
        }

        public ActionResult PutLastChatMessageDateTime(string CHANNEL_ID)
        {
            try
            {
                string url = $"{_urlTargetBase}/api/Twilio/PutLastChatMessageDateTime";
                string ID_DOCTOR = CHANNEL_ID.Substring(2, 1);
                int id_doctor; int.TryParse(ID_DOCTOR, out id_doctor);
                using (var client = new HttpClient())
                {
                    var objectJson = new JavaScriptSerializer().Serialize(
                        new PutLastChatMessageDateTimeRequest
                        {
                            ID_DOCTOR = id_doctor,
                            CHANNEL_ID = CHANNEL_ID,
                        }
                    );
                    var wcfResponse = client.PutAsync(url, new StringContent(objectJson, Encoding.UTF8, "application/json"));
                    if (wcfResponse.Result.StatusCode == HttpStatusCode.OK)
                    {
                        return new EmptyResult();
                    }
                }
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// Retrieve Twilio Channel list
        /// </summary>
        /// <returns></returns>
        public async Task<List<TwilioUserChannelsResponse>> GetMessagesNotificationsResponseAsync()
        {
            try
            {
                var url = $"{_urlTargetBase}/api/Twilio/GetUserChannels";
                using (var client = new HttpClient())
                {
                    var objectJson = new JavaScriptSerializer().Serialize(
                        new IFarmacias.Models.Request.TwilioGetUserChannelsRequest
                        {
                            UserIdentity = "dr" + ((CustomPrincipal)HttpContext.User).IdDoctor.ToString(),
                            Limit = 10000,
                            PageSize = 10000
                            //TODO: Paging
                        }
                    );
                    var response = await client.PostAsync(url, new StringContent(objectJson, Encoding.UTF8, "application/json"));
                    using (response)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonChannels = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<List<TwilioUserChannelsResponse>>(jsonChannels);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
            }
            return new List<TwilioUserChannelsResponse>();
        }

        /// <summary>
        /// Process the Channel FriendlyName and return the valid name
        /// </summary>
        /// <param name="unprocessedChannelFriendlyName"></param>
        /// <returns></returns>
        private static string GetChannelFriendlyName(string unprocessedChannelFriendlyName)
        {
            if (string.IsNullOrEmpty(unprocessedChannelFriendlyName))
                return string.Empty;

            var array = unprocessedChannelFriendlyName.Split(';');
            if (array.Length > 0)
                return array.Last();

            return string.Empty;
        }

        /// <summary>
        /// Perform HTTP GET async calls to API
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<ConsultasResponse>> PerformGetConsultasAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var wcfResponse = await client.GetAsync(url);
                    using (wcfResponse)
                    {
                        if (!wcfResponse.IsSuccessStatusCode)
                            return null;

                        var jsonConsultas = await wcfResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<ConsultasResponse>>(jsonConsultas);
                    }
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ActionResult> PatientHistoricalRecord(int? take)
        {
            if (take == null || take == 0)
                take = -30;

            var idDoctor = ((CustomPrincipal)HttpContext.User).IdDoctor;
            var inProgressUrl =
               $"{_urlTargetBase}/api/Consultas/{idDoctor}/3/consultsByStatus";

            var inProgressResultList = new List<ConsultasResponse>();
            var result = await PerformGetConsultasAsync(inProgressUrl);
            if (result != null)
            {
                result = result.OrderByDescending(orderedList => orderedList.Date);
                var deltaDay = DateTime.Today.AddDays(take.Value);
                foreach (ConsultasResponse lista in result)
                {
                    if (lista.Date > deltaDay)
                    {
                        inProgressResultList.Add(lista);
                    }
                }
            }
            var openUrl = $"{_urlTargetBase}/api/consultas/open?skip=0&take={take}";

            return View(new ConsultaDashBoardViewModel
            {
                InProgressList = inProgressResultList,
                OpenList = null,
                Take = take.Value,
                Skip = 0
            });
        }

        /// <summary>
        /// Get session time out parameters
        /// </summary>
        /// <returns></returns>
        private SessionTimeOutConfig GetTimeOutConfigValues()
        {
            SessionTimeOutConfig timeOutConfig = new SessionTimeOutConfig();
            timeOutConfig.MillisecWarning = Int32.Parse(millisecWarning);
            timeOutConfig.MillisecRedir = Int32.Parse(millisecRedir);
            timeOutConfig.Title = titleSession;
            timeOutConfig.Message = messageSession;
            timeOutConfig.KeepAliveButton = keepAliveButton;
            timeOutConfig.LogoutButton = logoutButton;
            timeOutConfig.ActionName = actionName;
            timeOutConfig.ControllerName = controllerName;
            timeOutConfig.KeepAlive = keepAlive;
            timeOutConfig.IgnoreUserActivity = ignoreUserActivity;
            timeOutConfig.CountdownBar = countdownBar;
            timeOutConfig.CountdownMessage = countdownMessage;
            timeOutConfig.CountdownSmart = countdownSmart;

            return timeOutConfig;
        }
    }
}