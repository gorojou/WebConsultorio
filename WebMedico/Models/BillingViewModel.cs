using System;
using System.Collections.Generic;
using System.Security.Principal;
using IFarmacias.Models.Response;

namespace WebMedico.Models
{
    public class BillingViewModel   {
        public int CodMedico { get; set; }
        public ICollection<ReadAllPaymentsResponse> BillingList { get; set; }
    }
}