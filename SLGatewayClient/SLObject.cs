using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public class SLObject
    {
        private GatewayClient _client;

        public SLObject(GatewayClient client)
        {
            _client = client;
        }

        // Object events
        public bool EnableObjectEvents()
        {

        }

        // Object commands
        public void OwnerSay(string text)
        {

        }

        public void Say(string text, int channel = 0)
        {

        }

        public void RegionSay(string text, int channel = 0)
        {

        }

        public void RegionSayTo(Guid target, string text, int channel = 0)
        {

        }

        public void ApplyRotationalImpulse(Vector vector, bool local)
        {

        }

        public EventHandle? Listen(int channel, string exactMatchLegacyName, Guid filterId, string exactMatchText)
        {
            if (!EnableObjectEvents)
            {
                return null;
            }
        }

        public void ListenRemove(EventHandle handle)
        {

        }

        public void Die()
        {

        }

        public void EjectFromLand(Guid agentId)
        {

        }

        public Guid GetOwner()
        {

        }

        public IEnumerable<string> GetAgentList(AgentListScope scope)
        {

        }

        public AgentInfo GetAgentInfo(Guid agentId)
        {

        }

        public EventHandle? RequestAgentData(Guid agentId, AgentData dataFlags)
        {
            if (!EnableObjectEvents)
            {
                return null;
            }
        }

        public EventHandle? RequestDisplayName(Guid agentId)
        {
            if (!EnableObjectEvents)
            {
                return null;
            }
        }
    }
}
