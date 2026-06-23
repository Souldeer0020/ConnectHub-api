using ConnectHub.Application.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class 
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetBySpecAsync(Ispecification<T> spec);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<IReadOnlyList<T>> ListBySpecAsync(Ispecification<T> spec);
        Task<int> CountBySpecAsync(Ispecification<T> spec);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        bool UserNameExists(string username);
    }
}
