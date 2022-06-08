// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public interface IDBReadService
    {
        Task<List<TEntity>> GetAsync<TEntity>() where TEntity : class;
        Task<List<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;
        Task<TEntity> SingleAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;
        Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;
        void Include<TEntity>() where TEntity : class;
        void Include<TEntity1, TEntity2>() where TEntity1 : class where TEntity2 : class;
        (int users, int settings, int organizations, int clients, int roles) Count();
    }
}
