using SLGatewayCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public class DelegatedObjectEvent : ObjectEvent
    {
        public Guid ObjectId { get; set; }
    }
}
