using SLWatchtower.Data;
using SLWatchtower.Repositories;

namespace SLWatchtower.Services
{
    public interface IUserRegistrationService
    {
        Task<bool> HasRegistration(string userId);
        Task<UserRegistration> GetRegistration(string userId);
        Task<bool> UpdateRegistration(UserRegistration registration);
    }

    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly ILogger _logger;
        private readonly IUserRegistrationRepository _userRegistrationRepository;

        public UserRegistrationService(ILogger<UserRegistrationService> logger, IUserRegistrationRepository userRegistrationRepository)
        {
            _logger = logger;
            _userRegistrationRepository = userRegistrationRepository;
        }

        public async Task<bool> HasRegistration(string userId)
        {
            return (await _userRegistrationRepository.Get(userId)) is not null;
        }

        public async Task<UserRegistration> GetRegistration(string userId)
        {
            return (await _userRegistrationRepository.Get(userId))?.ToUserRegistration();
        }

        public async Task<bool> UpdateRegistration(UserRegistration registration)
        {
            _logger.LogTrace("Updating registration for user {userId} with apiKey {apiKey}", registration.UserId, registration.ApiKey);
            return (await _userRegistrationRepository.Update(registration.ToEntity()));
        }
    }
}
