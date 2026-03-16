using FluentResults;

namespace TicketsSystem.Core.Errors
{
    public class AppSuccess : Success
    {
        public AppSuccess(string message, int statusCode) : base (message)
        {
            Metadata.Add("SuccessCode", statusCode);
        }
    }

    public class OkSuccess : AppSuccess
    {
        public OkSuccess(string message)
            : base (message, 200) { }
    }

    public class CreatedSuccess : AppSuccess
    {
        public CreatedSuccess(string message)
            : base (message, 201) { }
    }

    public class AcceptedSuccess : AppSuccess
    {
        public AcceptedSuccess(string message)
            : base(message, 202) { }
    }

    public class NoContentSuccess : AppSuccess
    {
        public NoContentSuccess(string message)
            : base(message, 204) { }
    }

    public class PartialContentSuccess : AppSuccess
    {
        public PartialContentSuccess(string message)
            : base(message, 206) { }
    }

}
