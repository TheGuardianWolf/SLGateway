using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient.Data
{
    public class AgentWorldProfile
    {
        public string Nickname { get; set; } = "";
        public string LegacyName { get; set; } = "";
        public byte[]? ProfileImage { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; } = "";
        public Uri Link { get; set; } = new Uri("http://world.secondlife.com");
    }
}
