using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SLGateway.Controllers
{
    public static class ControllerExtensions
    {
        public static string GetApiKey(this ControllerBase controller)
        {
            string authHeader = controller.Request.Headers["Authorization"];
            var key = authHeader.Split(' ', StringSplitOptions.TrimEntries)[1];
            return key;
        }
    }
}
