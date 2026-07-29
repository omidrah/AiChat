namespace AiChat.Application.Abstractions;

using AiChat.Application.Common.Ai;
using AiChat.Application.Common.Dtos;
using System.Threading;
using System.Threading.Tasks;

public interface IAiHealthService
{
    // برای چک کردن دوره‌ای سلامت هوش مصنوعی (با کش)
    Task<AiHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);

    // متد جدید برای دریافت اطلاعات تفصیلی سرور Ollama (مخصوص صفحه وضعیت)

    Task<OllamaServerDetails> GetServerDetailsAsync(CancellationToken cancellationToken = default);

    // متد جدید برای دریافت لیست ساده مدل‌ها (جایگزین OllamaService)

    Task<List<ModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
}

