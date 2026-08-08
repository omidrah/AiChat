using AiChat.Application.Authentications.Dtos;

namespace AiChat.Api.Contracts.Admin
{
    public interface IActiveDirectorySettingsService
    {
        ActiveDirectoryOptions Get();
        Task SaveAsync(ActiveDirectoryOptions options, CancellationToken cancellationToken = default);
    }
}



