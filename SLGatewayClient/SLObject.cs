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
        private GatewayClient _client;

        private IEnumerable<ObjectEventHandle> _eventHandles = new List<ObjectEventHandle>();

        private Guid _objectId;

        public bool EnableEvents
        {
            get => _client.EventPolling;
            set => _client.EventPolling = value;
        }

        public SLObject(Guid objectId, GatewayClient client, ILogger? logger = null)
        {
            _objectId = objectId;
            _client = client;
        }

        private void EnsureCommandSuccess(CommandEventResponse response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new CommandFailedException(response.HttpStatusCode);
            }
        }

        // Object commands
        public async Task OwnerSayAsync(string text)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLOwnerSay,
                Args = new dynamic[] { text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task SayAsync(string text, int channel = 0)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLSay,
                Args = new dynamic[] { channel, text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task RegionSayAsync(string text, int channel = 0)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLSay,
                Args = new dynamic[] { channel, text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task RegionSayToAsync(Guid target, string text, int channel = 0)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLRegionSayTo,
                Args = new dynamic[] { target, channel, text }
            });

            EnsureCommandSuccess(result);
        }

        public async Task ApplyRotationalImpulseAsync(IVector vector, bool local = true)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLApplyRotationalImpulse,
                Args = new dynamic[] { vector.X, vector.Y, vector.Z, local }
            });

            EnsureCommandSuccess(result);
        }

        public async Task<ObjectEventHandle?> ListenAsync(int channel, string exactMatchLegacyName, Guid filterId, string exactMatchText)
        {
            if (!EnableEvents)
            {
                return null;
            }

            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLListen,
                Args = new dynamic[] { channel, exactMatchLegacyName, filterId, exactMatchText }
            });

            EnsureCommandSuccess(result);

            if (!(result.Data is int))
            {
                throw new InvalidReturnDataTypeException(typeof(int), result.Data);
            }

            return new ObjectEventHandle
            {
                Code = ObjectEventCode.Listen,
                Handle = result.Data
            };
        }

        public async Task ListenRemoveAsync(int handle)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLListenRemove,
                Args = new dynamic[] { handle }
            });

            EnsureCommandSuccess(result);
        }

        public async Task DieAsync()
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLDie,
                Args = new dynamic[] { }
            });

            EnsureCommandSuccess(result);
        }

        public async Task EjectFromLandAsync(Guid agentId)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLEjectFromLand,
                Args = new dynamic[] { agentId }
            });

            EnsureCommandSuccess(result);
        }

        public async Task<Guid> GetOwnerAsync()
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetOwner,
                Args = new dynamic[] { }
            });

            EnsureCommandSuccess(result);

            return result.Data ?? Guid.Empty;
        }

        public async Task<IEnumerable<Guid>> GetAgentListAsync(AgentListScope scope)
        {
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetAgentList,
                Args = new dynamic[] { (int)scope }
            });

            EnsureCommandSuccess(result);

            if (!(result.Data is IEnumerable<string>))
            {
                throw new InvalidReturnDataTypeException(typeof(int), result.Data);
            }

            IEnumerable<string> data = result.Data;

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
            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetAgentInfo,
                Args = new dynamic[] { agentId }
            });

            EnsureCommandSuccess(result);

            if (!(result.Data is int))
            {
                throw new InvalidReturnDataTypeException(typeof(int), result.Data);
            }

            return result.Data ?? 0;
        }

        public async Task<ObjectEventHandle?> RequestAgentDataAsync(Guid agentId, AgentData dataFlags)
        {
            if (!EnableEvents)
            {
                return null;
            }

            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLRequestAgentData,
                Args = new dynamic[] { agentId, dataFlags }
            });

            EnsureCommandSuccess(result);

            if (!(result.Data is int))
            {
                throw new InvalidReturnDataTypeException(typeof(int), result.Data);
            }

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

            var result = await _client.SendCommandAsync(_objectId, new CommandEvent
            {
                Code = CommandEventCode.LLRequestAgentData,
                Args = new dynamic[] { agentId }
            });

            EnsureCommandSuccess(result);

            if (!(result.Data is int))
            {
                throw new InvalidReturnDataTypeException(typeof(int), result.Data);
            }

            return new ObjectEventHandle
            {
                Code = ObjectEventCode.Dataserver,
                Handle = result.Data
            };
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
                    await ListenRemoveAsync(eventHandle.Handle);
                }
            }
            await _client.DisposeAsync();
        }
    }

}
