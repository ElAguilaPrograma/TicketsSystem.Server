using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T?> GetById(Guid id);
        Task Create(T entity);
        Task Update(T entity);
    }
}
