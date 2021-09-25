using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SLGateway.Data;
using SLGateway.Services;
using SLGatewayCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SLGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IEventsService _eventsService;
        private readonly IObjectRegistrationService _objectRegistrationService;

        public EventsController(ILogger<EventsController> logger, IEventsService eventsService, IObjectRegistrationService objectRegistrationService)
        {
            _logger = logger;
            _eventsService = eventsService;
            _objectRegistrationService = objectRegistrationService;
        }

        [Authorize(ApiKeyAuthenticationPolicy.Client)]
        [Route("longpoll/{objectId}")]
        [HttpGet]
        public async Task<IActionResult> LongPoll(Guid objectId)
        {
            if (objectId == Guid.Empty)
            {
                _logger.LogWarning("Long poll requested with invalid object id");
                return BadRequest();
            }

            // Check ownership of object
            var obj = await _objectRegistrationService.GetObject(objectId);
            if (obj?.UserId != User.Claims.GetValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogWarning("Current user ({currentUserId}) does not match object user ({objectUserId})", User.Identity?.Name, obj?.UserId);
                return Forbid();
            }

            var events = await _eventsService.WaitForObjectEvents(objectId, 60 * 1000);
            _logger.LogTrace("Collected {count} events for object {objectId}", events.Count(), objectId);

            return Ok(events);
        }

        [Authorize(ApiKeyAuthenticationPolicy.Client)]
        [Route("push/{objectId}")]
        [HttpPost]
        public async Task<IActionResult> Push(Guid objectId, CommandEvent evt)
        {
            if (objectId == Guid.Empty)
            {
                _logger.LogWarning("Push event requested with invalid objectId");
                return BadRequest();
            }

            // Check ownership of object
            var obj = await _objectRegistrationService.GetObject(objectId);
            if (obj?.UserId != User.Claims.GetValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogWarning("Current user ({currentUserId}) does not match object user ({objectUserId})", User.Identity?.Name, obj?.UserId);
                return Forbid();
            }

            if (!Enum.IsDefined(typeof(CommandEventCode), evt.Code))
            {
                _logger.LogWarning("Push event requested with invalid event code");
                return BadRequest();
            }

            var response = await _eventsService.PushEventToObject(objectId, evt);
            if (response.Data is null)
            {
                _logger.LogWarning("Pushing event {event} for object {objectId} failed with code: {code}", evt, objectId, response.HttpStatusCode);
            }

            _logger.LogTrace("Pushing event {event} for object {objectId}", evt, objectId);
            return Ok(response);
        }

        [Authorize(ApiKeyAuthenticationPolicy.Object)]
        [Route("receive/{objectId}")]
        [HttpPost]
        public async Task<IActionResult> Receive(Guid objectId, ObjectEvent evt)
        {
            if (objectId == Guid.Empty)
            {
                _logger.LogWarning("Receive event requested with invalid objectId");
                return BadRequest();
            }

            // Check object ownership
            var obj = await _objectRegistrationService.GetObject(objectId);
            if (obj?.ApiKey != this.GetApiKey())
            {
                return Forbid();
            }

            if (!Enum.IsDefined(typeof(ObjectEventCode), evt.Code))
            {
                _logger.LogWarning("Add event requested with invalid event code");
                return BadRequest();
            }

            if (!await _eventsService.AddEventForObject(objectId, evt))
            {
                _logger.LogWarning("Adding event {event} for object {objectId} failed", evt, objectId);
                return BadRequest();
            }

            _logger.LogTrace("Adding event {event} for object {objectId}", evt, objectId);

            return Ok();
        }
    }
}
