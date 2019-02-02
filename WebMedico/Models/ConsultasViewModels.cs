using System.Collections.Generic;
using IFarmacias.Models;
using System.ComponentModel;

namespace WebMedico.Models
{
    public class BasicInfoViewModel : PatientBasicInfoResponse
    {
        public string IdConsult { get; set; }

        public SessionTimeOutConfig SessionConfig { get; set; }

        [DisplayName("Deseja enviar alguma informação/recomendação para a consulta de retorno?")]
        public string Note { get; set; }
    }

    public sealed class ConsultaDashBoardViewModel
    {
        public int Skip { get; set; }

        public int Take { get; set; }

        public SessionTimeOutConfig SessionConfig { get; set; }

        public ICollection<ConsultasResponse> InProgressList { get; set; }

        public ICollection<ConsultasResponse> OpenList { get; set; }
    }
    public class ConfigureDailyCleaningMedicalConsultationViewModel
    {
        public int TimeofValidityinHours { get; set; }
    }

    public class SessionTimeOutConfig
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string KeepAliveButton { get; set; }
        public string KeepAlive { get; set; }
        public string IgnoreUserActivity { get; set; }
        public string LogoutButton { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string CountdownBar { get; set; }
        public string CountdownMessage { get; set; }
        public string CountdownSmart { get; set; }
        public int MillisecWarning { get; set; }
        public int MillisecRedir { get; set; }
    }

}