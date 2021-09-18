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

    public static class ApiRoles
    {
        public const string Object = "object";
        public const string Client = "client";
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
