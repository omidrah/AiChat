using AiChat.Application.Abstractions;
using System.Collections.Concurrent;

namespace AiChat.Api.Services
{
    /// <summary>
    /// یک کلاس Singleton برای ردیابی و مدیریت توکن‌های لغو در سطح برنامه ایجاد می‌کنیم. این کلاس به ما اجازه می‌دهد تا برای هر مکالمه یک توکن ایجاد، نگهداری و در صورت نیاز باطل کنیم.
    /// </summary>
    public class ChatCancellationTracker : IChatCancellationTracker
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _ctsMap = new();

        public CancellationToken Register(string conversationId)
        {
            // اگر از قبل توکنی برای این مکالمه وجود دارد، ابتدا آن را باطل می‌کنیم
            Cancel(conversationId);

            var cts = new CancellationTokenSource();
            _ctsMap[conversationId] = cts;
            return cts.Token;
        }

        public void Cancel(string conversationId)
        {
            if (_ctsMap.TryRemove(conversationId, out var cts))
            {
                try
                {
                    cts.Cancel();
                }
                finally
                {
                    cts.Dispose();
                }
            }
        }

        public void Remove(string conversationId)
        {
            _ctsMap.TryRemove(conversationId, out _);
        }
    }

}
