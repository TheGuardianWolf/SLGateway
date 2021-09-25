using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayCore.Events
{
    public class DataserverEvent
    {
        public Guid QueryId { get; set; }
        public string Data { get; set; }
    }
}
