using SLGatewayClient;
using SLGatewayClient.Data;

namespace SLWatchtower.Services
{
    public interface IAgentService
    {
        Task<AgentWorldProfile> GetAgentProfile(Guid agentKey);
    }

    public class AgentService : IAgentService
    {
        private readonly SLWorld _worldApi;
        private readonly ILogger _logger;

        public AgentService(ILogger<SLWorld> logger)
        {
            _logger = logger;
            _worldApi = new SLWorld(_logger);
        }

        public async Task<AgentWorldProfile> GetAgentProfile(Guid agentKey)
        {
            return await _worldApi.GetAgentProfile(agentKey);
        }
    }
}
