// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ARCHIVE.COMMON.Servises
{
    public interface IAdminService
    {
        #region CRUD methods
        Task<List<TDestination>> GetAsync<TSource, TDestination>(bool include = false) where TSource : class where TDestination : class;
        Task<List<TDestination>> GetAsync<TSource, TDestination>(Expression<Func<TSource, bool>> expression, bool include = false) where TSource : class where TDestination : class;
        Task<TDestination> SingleAsync<TSource, TDestination>(Expression<Func<TSource, bool>> expression, bool include = false) where TSource : class where TDestination : class;
        Task<Int64> CreateAsyncInt64<TSource, TDestination>(TSource item) where TSource : class where TDestination : class;
        Task<Int32> CreateAsyncInt32<TSource, TDestination>(TSource item) where TSource : class where TDestination : class;
        Task<bool> UpdateAsync<TSource, TDestination>(TSource item) where TSource : class where TDestination : class;
        Task<(bool res, string err)> DeleteAsync<TSource>(Expression<Func<TSource, bool>> expression) where TSource : class;
        Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;
        Task<string> SaveDBChangesAsync();
        string AddOrUpdateItemWOSave<TSource, TDestination>(TSource item, bool create = true) where TSource : class where TDestination : class;
        #endregion
    }
}
