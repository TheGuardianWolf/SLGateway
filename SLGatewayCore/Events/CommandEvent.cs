using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLGatewayCore.Events
{
    public enum CommandEventCode
    {
        LLOwnerSay = 0,
        LLSay = 1,
        LLRegionSay = 2,
        LLRegionSayTo = 3,
        LLApplyRotationalImpulse = 4,
        LLListen = 5,
        LLListenRemove = 6,
        LLDie = 7,
        LLEjectFromLand = 8,
        LLGetOwner = 9,
        LLGetAgentList = 10,
        LLGetAgentInfo = 11,
        LLRequestAgentData = 12,
        LLRequestDisplayName = 13,
        LLGetParcelDetails = 14,
        LLGetObjectDetails = 15,
        LLGetPos = 16
    }

    /// <summary>
    /// Used to send LL commands to the in world object
    /// </summary>
    public class CommandEvent
    {
        public CommandEventCode Code { get; set; }
        public IEnumerable<object> Args { get; set; } = Enumerable.Empty<object>();
    }
}
