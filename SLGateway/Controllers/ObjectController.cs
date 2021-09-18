using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGateway.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectRegistrationService _ors;
        private readonly ILogger _logger;
        private readonly string _allowedUrlHostname;

        public ObjectController(IConfiguration configuration, ILogger<ObjectController> logger, IObjectRegistrationService ors)
        {
            _logger = logger;
            _ors = ors;
            _allowedUrlHostname = configuration.GetValue<string>("SecondLifeHost");
        }

        [Authorize(ApiKeyAuthenticationPolicy.Object)]
        [Route("register/{id}")]
        [HttpPost]
        public IActionResult Register(Guid id, ObjectRegistration reg)
        {
            if (reg == null || string.IsNullOrWhiteSpace(reg.Token) || string.IsNullOrWhiteSpace(reg.Url))
            {
                return BadRequest();
            }

            var url = new Uri(reg.Url);
            if (!url.Host.EndsWith(_allowedUrlHostname))
            {
                // LL say it is not recommended to check hostname, but I would rather not risk spamming another url
                _logger.LogDebug("Registered url for {objectId} does end with required hostname {host}", id, _allowedUrlHostname);
                return BadRequest();
            }

            if (!_ors.Register(new ObjectRegistration
            {
                Id = id,
                Url = reg.Url,
                Token = reg.Token
            }))
            {
                _logger.LogWarning("Object {id} could not be registered", id);
                return BadRequest();
            }

            _logger.LogTrace("Registered object {id} with {url} and {token}", id, reg.Url, reg.Token);

            return Ok();
        }

        [Authorize(ApiKeyAuthenticationPolicy.Object)]
        [Route("register/{id}")]
        [HttpDelete]
        public IActionResult Deregister(Guid id)
        {
            // Check object ownership
            if (_ors.GetObject(id)?.ApiKey != this.GetApiKey())
            {
                return Forbid();
            }

            if (!_ors.Deregister(id))
            {
                _logger.LogWarning("Object {id} could not be deregistered", id);
                return BadRequest();
            }

            _logger.LogTrace("Deregistered object {id}", id);

            return Ok();
        }
    }
}
