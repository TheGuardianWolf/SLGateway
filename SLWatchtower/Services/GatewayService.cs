using SLGatewayClient;

namespace SLWatchtower.Services
{
	public interface IGatewayService
	{
		SLGatewayClient.Gateway GetGatewayClient(string userId, string apiKey);
		SLObject GetObject(string userId, Guid objectId);
	}

	public class GatewayService : IDisposable, IGatewayService
	{
		private readonly ILogger<GatewayService> _logger;
		private readonly string _gatewayUrl;
		private readonly IDictionary<string, SLGatewayClient.Gateway> _userGateways = new Dictionary<string, SLGatewayClient.Gateway>();
		private readonly IDictionary<Guid, SLObject> _activeObjects = new Dictionary<Guid, SLObject>();

		public GatewayService(ILogger<GatewayService> logger, IConfiguration configuration)
		{
			_logger = logger;
			_gatewayUrl = configuration.GetValue<string>("GatewayUrl");
		}

		public SLGatewayClient.Gateway GetGatewayClient(string userId, string apiKey)
		{
			if (!_userGateways.ContainsKey(userId))
			{
				var newGateway = new SLGatewayClient.Gateway(new GatewayConfiguration
				{
					ApiKey = apiKey,
					GatewayUrl = _gatewayUrl,
					Logger = _logger
				});
				_userGateways[userId] = newGateway;
			}

			if (_userGateways[userId].ApiKey != apiKey)
			{
				_userGateways[userId].UpdateApiKey(apiKey);
			}

			var gateway = _userGateways[userId];

			return gateway;
		}

		public SLObject GetObject(string userId, Guid objectId)
		{
			if (!_userGateways.ContainsKey(userId))
			{
				return null;
			}

			if (_activeObjects.ContainsKey(objectId))
			{
				return _activeObjects[objectId];
			}

			var obj = _userGateways[userId].UseObject(objectId);
			_activeObjects[objectId] = obj;

			return obj;
		}

		public void Dispose()
		{
			foreach (var obj in _activeObjects.Values)
			{
				obj?.Dispose();
			}
			_activeObjects.Clear();
		}
	}
}
