using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLGatewayCore.Events
{
    public enum ObjectEventCode
    {
        Listen = 0,
        Dataserver = 1
    }

    /// <summary>
    /// Event sent from in world object
    /// </summary>
    public class ObjectEvent
    {
        public ObjectEventCode Code { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }

    public class ObjectEvent<T> : ObjectEvent
    {
        public IEnumerable<T> Args { get; set; } = Enumerable.Empty<T>();
    }
}
