using FluentResults;

namespace TicketsSystem.Core.Errors
{
    public class AppError : Error
    {
        public AppError(string message, int statusCode) : base (message)
        {
            Metadata.Add("ErrorCode", statusCode);
        }
    }

    public class NotFoundError : AppError
    {
        public NotFoundError(string message)
            : base(message, 404) { }
    }

    public class BadRequestError : AppError
    {
        public BadRequestError(string message)
            : base(message, 400) { }
    }

    public class ForbiddenError : AppError
    {
        public ForbiddenError(string message)
            : base(message, 403) { }
    }

    public class UnauthorizedError : AppError
    {
        public UnauthorizedError(string message)
            : base(message, 401) { }
    }

    public class InternalServerError : AppError
    {
        public InternalServerError(string message)
            : base (message, 500) { }
    }

    public class PayloadTooLargeError : AppError
    {
        public PayloadTooLargeError(string message)
            : base (message, 413) { }
    }

    public class UnsupportedMediaType : AppError
    {
        public UnsupportedMediaType(string message) 
            : base (message, 415) { }
    }
}
