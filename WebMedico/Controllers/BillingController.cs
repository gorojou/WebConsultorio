using IFarmacias.Models.Response;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMedico.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace WebMedico.Controllers
{
    [System.Web.Mvc.Authorize]
    public class BillingController : Controller
    {
        /// <summary>
        /// API Base Uri
        /// </summary>
        private readonly string _urlTargetBase = ConfigurationManager.AppSettings["ApiUrl"].ToString();

        // GET: Billing
        public async Task<ActionResult> BillingView()
        {
  
            var doctorId = ((CustomPrincipal)HttpContext.User).IdDoctor;
            var serviceUrl = $"{_urlTargetBase}/api/ReadAllPaymentsByDoctor/{doctorId}"; 
           
            var billingResultList = new List<ReadAllPaymentsResponse>();

            var result = await PerformGetConsultasAsync(serviceUrl);
            
            if (result != null)
                billingResultList.AddRange(result);


            return View(new BillingViewModel
            {
                CodMedico = doctorId,
                BillingList = billingResultList
            });
            //return View();
        }

        private static async Task<IEnumerable<ReadAllPaymentsResponse>> PerformGetConsultasAsync(string url)
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
                        return JsonConvert.DeserializeObject<List<ReadAllPaymentsResponse>>(jsonConsultas);
                    }
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }
    }
}