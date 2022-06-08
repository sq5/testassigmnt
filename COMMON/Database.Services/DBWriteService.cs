// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DATABASE.Context;
using System;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public class DBWriteService : IDBWriteService
    {
        private readonly SearchServiceDBContext _db;

        public DBWriteService(SearchServiceDBContext db)
        {
            _db = db;
        }

        public void Add<TEntity>(TEntity item) where TEntity : class
        {
            try
            {
                _db.Add(item);
            }
            catch
            {
                throw;
            }
        }

        public void Delete<TEntity>(TEntity item) where TEntity : class
        {
            try
            {
                _db.Set<TEntity>().Remove(item);
            }
            catch
            {
                throw;
            }
        }

        public void Update<TEntity>(TEntity item) where TEntity : class
        {
            try
            {
                var entity = _db.Find<TEntity>(item.GetType().GetProperty("Id").GetValue(item));
                if (entity != null)
                    _db.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                _db.Set<TEntity>().Update(item);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateWithoutCheck<TEntity>(TEntity item) where TEntity : class
        {
            try
            {
                _db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                _db.Set<TEntity>().Update(item);
            }
            catch
            {
                throw;
            }
        }

        public async Task<(bool res, string err)> SaveChangesAsync()
        {
            try
            {
                return (await _db.SaveChangesAsync() >= 0, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.InnerException.Message);
            }
        }
    }
}
