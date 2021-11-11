using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using SLGatewayClient.Data;
using SLGatewayCore.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SLGatewayClient
{
    /// <summary>
    /// This provides a way to access the World API provided by LL, it is not related to the gateway
    /// http://wiki.secondlife.com/wiki/World_API
    /// </summary>
    public class SLWorld
    {
        private static HttpClient _httpClient = StaticHttpClient.GetClient();

        private readonly ILogger? _logger;

        public SLWorld(ILogger? logger = null)
        {
            _logger = logger;
        }

        public async Task<AgentWorldProfile?> GetAgentProfile(Guid agentKey)
        {
            var uri = new Uri($"https://world.secondlife.com/resident/{agentKey}");
            _logger?.LogTrace("Getting agent profile from uri: {uri}", uri);

            var response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Agent profile cannot be accessed: ({statusCode}) {uri}", response.StatusCode, uri);
                return null;
            }

            var document = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(document);

            var profile = new AgentWorldProfile
            {
                Link = uri
            };

            var name = htmlDoc.DocumentNode.SelectSingleNode("//head/title")?.InnerText ?? "";
            _logger?.LogTrace("Name node found as {name}", name);

            if (!string.IsNullOrEmpty(name))
            {
                var nameRegexp = new Regex(@"^(.*?) \((.*?)\)$");

                var nameMatch = nameRegexp.Match(name);

                if (nameMatch.Success && nameMatch.Groups.Count > 2)
                {
                    profile.Nickname = nameMatch.Groups[1]?.Value ?? "";
                    profile.LegacyName = nameMatch.Groups[2]?.Value ?? "";

                    _logger?.LogTrace("Nickname: {nickname}, LegacyName: {legacyName}", profile.Nickname, profile.LegacyName);
                }
            }

            var description = htmlDoc.DocumentNode.SelectSingleNode(@"//head/meta[@name=""description""]")?.Attributes["content"]?.Value ?? "";
            profile.Description = HttpUtility.HtmlDecode(description);
            _logger?.LogTrace("Description: {description}", description);

            var imgSrc = htmlDoc.QuerySelector("img.parcelimg")?.Attributes["src"]?.Value; // Bigger image at slash 2

            if (!string.IsNullOrEmpty(imgSrc))
            {
                imgSrc = $"{imgSrc?.Substring(0, imgSrc.Length - 1)}2";
                var imgResponse = await _httpClient.GetAsync(imgSrc);

                if (imgResponse.IsSuccessStatusCode)
                {
                    _logger?.LogTrace("Got image successfully");
                    var imageBytes = await imgResponse.Content.ReadAsByteArrayAsync();
                    if (ImageHelper.IsValidImage(imageBytes))
                    {
                        profile.ProfileImage = imageBytes;
                    }
                    else
                    {
                        _logger?.LogTrace("Failed to validate image");
                    }
                }
                else
                {
                    _logger?.LogTrace("Failed to get image: {statusCode}", imgResponse.StatusCode);
                }
            }

            var residentSince = HttpUtility.HtmlDecode(htmlDoc.QuerySelector("p.info")?.InnerText ?? "");
            if (!string.IsNullOrEmpty(residentSince))
            {
                var dateRegexp = new Regex(@"\d\d\d\d-\d\d-\d\d");
                var dateMatch = dateRegexp.Match(residentSince);
                if (dateMatch.Success && dateMatch.Groups.Count > 0)
                {
                    var success = DateTime.TryParse(dateMatch.Groups[0].Value, out var date);
                    if (success)
                    {
                        profile.CreationDate = date;
                    }

                    _logger?.LogTrace("Creation date parsed as: {creationDate}", profile.CreationDate);
                }
            }

            return profile;
        }
    }
}
