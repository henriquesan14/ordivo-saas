namespace Ordivo.SharedKernel.Results;
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None) throw new ArgumentException("Invalid result state.", nameof(error));
        IsSuccess = isSuccess; Error = error;
    }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}
public sealed class Result<T>(T? value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    public T Value => IsSuccess ? value! : throw new InvalidOperationException("A failure result has no value.");
}
