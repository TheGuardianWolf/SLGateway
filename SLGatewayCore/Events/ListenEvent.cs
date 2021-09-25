using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayCore.Events
{
    public class ListenEvent
    {
        public int Channel { get; set; }
        public string Name { get; set; } = "";
        public Guid Id { get; set; }
        public string Message { get; set; } = "";
    }
}
