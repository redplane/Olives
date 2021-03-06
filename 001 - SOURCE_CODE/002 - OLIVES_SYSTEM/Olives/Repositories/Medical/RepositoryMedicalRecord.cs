﻿using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Olives.Enumerations.Filter;
using Olives.Interfaces.Medical;
using Olives.ViewModels.Filter.Medical;
using Olives.ViewModels.Response.Medical;
using Shared.Enumerations;
using Shared.Interfaces;
using Shared.Models;

namespace Olives.Repositories.Medical
{
    public class RepositoryMedicalRecord : IRepositoryMedicalRecord
    {
        #region Properties

        private readonly IOliveDataContext _dataContext;

        #endregion

        #region Constructors

        public RepositoryMedicalRecord(IOliveDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Initialize / edit a medical record asynchronously.
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public async Task<MedicalRecord> InitializeMedicalRecordAsync(MedicalRecord initializer)
        {
            // Context initialization.
            var context = _dataContext.Context;

            // Record doesn't contain id. That means it is a new record. 
            context.MedicalRecords.AddOrUpdate(initializer);

            // Save the record asynchronously.
            await context.SaveChangesAsync();

            return initializer;
        }

        /// <summary>
        ///     Find a medical record by using specific id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MedicalRecord> FindMedicalRecordAsync(int id)
        {
            // By default, take all record.
            var context = _dataContext.Context;
            IQueryable<MedicalRecord> medicalRecords = context.MedicalRecords;

            // Find the record by using id.
            return await medicalRecords.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        ///     Delete medical record asynchronously.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<int> DeleteMedicalRecordAsync(FilterMedicalRecordViewModel filter)
        {
            var context = _dataContext.Context;
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    #region Medical record

                    IQueryable<MedicalRecord> medicalRecords = context.MedicalRecords;
                    medicalRecords = FilterMedicalRecords(medicalRecords, filter, context);
                    var medicalRecordsList = await medicalRecords.ToListAsync();

                    #endregion

                    #region Medical record dependencies
                    
                    foreach (var medicalRecord in medicalRecordsList)
                    {
                        // Experiment note.
                        context.ExperimentNotes.RemoveRange(context.ExperimentNotes.Where(x => x.MedicalRecordId == medicalRecord.Id));

                        // Medical note.
                        context.MedicalNotes.RemoveRange(
                            context.MedicalNotes.Where(x => x.MedicalRecordId == medicalRecord.Id));
                        
                        // Medical images.
                        IQueryable<MedicalImage> medicalImages = context.MedicalImages;
                        medicalImages = medicalImages.Where(x => x.MedicalRecordId == medicalRecord.Id);
                        await medicalImages.ForEachAsync(x =>
                        {
                            var junkFile = new JunkFile();
                            junkFile.FullPath = x.FullPath;
                            context.JunkFiles.Add(junkFile);
                        });

                        context.MedicalImages.RemoveRange(medicalImages);

                        // Prescription images.
                        IQueryable<PrescriptionImage> prescriptionImages = context.PrescriptionImages;
                        prescriptionImages = prescriptionImages.Where(x => x.MedicalRecordId == medicalRecord.Id);
                        await prescriptionImages.ForEachAsync(x =>
                        {
                            var junkFile = new JunkFile();
                            junkFile.FullPath = x.FullPath;
                            context.JunkFiles.Add(junkFile);
                        });

                        context.PrescriptionImages.RemoveRange(prescriptionImages);

                        // Prescription
                        IQueryable<Prescription> prescriptions = context.Prescriptions;
                        prescriptions = prescriptions.Where(x => x.MedicalRecordId == medicalRecord.Id);
                        context.Prescriptions.RemoveRange(prescriptions);

                    }

                    #endregion
                    
                    // Lastly, medical records should be deleted.
                    context.MedicalRecords.RemoveRange(medicalRecords);

                    // Save changes asynchronously.
                    var records = await context.SaveChangesAsync();

                    // Begin the transaction.
                    transaction.Commit();

                    return records;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Filter medical records by using specific conditions.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<ResponseMedicalRecordFilter> FilterMedicalRecordAsync(FilterMedicalRecordViewModel filter)
        {
            // By default, take all records.
            var context = _dataContext.Context;
            IQueryable<MedicalRecord> medicalRecords = context.MedicalRecords;
            medicalRecords = FilterMedicalRecords(medicalRecords, filter, context);

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
                medicalRecords = medicalRecords.Skip(filter.Page*filter.Records.Value)
                    .Take(filter.Records.Value);
            }

            response.MedicalRecords = await medicalRecords.ToListAsync();

            return response;
        }

        /// <summary>
        ///     Filter medical records by using specific conditions.
        /// </summary>
        /// <param name="medicalRecords"></param>
        /// <param name="filter"></param>
        /// <param name="olivesHealthEntities"></param>
        /// <returns></returns>
        private IQueryable<MedicalRecord> FilterMedicalRecords(IQueryable<MedicalRecord> medicalRecords,
            FilterMedicalRecordViewModel filter, OlivesHealthEntities olivesHealthEntities)
        {
            // Base on requester role, do the filter.
            medicalRecords = FilterMedicalRecordsByRequesterRole(medicalRecords, filter, olivesHealthEntities);

            // Id is specified.
            if (filter.Id != null)
                medicalRecords = medicalRecords.Where(x => x.Id == filter.Id);

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

            return medicalRecords;
        }

        /// <summary>
        ///     Base on the requester role to do exact filter function.
        /// </summary>
        /// <param name="medicalRecords"></param>
        /// <param name="filter"></param>
        /// <param name="olivesHealthEntities"></param>
        /// <returns></returns>
        private IQueryable<MedicalRecord> FilterMedicalRecordsByRequesterRole(IQueryable<MedicalRecord> medicalRecords,
            FilterMedicalRecordViewModel filter, OlivesHealthEntities olivesHealthEntities)
        {
            // Requester is not defined.
            if (filter.Requester == null)
                throw new Exception("Requester must be specified.");

            // Patient only can see his/her records.
            if (filter.Requester.Role == (byte) Role.Patient)
            {
                medicalRecords = medicalRecords.Where(x => x.Owner == filter.Requester.Id);
                if (filter.Partner != null)
                    medicalRecords = medicalRecords.Where(x => x.Creator == filter.Partner.Value);

                return medicalRecords;
            }

            // Doctor can see every record whose owner has connection to him/her.
            IQueryable<Relationship> relationships = olivesHealthEntities.Relationships;
            relationships = relationships.Where(x => x.Target == filter.Requester.Id);

            // Partner is specified. This means to be a patient
            // Only patient can send request to doctor, that means he/she is the source of relationship.
            if (filter.Partner != null)
                relationships = relationships.Where(x => x.Source == filter.Partner.Value);

            var results = from r in relationships
                from m in medicalRecords
                where r.Source == m.Owner || r.Source == m.Creator
                select m;

            return results;
        }

        #endregion
    }
}