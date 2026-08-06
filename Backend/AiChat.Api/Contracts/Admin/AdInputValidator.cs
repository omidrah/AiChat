using System.Net;
using System.Text.RegularExpressions;

namespace AiChat.Api.Contracts.Admin;

public static partial class AdInputValidator
{
    [GeneratedRegex(
        @"^(?=.{1,253}$)([a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$",
        RegexOptions.Compiled)]
    private static partial Regex DomainRegex();

    [GeneratedRegex(
        @"^[a-zA-Z0-9][a-zA-Z0-9.-]{0,252}$",
        RegexOptions.Compiled)]
    private static partial Regex HostRegex();

    [GeneratedRegex(
        @"^(?:(?:CN|OU|DC)=[^,=]+)(?:,(?:(?:CN|OU|DC)=[^,=]+))*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DistinguishedNameRegex();

    public static bool IsValidHost(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (IPAddress.TryParse(value, out _))
            return true;

        return HostRegex().IsMatch(value);
    }

    public static bool IsValidDomain(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
               && DomainRegex().IsMatch(value);
    }

    public static bool IsValidDistinguishedName(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
               && DistinguishedNameRegex().IsMatch(value);
    }
}