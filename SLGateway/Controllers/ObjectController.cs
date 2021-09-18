using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGateway.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGateway.Controllers
{
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.BearerAuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectRegistrationService _ors;
        private readonly ILogger _logger;

        public ObjectController(ILogger<ObjectController> logger, IObjectRegistrationService ors)
        {
            _logger = logger;
            _ors = ors;
        }

        [Route("register/{id}")]
        [HttpPost]
        public IActionResult Register(Guid id, ObjectRegistration reg)
        {
            if (reg == null || string.IsNullOrWhiteSpace(reg.Token) || string.IsNullOrWhiteSpace(reg.Url))
            {
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

        [Route("register/{id}")]
        [HttpDelete]
        public IActionResult Deregister(Guid id)
        {
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
