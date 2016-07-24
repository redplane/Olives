﻿using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Shared.Enumerations;
using Shared.Enumerations.Filter;
using Shared.Interfaces;
using Shared.Models;
using Shared.ViewModels.Filter;
using Shared.ViewModels.Response;

namespace Shared.Repositories
{
    public class RepositoryMedicalRecord : IRepositoryMedicalRecord
    {
        /// <summary>
        ///     Initialize / edit a medical record asynchronously.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<MedicalRecord> InitializeMedicalRecordAsync(MedicalRecord info)
        {
            // Database context initialization.
            var context = new OlivesHealthEntities();

            // Add or update the record.
            context.MedicalRecords.AddOrUpdate(info);

            // Save the record asynchronously.
            await context.SaveChangesAsync();
            return info;
        }

        /// <summary>
        ///     Find a medical record by using specific id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MedicalRecord> FindMedicalRecordAsync(int id)
        {
            // Database context initialization.
            var context = new OlivesHealthEntities();

            // By default, take all record.
            IQueryable<MedicalRecord> results = context.MedicalRecords;

            // Find the record by using id.
            return await results.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        ///     Initialize / update medical image.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<MedicalImage> InitializeMedicalImageAsync(MedicalImage info)
        {
            // Database context initialization.
            var context = new OlivesHealthEntities();

            context.MedicalImages.AddOrUpdate(info);
            await context.SaveChangesAsync();
            return info;
        }

        /// <summary>
        ///     Filter medical records by using specific conditions.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<ResponseMedicalRecordFilter> FilterMedicalRecordAsync(FilterMedicalRecordViewModel filter)
        {
            // Database context initialization.
            var context = new OlivesHealthEntities();

            // By default, take all records.
            IQueryable<MedicalRecord> medicalRecords = context.MedicalRecords;

            // Base on the mode of image filter to decide the role of requester.
            if (filter.Mode == RecordFilterMode.RequesterIsOwner)
            {
                medicalRecords = medicalRecords.Where(x => x.Owner == filter.Requester);
                if (filter.Partner != null)
                    medicalRecords = medicalRecords.Where(x => x.Creator == filter.Partner.Value);
            }
            else if (filter.Mode == RecordFilterMode.RequesterIsCreator)
            {
                medicalRecords = medicalRecords.Where(x => x.Creator == filter.Requester);
                if (filter.Partner != null)
                    medicalRecords = medicalRecords.Where(x => x.Owner == filter.Partner);
            }
            else
            {
                if (filter.Partner == null)
                    medicalRecords =
                        medicalRecords.Where(x => x.Creator == filter.Requester || x.Owner == filter.Requester);
                else
                    medicalRecords =
                        medicalRecords.Where(
                            x =>
                                (x.Creator == filter.Requester && x.Owner == filter.Partner.Value) ||
                                (x.Creator == filter.Partner.Value && x.Owner == filter.Requester));
            }


            // Time is specified.
            if (filter.MinTime != null) medicalRecords = medicalRecords.Where(x => x.Time >= filter.MinTime);
            if (filter.MaxTime != null) medicalRecords = medicalRecords.Where(x => x.Time <= filter.MaxTime);

            // Medical category is specified.
            if (filter.Category != null)
                medicalRecords = medicalRecords.Where(x => x.Category == filter.Category.Value);

            // Created is specified.
            if (filter.MinCreated != null) medicalRecords = medicalRecords.Where(x => x.Created >= filter.MinCreated);
            if (filter.MaxCreated != null) medicalRecords = medicalRecords.Where(x => x.Created <= filter.MaxCreated);

            // Last modified is specified.
            if (filter.MinLastModified != null)
                medicalRecords = medicalRecords.Where(x => x.LastModified >= filter.MinLastModified);
            if (filter.MaxLastModified != null)
                medicalRecords = medicalRecords.Where(x => x.LastModified <= filter.MaxLastModified);

            // Result sorting
            switch (filter.Direction)
            {
                case SortDirection.Ascending:
                    switch (filter.Sort)
                    {
                        case MedicalRecordFilterSort.Created:
                            medicalRecords = medicalRecords.OrderBy(x => x.Created);
                            break;
                        case MedicalRecordFilterSort.Time:
                            medicalRecords = medicalRecords.OrderBy(x => x.Time);
                            break;
                        default:
                            medicalRecords = medicalRecords.OrderBy(x => x.LastModified);
                            break;
                    }
                    break;
                default:
                    switch (filter.Sort)
                    {
                        case MedicalRecordFilterSort.Created:
                            medicalRecords = medicalRecords.OrderByDescending(x => x.Created);
                            break;
                        case MedicalRecordFilterSort.Time:
                            medicalRecords = medicalRecords.OrderByDescending(x => x.Time);
                            break;
                        default:
                            medicalRecords = medicalRecords.OrderByDescending(x => x.LastModified);
                            break;
                    }
                    break;
            }

            // Calculate the total result.
            var response = new ResponseMedicalRecordFilter();
            response.Total = await medicalRecords.CountAsync();

            // Record is defined.
            if (filter.Records != null)
            {
                medicalRecords = medicalRecords.Skip(filter.Page * filter.Records.Value)
                    .Take(filter.Records.Value);
            }

            response.MedicalRecords = await medicalRecords.ToListAsync();

            return response;
        }
    }
}