namespace Shared
{
    public record Error
    {
        public string Code { get; }

        public string Description { get; }

        private Error() { }

        public Error(string code, string description)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public static Error None =>
            new(string.Empty, string.Empty);
    }

    public sealed record NotFoundError(
    string Code,
    string Description)
    : Error(Code, Description);

    public sealed record BadRequestError(
        string Code,
        string Description)
        : Error(Code, Description);

    public sealed record DuplicateError(
        string Code,
        string Description)
        : Error(Code, Description);
}
