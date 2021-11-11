using SLGatewayClient;
using SLWatchtower.Data;
using SLWatchtower.Repositories;

namespace SLWatchtower.Services
{
    public interface IParcelService
    {
        Task<bool> AddParcel(ParcelRegistration parcelRegistration);
        Task<IEnumerable<Guid>> GetParcelAgentList(string userId, Guid objectId);
        Task<ParcelDetails> GetParcelDetails(string userId, Guid objectId);
        Task<IEnumerable<ParcelRegistration>> GetParcelsForUser(string userId);
        Task<bool> RemoveParcel(string userId, Guid objectId);
    }

    public class ParcelService : IParcelService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IParcelRegistrationRepository _parcelRegistrationRepository;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IGatewayService _gatewayService;

        public ParcelService(ILogger<ParcelService> logger, IParcelRegistrationRepository parcelRegistrationRepository, IUserRegistrationService userRegistrationService, IGatewayService gatewayService, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _parcelRegistrationRepository = parcelRegistrationRepository;
            _userRegistrationService = userRegistrationService;
            _gatewayService = gatewayService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<ParcelRegistration>> GetParcelsForUser(string userId)
        {
            if (!await _userRegistrationService.HasRegistration(userId))
            {
                return null;
            }

            var entities = await _parcelRegistrationRepository.GetByUser(userId);
            return entities.Select(x => x.ToParcelRegistration());
        }

        public async Task<bool> AddParcel(ParcelRegistration parcelRegistration)
        {
            if (!await _userRegistrationService.HasRegistration(parcelRegistration.UserId))
            {
                return false;
            }

            if (parcelRegistration.ObjectId == Guid.Empty)
            {
                return false;
            }

            var parcelDetails = await GetParcelDetails(parcelRegistration.UserId, parcelRegistration.ObjectId);

            parcelRegistration.ParcelName = parcelDetails.ParcelName;
            parcelRegistration.ParcelId = parcelDetails.ParcelId;
            parcelRegistration.ParcelDescription = parcelDetails.ParcelDescription;

            return await _parcelRegistrationRepository.Update(parcelRegistration.ToEntity());
        }

        public async Task<bool> RemoveParcel(string userId, Guid objectId)
		{
            if (objectId == Guid.Empty)
			{
                return false;
			}

            if (!await _parcelRegistrationRepository.Delete(userId, objectId))
			{
                return false;
			}

            return true;
		}

        public async Task<ParcelDetails> GetParcelDetails(string userId, Guid objectId)
        {
            var slo = _gatewayService.GetObject(userId, objectId);

            var objectPosition = await slo.GetPosAsync();
            var parcelInfo = await slo.GetParcelDetailsAsync(objectPosition, new[] { ParcelDetailsParams.Name, ParcelDetailsParams.Description, ParcelDetailsParams.ParcelKey });

            return new ParcelDetails
            {
                ParcelDescription = parcelInfo.Description,
                ParcelId = parcelInfo.ParcelKey,
                ParcelName = parcelInfo.Name,
            };
        }

        public async Task<IEnumerable<Guid>> GetParcelAgentList(string userId, Guid objectId)
        {
            var slo = _gatewayService.GetObject(userId, objectId);

            var agentList = await slo.GetAgentListAsync(AgentListScope.Parcel);

            return agentList;
        }
    }
}
