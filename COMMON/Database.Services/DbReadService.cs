// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using DATABASE.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public class DbReadService : IDBReadService
    {
        private readonly SearchServiceDBContext _db;

        public DbReadService(SearchServiceDBContext db)
        {
            _db = db;
        }

        public async Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return await _db.Set<TEntity>().AnyAsync(expression);
        }

        public async Task<List<TEntity>> GetAsync<TEntity>() where TEntity : class
        {
            return await _db.Set<TEntity>().ToListAsync();
        }

        public async Task<List<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return await _db.Set<TEntity>().Where(expression).ToListAsync();
        }

        public async Task<TEntity> SingleAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return await _db.Set<TEntity>().Where(expression).SingleOrDefaultAsync();
        }

        public void Include<TEntity>() where TEntity : class
        {
            var propNames = _db.Model.FindEntityType(typeof(TEntity)).GetNavigations().Select(e => e.Name);
            foreach (var name in propNames)
            {
                _db.Set<TEntity>().Include(name).Load();
            }
        }

        public void Include<TEntity1, TEntity2>()
            where TEntity1 : class
            where TEntity2 : class
        {
            Include<TEntity1>();
            Include<TEntity2>();
        }

        public (int users, int settings, int organizations, int clients, int roles) Count()
        {
            return (
                    users: _db.Users.Count(),
                    settings: _db.Settings.Count(),
                    organizations: _db.Organizations.Count(),
                    clients: _db.Clients.Count(),
                    roles: _db.Roles.Count()
                );
        }
    }
}
