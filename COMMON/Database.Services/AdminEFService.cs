// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using ARCHIVE.COMMON.Servises;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public class AdminEFService : IAdminService
    {
        #region Properties
        private readonly IDBReadService _dbRead;
        private readonly IDBWriteService _dbWrite;
        private readonly IMapper _mapper;
        #endregion
        public AdminEFService(IDBReadService dbRead, IDBWriteService dbWrite, IMapper mapper)
        {
            _dbRead = dbRead;
            _dbWrite = dbWrite;
            _mapper = mapper;
        }

        public async Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return await _dbRead.AnyAsync(expression);
        }

        public async Task<Int64> CreateAsyncInt64<TSource, TDestination>(TSource item)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entity = _mapper.Map<TDestination>(item);
                _dbWrite.Add(entity);
                var succeeded = await _dbWrite.SaveChangesAsync();
                if (succeeded.res) return (Int64)entity.GetType().GetProperty("Id").GetValue(entity);
            }
            catch (Exception)
            {
            }
            return -1;
        }

        public string AddOrUpdateItemWOSave<TSource, TDestination>(TSource item, bool create = true)
            where TSource : class
            where TDestination : class
        {
            string err = "";
            try
            {
                var entity = _mapper.Map<TDestination>(item);
                if (create)
                    _dbWrite.Add(entity);
                else
                    _dbWrite.Update(entity);
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;
        }
        public async Task<string> SaveDBChangesAsync()
        {
            string err = "";
            try
            {
                var succeeded = await _dbWrite.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;
        }

        public async Task<int> CreateAsyncInt32<TSource, TDestination>(TSource item)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entity = _mapper.Map<TDestination>(item);
                _dbWrite.Add(entity);
                var succeeded = await _dbWrite.SaveChangesAsync();
                if (succeeded.res) return (int)entity.GetType().GetProperty("Id").GetValue(entity);
            }
            catch (Exception)
            {
            }
            return -1;
        }

        public async Task<(bool res, string err)> DeleteAsync<TSource>(Expression<Func<TSource, bool>> expression) where TSource : class
        {
            try
            {
                var entity = await _dbRead.SingleAsync(expression);
                _dbWrite.Delete(entity);
                var result = await _dbWrite.SaveChangesAsync();
                return (res: result.res, err: result.err);
            }
            catch (Exception ex)
            {
                return (res: false, err: ex.Message + " StackTrace: " + ex.StackTrace);
            }
        }

        public async Task<List<TDestination>> GetAsync<TSource, TDestination>(bool include = false)
            where TSource : class
            where TDestination : class
        {
            if (include)
                _dbRead.Include<TSource>();
            var entities = await _dbRead.GetAsync<TSource>();
            return _mapper.Map<List<TDestination>>(entities);
        }

        public async Task<List<TDestination>> GetAsync<TSource, TDestination>(Expression<Func<TSource, bool>> expression, bool include = false)
            where TSource : class
            where TDestination : class
        {
            if (include) _dbRead.Include<TSource>();
            var entities = await _dbRead.GetAsync(expression);
            return _mapper.Map<List<TDestination>>(entities);
        }

        public async Task<TDestination> SingleAsync<TSource, TDestination>(Expression<Func<TSource, bool>> expression, bool include = false)
            where TSource : class
            where TDestination : class
        {
            if (include) _dbRead.Include<TSource>();
            var entities = await _dbRead.SingleAsync(expression);
            return _mapper.Map<TDestination>(entities);
        }

        public async Task<bool> UpdateAsync<TSource, TDestination>(TSource item)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entity = _mapper.Map<TDestination>(item);
                _dbWrite.Update(entity);
                var result = await _dbWrite.SaveChangesAsync();
                return result.res;
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}
