namespace Shared;

public class Result : IResult
{
    public bool IsSuccess { get; private set; }

    public bool IsFailure => IsSuccess is false;

    public Error Error { get; private set; }

    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException();
        }
        else if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result<T> Success<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new(true, Error.None, value);
    }

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Failure<T>(Error error) => new(false, error, default);
}

public class Result<T>(bool isSuccess, Error error, T? value)
    : Result(isSuccess, error), IResult<T>
{
    public T Value => IsFailure
        ? throw new InvalidOperationException("Cannot access the value of a failed result.")
        : value!;
}
