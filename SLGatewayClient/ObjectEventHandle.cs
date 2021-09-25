using SLGatewayCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public class ObjectEventHandle
    {
        public ObjectEventCode Code { get; set; }
        public int Handle { get; set; }
    }
}
