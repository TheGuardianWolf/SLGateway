using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SLGateway.Data
{
	public static class ClaimsExtensions
	{
		public static string GetValue(this IEnumerable<Claim> claims, string claimType)
		{
			return claims.FirstOrDefault(x => x.Type == claimType)?.Value;
		}
	}
}
