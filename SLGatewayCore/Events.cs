using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLGatewayCore
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
        LLRequestDisplayName = 13
    }

    public class CommandEventInfo
    {
        public CommandEventCode Code { get; }
        public IEnumerable<dynamic> Args { get; }
        public Type[] ArgTypes { get; } = new Type[] { };
        public Type ReturnType { get; } = typeof(void);
        public ObjectEventCode? LinkedEventCode { get; } = null;

        public CommandEventInfo(CommandEventCode evtCode, IEnumerable<dynamic> args)
        {
            Code = evtCode;
            Args = args;

            switch (evtCode)
            {
                case CommandEventCode.LLOwnerSay:
                    ArgTypes = new Type[] { };
                    break;
                case CommandEventCode.LLSay:
                    ArgTypes = new Type[] { typeof(int), typeof(string) };
                    break;
                case CommandEventCode.LLRegionSay:
                    ArgTypes = new Type[] { typeof(int), typeof(string) };
                    break;
                case CommandEventCode.LLRegionSayTo:
                    ArgTypes = new Type[] { typeof(Guid), typeof(int), typeof(string) };
                    break;
                case CommandEventCode.LLApplyRotationalImpulse:
                    ArgTypes = new Type[] { typeof(float), typeof(float), typeof(float), typeof(bool) };
                    break;
                case CommandEventCode.LLListen:
                    ArgTypes = new Type[] { typeof(int), typeof(string), typeof(Guid), typeof(string) };
                    ReturnType = typeof(int);
                    LinkedEventCode = ObjectEventCode.Listen;
                    break;
                case CommandEventCode.LLListenRemove:
                    ArgTypes = new Type[] { typeof(int) };
                    break;
                case CommandEventCode.LLDie:
                    ArgTypes = new Type[] { };
                    break;
                case CommandEventCode.LLEjectFromLand:
                    ArgTypes = new Type[] { typeof(Guid) };
                    break;
                case CommandEventCode.LLGetOwner:
                    ArgTypes = new Type[] { };
                    ReturnType = typeof(Guid);
                    break;
                case CommandEventCode.LLGetAgentList:
                    ArgTypes = new Type[] { typeof(int), typeof(IEnumerable<object>) };
                    ReturnType = typeof(IEnumerable<string>); 
                    break;
                case CommandEventCode.LLGetAgentInfo:
                    ArgTypes = new Type[] { typeof(Guid) };
                    ReturnType = typeof(int);
                    break;
                case CommandEventCode.LLRequestAgentData:
                    ArgTypes = new Type[] { typeof(Guid), typeof(int) };
                    ReturnType = typeof(Guid);
                    LinkedEventCode = ObjectEventCode.Dataserver;
                    break;
                case CommandEventCode.LLRequestDisplayName:
                    ArgTypes = new Type[] { typeof(Guid) };
                    ReturnType = typeof(Guid);
                    LinkedEventCode = ObjectEventCode.Dataserver;
                    break;
            }
        }
    }

    /// <summary>
    /// Used to send LL commands to the in world object
    /// </summary>
    public class CommandEvent
    {
        public CommandEventCode Code { get; set; }
        public IEnumerable<dynamic> Args { get; set; } = Enumerable.Empty<dynamic>();
    }

    public class CommandEventResponse
    {
        public int HttpStatus { get; set; }
        public dynamic? Data { get; set; }
    }

    public static class CommandEventExtenstions
    {
        public static CommandEventInfo ToEventInfo(this CommandEvent evt)
        {
            return new CommandEventInfo(evt.Code, evt.Args);
        }
    }

    public enum ObjectEventCode
    {
        Listen = 0,
        Dataserver = 1
    }

    public class ObjectEventInfo
    {
        public ObjectEventCode Code { get; }
        public IEnumerable<dynamic> Args { get; }
        public Type[] ArgTypes { get; } = new Type[] { };
        public Type ReturnType { get; } = typeof(void);
        public ObjectEventInfo(ObjectEventCode evtCode, IEnumerable<dynamic> args)
        {
            Code = evtCode;
            Args = args;

            switch (evtCode)
            {
                case ObjectEventCode.Listen:
                    ArgTypes = new Type[] { typeof(int), typeof(string), typeof(Guid), typeof(string) };
                    break;
                case ObjectEventCode.Dataserver:
                    ArgTypes = new Type[] { typeof(Guid), typeof(string) };
                    break;
            }
        }
    }

    /// <summary>
    /// Event sent from in world object
    /// </summary>
    public class ObjectEvent
    {
        public ObjectEventCode Code { get; set; }
        public IEnumerable<dynamic> Args { get; set; } = Enumerable.Empty<dynamic>();
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }

    public static class ObjectEventExtenstions
    {
        public static ObjectEventInfo ToEventInfo(this ObjectEvent evt)
        {
            return new ObjectEventInfo(evt.Code, evt.Args);
        }
    }
}
