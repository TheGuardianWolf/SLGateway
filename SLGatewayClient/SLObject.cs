using Microsoft.Extensions.Logging;
using SLGatewayClient.Data;
using SLGatewayCore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public EventHandler<ObjectEvent>? OnEvent;
        public EventHandler<ListenEvent>? OnListenEvent;
        public EventHandler<DataserverEvent>? OnDataserverEvent;
        
        // TODO: Complete
        public EventHandler<object>? OnTouchEvent;
        public EventHandler<object>? OnLinkMessage;
        public EventHandler<object>? OnSensor;

        public SLObject(Guid objectId, GatewayClient client, ILogger? logger = null)
        {
            ObjectId = objectId;
            _client = client;
            _pollingClient = new LongPollObjectClient(objectId, client, logger);
            _pollingClient.OnEventReceived += PollingClient_OnEventReceived;
        }

        private void PollingClient_OnEventReceived(object sender, ObjectEvent<JsonElement> e)
        {
            var argsList = e.Args.ToList();
            switch (e.Code)
            {
                case ObjectEventCode.Listen:
                    {
                        var channel = argsList[0].GetInt32();
                        var name = argsList[1].GetString() ?? "";
                        var id = argsList[2].GetGuid();
                        var message = argsList[3].GetString() ?? "";
                        OnListenEvent?.BeginInvoke(this, new ListenEvent
                        {
                            Channel = channel,
                            Id = id,
                            Name = name,
                            Message = message
                        }, (r) => { }, new object());
                    }
                    break;
                case ObjectEventCode.Dataserver:
                    {
                        var queryId = argsList[0].GetGuid();
                        var data = argsList[1].GetString() ?? "";
                        OnDataserverEvent?.BeginInvoke(this, new DataserverEvent
                        {
                            QueryId = queryId,
                            Data = data
                        }, (r) => { }, new object());
                    }
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

        public async Task<ParcelDetails> GetParcelDetailsAsync(Vector pos, IEnumerable<ParcelDetailsParams>? @params = null)
        {
            var sendParams = new List<ParcelDetailsParams>
            {
                ParcelDetailsParams.Name,
                ParcelDetailsParams.Description,
                ParcelDetailsParams.OwnerKey,
                ParcelDetailsParams.GroupKey,
                ParcelDetailsParams.Area,
                ParcelDetailsParams.ParcelKey,
                ParcelDetailsParams.CanSeeAvatars
            };

            if (@params != null)
            {
                sendParams = @params.ToList();
            }

            var argsList = new List<object>
            {
                pos.X,
                pos.Y,
                pos.Z
            };
            
            foreach (var p in sendParams)
            {
                argsList.Add(p);
            }

            var result = await _client.SendCommandAsync<IEnumerable<JsonElement>>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetParcelDetails,
                Args = argsList
            });

            EnsureCommandSuccess(result);

            if (result.Data == null)
            {
                throw new InvalidReturnDataTypeException(typeof(IEnumerable<JsonElement>), result.Data);
            }

            if (result.Data.Count() != sendParams.Count)
            {
                throw new InvalidReturnDataLengthException(sendParams.Count, result.Data.Count());
            }

            var parcelDetails = ParcelDetails.FromParams(sendParams, result.Data);

            return parcelDetails;
        }

        public Task<object> GetObjectDetailsAsync(Guid id, IEnumerable<object> @params)
        {
            throw new NotImplementedException();
        }

        public async Task<Vector> GetPosAsync()
        {
            var result = await _client.SendCommandAsync<IEnumerable<double>>(ObjectId, new CommandEvent
            {
                Code = CommandEventCode.LLGetPos,
                Args = new object[] { }
            });

            EnsureCommandSuccess(result);

            if (result.Data == null)
            {
                throw new InvalidReturnDataTypeException(typeof(IEnumerable<int>), result.Data);
            }

            if (result.Data.Count() != 3)
            {
                throw new InvalidReturnDataLengthException(3, result.Data.Count());
            }

            var dataList = result.Data.ToList();
            var vector = new Vector(dataList[0], dataList[1], dataList[2]);

            return vector;
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
