using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayCore.Events
{
    public class EventResponse
    {
        public int HttpStatusCode { get; set; }
        public bool IsSuccessStatusCode
        {
            get { return HttpStatusCode >= 200 && HttpStatusCode <= 299; }
        }
    }

    public class EventResponse<T> : EventResponse
    {
        public T? Data { get; set; }
    }
}
