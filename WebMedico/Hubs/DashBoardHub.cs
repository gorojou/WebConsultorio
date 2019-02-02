using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WebMedico.Hubs
{
    public class DashBoardHub : Hub
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Send()
        {
            Clients.All.refreshTable();
        }
    }
}