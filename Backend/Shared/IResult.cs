namespace Shared;

public interface IResult
{
    bool IsSuccess { get; }

    bool IsFailure { get; }

    Error Error { get; }
}

public interface IResult<out T> : IResult
{
    T Value { get; }
}
