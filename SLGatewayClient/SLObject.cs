using SLGatewayCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SLGatewayClient
{
    public class SLObject : IAsyncDisposable, IDisposable
    {
        private GatewayClient _client;

        private IEnumerable<ObjectEventHandle> _eventHandles = new List<ObjectEventHandle>();

        public bool EnableEvents
        {
            get => _client.EventPolling;
            set => _client.EventPolling = value;
        }

        public SLObject(GatewayClient client)
        {
            _client = client;
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

        public void ApplyRotationalImpulse(IVector vector, bool local)
        {

        }

        public Task<ObjectEventHandle?> ListenAsync(int channel, string exactMatchLegacyName, Guid filterId, string exactMatchText)
        {
            if (!EnableObjectEvents)
            {
                return null;
            }
        }

        public Task ListenRemove(int handle)
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

        public ObjectEventHandle? RequestAgentData(Guid agentId, AgentData dataFlags)
        {
            if (!EnableObjectEvents)
            {
                return null;
            }
        }

        public ObjectEventHandle? RequestDisplayName(Guid agentId)
        {
            if (!EnableObjectEvents)
            {
                return null;
            }
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var eventHandle in _eventHandles)
            {
                if (eventHandle.Code == ObjectEventCode.Listen)
                {
                    await ListenRemove(eventHandle.Handle);
                }
            }
            await _client.DisposeAsync();
        }
    }
}
