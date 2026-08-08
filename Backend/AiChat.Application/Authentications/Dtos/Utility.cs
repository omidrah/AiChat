namespace AiChat.Application.Authentications.Dtos;
public static class Utility
{
    // داخل کلاس ActiveDirectoryAuthService یا یک Helper مشترک
    public static string NormalizeUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return string.Empty;

        if (userName.Contains('\\'))
            return userName.Split('\\', 2)[1].Trim();

        if (userName.Contains('@'))
            return userName.Split('@', 2)[0].Trim();

        return userName.Trim();
    }
}
