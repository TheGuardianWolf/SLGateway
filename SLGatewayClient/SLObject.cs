using Microsoft.Extensions.Logging;
using SLGatewayCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLGatewayClient
{
    public class SLObject : IAsyncDisposable, IDisposable
    {
        private readonly GatewayClient _client;

        private readonly IEnumerable<ObjectEventHandle> _eventHandles = new List<ObjectEventHandle>();

        private readonly LongPollObjectClient _pollingClient;

        public Guid ObjectId { get; }

        public bool EnableEvents
        {
            get => _pollingClient.Enabled;
            set => _pollingClient.Enabled = value;
        }

        public EventHandler<ListenEvent>? OnListenEvent;
        public EventHandler<DataserverEvent>? OnDataserverEvent;

        public SLObject(Guid objectId, GatewayClient client, ILogger? logger = null)
        {
            ObjectId = objectId;
            _client = client;
            _pollingClient = new LongPollObjectClient(objectId, client, logger);
            _pollingClient.OnEventReceived += PollingClient_OnEventReceived;
        }

        private void PollingClient_OnEventReceived(object sender, ObjectEvent e)
        {
            switch (e.Code)
            {
                case ObjectEventCode.Listen:
                    break;
                case ObjectEventCode.Dataserver:
                    break;
            }
        }

        private void EnsureCommandSuccess(EventResponse response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new CommandFailedException(response.HttpStatusCode);
            }
        }

        #region Object commands
        public async Task OwnerSayAsync(string text)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLOwnerSay,
                Args = new object[] { text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task SayAsync(string text, int channel = 0)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLSay,
                Args = new object[] { channel, text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task RegionSayAsync(string text, int channel = 0)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLSay,
                Args = new object[] { channel, text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task RegionSayToAsync(Guid target, string text, int channel = 0)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLRegionSayTo,
                Args = new object[] { target, channel, text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task ApplyRotationalImpulseAsync(IVector vector, bool local = true)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLApplyRotationalImpulse,
                Args = new object[] { vector.X, vector.Y, vector.Z, local }
            });

            EnsureCommandSuccess(result);
        }

        public async Task<ObjectEventHandle?> ListenAsync(int channel, string exactMatchLegacyName, Guid filterId, string exactMatchText)
        {
            if (!EnableEvents)
            {
                return null;
            }

            var result = await _client.SendCommandAsync<int>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLListen,
                Args = new object[] { channel, exactMatchLegacyName, filterId, exactMatchText }
            });

            EnsureCommandSuccess(result);

            return new ObjectEventHandle
            {
                Code = ObjectEventCode.Listen,
                Handle = result.Data
            };
        }

        public async Task ListenRemoveAsync(int handle)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLListenRemove,
                Args = new object[] { handle }
            });

            EnsureCommandSuccess(result);
        }

        public async Task DieAsync()
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLDie,
                Args = new object[] { }
            });

            EnsureCommandSuccess(result);
        }

        public async Task EjectFromLandAsync(Guid agentId)
        {
            var result = await _client.SendCommandAsync(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLEjectFromLand,
                Args = new object[] { agentId }
            });

            EnsureCommandSuccess(result);
        }

        public async Task<Guid> GetOwnerAsync()
        {
            var result = await _client.SendCommandAsync<Guid>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetOwner,
                Args = new object[] { }
            });

            EnsureCommandSuccess(result);

            return result.Data;
        }

        public async Task<IEnumerable<Guid>> GetAgentListAsync(AgentListScope scope)
        {
            var result = await _client.SendCommandAsync<IEnumerable<string>>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetAgentList,
                Args = new object[] { (int)scope }
            });

            EnsureCommandSuccess(result);

            var data = result.Data;

            if (data.Any())
            {
                var firstItem = data.First();
                var success = Guid.TryParse(firstItem, out var _);
                if (!success)
                {
                    throw new CommandFailedException(firstItem);
                }
            }

            return data.Select(x =>
            {
                Guid.TryParse(x, out var guid);
                return guid;
            }).Where(x => x != Guid.Empty);
        }

        public async Task<AgentInfo> GetAgentInfoAsync(Guid agentId)
        {
            var result = await _client.SendCommandAsync<int>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetAgentInfo,
                Args = new object[] { agentId }
            });

            EnsureCommandSuccess(result);

            return (AgentInfo)result.Data;
        }

        public async Task<ObjectEventHandle?> RequestAgentDataAsync(Guid agentId, AgentData dataFlags)
        {
            if (!EnableEvents)
            {
                return null;
            }

            var result = await _client.SendCommandAsync<int>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLRequestAgentData,
                Args = new object[] { agentId, dataFlags }
            });

            EnsureCommandSuccess(result);

            return new ObjectEventHandle
            {
                Code = ObjectEventCode.Dataserver,
                Handle = result.Data
            };
        }

        public async Task<ObjectEventHandle?> RequestDisplayNameAsync(Guid agentId)
        {
            if (!EnableEvents)
            {
                return null;
            }

            var result = await _client.SendCommandAsync<int>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLRequestAgentData,
                Args = new object[] { agentId }
            });

            EnsureCommandSuccess(result);

            return new ObjectEventHandle
            {
                Code = ObjectEventCode.Dataserver,
                Handle = result.Data
            };
        }
        #endregion

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
                    await ListenRemoveAsync(eventHandle.Handle);
                }
            }
            await _pollingClient.DisposeAsync();
        }
    }

}
