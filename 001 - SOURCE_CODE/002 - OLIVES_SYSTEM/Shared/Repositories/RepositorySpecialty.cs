﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shared.Enumerations;
using Shared.Interfaces;
using Shared.Models;
using Shared.ViewModels.Filter;
using Shared.ViewModels.Response;

namespace Shared.Repositories
{
    public class RepositorySpecialty : IRepositorySpecialty
    {
        #region Properties

        private readonly IOliveDataContext _dataContext;

        #endregion

        #region Constructors

        public RepositorySpecialty(IOliveDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Find the specialty by using specific id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Specialty> FindSpecialtyAsync(int id)
        {
            // Find the specialty by using id.
            var context = _dataContext.Context;
            IQueryable<Specialty> specialties = context.Specialties;
            return await specialties.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        ///     Filter specialties by using specific conditions.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<ResponseSpecialtyFilter> FilterSpecialtyAsync(FilterSpecialtyViewModel filter)
        {
            // Response initialization.
            var response = new ResponseSpecialtyFilter();

            // Filtered results initialization.
            var context = _dataContext.Context;
            IQueryable<Specialty> results = context.Specialties;

            // Specialty name has been specified.
            if (!string.IsNullOrEmpty(filter.Name))
                results = results.Where(x => x.Name.Contains(filter.Name));

            switch (filter.Direction)
            {
                case SortDirection.Ascending:
                    results = results.OrderBy(x => x.Name);
                    break;
                default:
                    results = results.OrderByDescending(x => x.Name);
                    break;
            }

            // Count the matching results.
            response.Total = await results.CountAsync();

            // By default, order record by name ascendingly.
            results = results.OrderBy(x => x.Name);

            // Record is defined.
            if (filter.Records != null)
            {
                results = results.Skip(filter.Page*filter.Records.Value)
                    .Take(filter.Records.Value);
            }

            // Retrieve the specialty list.
            response.Specialties = await results.ToListAsync();
            return response;
        }

        #endregion
    }
}