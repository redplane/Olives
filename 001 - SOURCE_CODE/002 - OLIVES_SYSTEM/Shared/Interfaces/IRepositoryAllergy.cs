﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;
using Shared.ViewModels;
using Shared.ViewModels.Response;

namespace Shared.Interfaces
{
    public interface IRepositoryAllergy
    {
        /// <summary>
        /// Filter allergies with specific conditions.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<ResponseAllergyFilter> FilterAllergy(AllergyGetViewModel filter);

        /// <summary>
        /// Initialize an allergy with given information.
        /// </summary>
        /// <param name="info"></param>
        Task<Allergy> InitializeAllergyAsync(Allergy info);

        /// <summary>
        /// Find allergy by using id and owner id.
        /// </summary>
        /// <param name="id">Allergy Id</param>
        /// <param name="owner">Allergy owner</param>
        /// <returns></returns>
        Task<IList<Allergy>> FindAllergyAsync(int id, int owner);

        /// <summary>
        /// Delete an allergy synchronously.
        /// </summary>
        /// <param name="allergy"></param>
        /// <returns></returns>
        void DeleteAllergy(Allergy allergy);
    }
}