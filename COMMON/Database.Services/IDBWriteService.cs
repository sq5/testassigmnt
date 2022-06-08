using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public interface IDBWriteService
    {
        Task<(bool res, string err)> SaveChangesAsync();

        void Add<TEntity>(TEntity item) where TEntity : class;
        void Delete<TEntity>(TEntity item) where TEntity : class;
        void Update<TEntity>(TEntity item) where TEntity : class;
        void UpdateWithoutCheck<TEntity>(TEntity item) where TEntity : class;
    }
}
