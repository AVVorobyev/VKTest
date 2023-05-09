namespace Core
{
    public sealed class Result<T> : Result
    {
        public T? Model { get; private set; }

        public static Result<T> Success(T? model)
        {
            return new Result<T> { Model = model };
        }

        public new static Result<T> Fail(Exception exception, string message)
        {
            return new Result<T>()
            {
                Exception = exception,
                ErrorMessage = message
            };
        }
    }

    public class Result
    {
        public string? ErrorMessage { get; protected set; }
        public Exception? Exception { get; protected set; }
        public bool Succeeded => string.IsNullOrEmpty(ErrorMessage);

        protected Result() { }

        public static Result Success()
        {
            return new Result();
        }

        public static Result Fail(Exception exception, string message)
        {
            Guard.NotNull(exception, nameof(exception));
            Guard.NotNull(message, nameof(message));

            return new Result()
            {
                ErrorMessage = message,
                Exception = exception
            };
        }
    }
}
