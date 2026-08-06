namespace AiChat.Api.Contracts.Admin
{
    public interface IActiveDirectoryDiagnosticService
    {
        Task<ActiveDirectoryDiagnosticResultDto> RunAsync(
            ActiveDirectoryDiagnosticRequest request,
            CancellationToken cancellationToken = default);
    }
}