﻿using System.Threading.Tasks;
using Olives.ViewModels.Filter.Personal;
using Olives.ViewModels.Response.Personal;
using Shared.Models;
using Shared.ViewModels.Filter;
using Shared.ViewModels.Response;

namespace Olives.Interfaces.PersonalNote
{
    public interface IRepositoryBloodPressure
    {
        /// <summary>
        ///     Initialize and save heart
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Task<BloodPressure> InitializeBloodPressureNoteAsync(BloodPressure info);

        /// <summary>
        ///     Find heartbeat by using heartbeat note id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BloodPressure> FindBloodPressureNoteAsync(int id);

        /// <summary>
        ///     Find heartbeat by using conditions.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<ResponseBloodPressureFilter> FilterBloodPressureNoteAsync(FilterBloodPressureViewModel filter);

        /// <summary>
        ///     Delete a heartbeat note asynchronously.
        /// </summary>
        /// <param name="filter"></param>
        Task<int> DeleteBloodPressureNoteAsync(FilterBloodPressureViewModel filter);
    }
}