using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Data
{
    public class ObjectRegistration
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public string ApiKey { get; set; }
    }
}
