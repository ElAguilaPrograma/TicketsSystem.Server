namespace TicketsSystem.Core.Interfaces
{
    public interface IGetUserRole
    {
        Task<bool> UserIsAdmin(Guid userId);
        Task<bool> UserIsAgent(Guid userId);
        Task<bool> UserIsUser(Guid userId);
    }
}
