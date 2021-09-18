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
        public const string Object = "Object";
    }

    public class ApiKey : IApiKey
    {
        public string Key { get; }
        public string Name { get; }
        public DateTime CreatedUtc { get; }

        public string OwnerName { get; }

        public IReadOnlyCollection<Claim> Claims { get; }
    }
}
