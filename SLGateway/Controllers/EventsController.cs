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
using System.Threading.Tasks;

namespace SLGateway.Controllers
{
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.BearerAuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IEventsService _es;

        public EventsController(ILogger<EventsController> logger, IEventsService eventsService)
        {
            _logger = logger;
            _es = eventsService;
        }

        [Route("longpoll/{objectId}")]
        [HttpGet]
        public async Task<IActionResult> LongPoll(Guid objectId)
        {
            if (objectId == Guid.Empty)
            {
                _logger.LogWarning("Long poll requested with invalid object id");
                return BadRequest();
            }

            var events = await _es.WaitForObjectEvents(objectId, 60 * 1000);
            _logger.LogTrace("Collected {count} events for object {objectId}", events.Count(), objectId);

            return Ok(events);
        }

        [Route("push/{objectId}")]
        [HttpPost]
        public async Task<IActionResult> Push(Guid objectId, CommandEvent evt)
        {
            if (objectId == Guid.Empty)
            {
                _logger.LogWarning("Push event requested with invalid objectId");
                return BadRequest();
            }

            if (!Enum.IsDefined(typeof(CommandEventCode), evt.Code))
            {
                _logger.LogWarning("Push event requested with invalid event code");
                return BadRequest();
            }

            var response = await _es.PushEventToObject(objectId, evt);
            if (response.Data is null)
            {
                _logger.LogWarning("Pushing event {event} for object {objectId} failed with code: {code}", evt, objectId, response.HttpStatus);
                return StatusCode(response.HttpStatus);
            }

            _logger.LogTrace("Pushing event {event} for object {objectId}", evt, objectId);
            return Ok(response.Data);
        }

        [Route("receive/{objectId}")]
        [HttpPost]
        public IActionResult Receive(Guid objectId, ObjectEvent evt)
        {
            if (objectId == Guid.Empty)
            {
                _logger.LogWarning("Receive event requested with invalid objectId");
                return BadRequest();
            }

            if (!Enum.IsDefined(typeof(ObjectEventCode), evt.Code))
            {
                _logger.LogWarning("Add event requested with invalid event code");
                return BadRequest();
            }

            if (!_es.AddEventForObject(objectId, evt))
            {
                _logger.LogWarning("Adding event {event} for object {objectId} failed", evt, objectId);
                return BadRequest();
            }

            _logger.LogTrace("Adding event {event} for object {objectId}", evt, objectId);

            return Ok();
        }
    }
}
