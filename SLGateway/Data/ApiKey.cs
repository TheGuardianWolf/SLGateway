using AspNetCore.Authentication.ApiKey;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SLGateway.Data
{
    public static class ApiKeyAuthenticationDefaults
    {
        public const string BearerAuthenticationScheme = "Bearer";
    }

    public static class ApiKeyClaims
    {
        public const string Object = "slgateway/object:all";
        public const string Client = "slgateway/client:all";
    }

    public class ApiKey : IApiKey
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string OwnerName { get; set; }
        public IReadOnlyCollection<Claim> Claims { get; set; }
    }
}
