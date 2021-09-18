using AspNetCore.Authentication.ApiKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace SLGateway.Data
{
    public static class ApiKeyAuthenticationPolicy
    {
        public const string Object = "ApiKeyObject";
        public const string Client = "ApiKeyClient";
    }

    public static class ApiKeyAuthenticationDefaults
    {
        public const string BearerAuthenticationScheme = "Bearer";
    }

    public static class ApiKeyScopes
    {
        public const string Object = "slgateway/object:all";
        public const string Client = "slgateway/client:all";
    }

    public class ApiKey : IApiKey
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        [JsonIgnore]
        public string OwnerName => UserId; // Aliased for interface
        public string UserId { get; set; }
        [JsonIgnore]
        public IReadOnlyCollection<Claim> Claims => Scopes?.Select(s => new Claim(s, bool.TrueString))?.ToList() ?? new List<Claim>();
        public IEnumerable<string> Scopes { get; set; } = new List<string>();
    }
}
