namespace AiChat.Application.Authentications.Dtos
{
    public enum AuthenticationProviderEnum
    {
        Local, //لاگین با username/password دیتابیس خودت
        ActiveDirectory, //لاگین با فرم، ولی username/password در AD چک می‌شود
        WindowsIntegrated //بدون فرم، با Negotiate و هویت ویندوز کاربر
    }
}
