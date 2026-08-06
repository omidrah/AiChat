using AiChat.Application.Common.Options;

namespace AiChat.Api.Contracts.Admin
{
    public interface IActiveDirectorySettingsService
    {
        ActiveDirectoryOptions Get();
        Task SaveAsync(ActiveDirectoryOptions options, CancellationToken cancellationToken = default);
    }
}



