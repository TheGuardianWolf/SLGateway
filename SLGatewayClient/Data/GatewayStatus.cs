using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient.Data
{
	[Flags]
	public enum GatewayStatus
	{
		Available = 0b1,
		InvalidApiKey = 0b10,
		Unavailable = 0b100,
	}
}
