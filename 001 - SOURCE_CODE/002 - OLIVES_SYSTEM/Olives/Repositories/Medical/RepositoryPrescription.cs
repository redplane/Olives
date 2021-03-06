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
    public class RepositoryPrescription : IRepositoryPrescription
    {
        #region Properties

        private readonly IOliveDataContext _dataContext;

        #endregion

        #region Constructors

        public RepositoryPrescription(IOliveDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Find the prescription asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Prescription> FindPrescriptionAsync(int id)
        {
            // Take all prescriptions.
            var context = _dataContext.Context;
            IQueryable<Prescription> prescriptions = context.Prescriptions;

            // Find the prescription.
            prescriptions = prescriptions.Where(x => x.Id == id);

            return await prescriptions.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Find prescription by using specific conditions.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<Prescription> FindPrescriptionAsync(FilterPrescriptionViewModel filter)
        {
            // Database context initialization.
            var context = _dataContext.Context;

            // Take all prescription.
            IQueryable<Prescription> prescriptions = context.Prescriptions;

            // Filter prescription.
            prescriptions = FilterPrescriptions(prescriptions, filter);

            return await prescriptions.FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Initialize or update an prescription.
        /// </summary>
        /// <param name="prescription"></param>
        /// <returns></returns>
        public async Task<Prescription> InitializePrescriptionAsync(Prescription prescription)
        {
            var context = _dataContext.Context;

            // Initialize or update prescription.
            context.Prescriptions.AddOrUpdate(prescription);

            // Save change to database.
            await context.SaveChangesAsync();

            return prescription;
        }

        /// <summary>
        ///     Delete prescription by using id.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<int> DeletePrescriptionAsync(FilterPrescriptionViewModel filter)
        {
            var context = _dataContext.Context;

            // Begin a transaction.
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    #region Find all prescriptions

                    // By default, delete all record.
                    IQueryable<Prescription> prescriptions = context.Prescriptions;
                    prescriptions = FilterPrescriptions(prescriptions, filter);

                    // Load the prescriptions into memory.
                    var prescriptionsList = await prescriptions.ToListAsync();

                    #endregion

                    #region Find all prescription images

                    foreach (var prescription in prescriptionsList)
                    {
                        // Find all prescription images first.
                        IQueryable<PrescriptionImage> prescriptionImages = context.PrescriptionImages;

                        // Find the prescription images which belong to the deleted prescription.
                        prescriptionImages = prescriptionImages.Where(x => x.PrescriptionId == prescription.Id);

                        // Initialize a list of junk file.
                        await prescriptionImages.ForEachAsync(x =>
                        {
                            var junkFile = new JunkFile();
                            junkFile.FullPath = x.FullPath;

                            // Enlist all images which should be deleted.
                            context.JunkFiles.Add(junkFile);
                        });

                        // Delete all prescription images first due to its dependence on prescriptions.
                        context.PrescriptionImages.RemoveRange(prescriptionImages);
                    }

                    #endregion

                    // Delete the matched prescriptions.
                    context.Prescriptions.RemoveRange(prescriptions);

                    // Save the changes and count the number of affected records.
                    var records = await context.SaveChangesAsync();

                    // Commit the transaction.
                    transaction.Commit();

                    // Tell the calling function about the affected records.
                    return records;
                }
                catch
                {
                    // As the exception happens. Rollback the transaction and keep throwing error.
                    transaction.Rollback();

                    throw;
                }
            }
        }

        /// <summary>
        ///     Filter prescription asynchronously.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<ResponsePrescriptionFilterViewModel> FilterPrescriptionAsync(
            FilterPrescriptionViewModel filter)
        {
            // By default, take all prescriptions.
            var context = _dataContext.Context;
            IQueryable<Prescription> prescriptions = context.Prescriptions;

            // Base on the requester's role to do the exact filter.
            prescriptions = FilterPrescriptionByRequesterRole(prescriptions, filter, context);
            prescriptions = FilterPrescriptions(prescriptions, filter);

            // Sort the record.
            switch (filter.Sort)
            {
                case NoteResultSort.Created:
                    if (filter.Direction == SortDirection.Ascending)
                    {
                        prescriptions = prescriptions.OrderBy(x => x.Created);
                        break;
                    }
                    prescriptions = prescriptions.OrderByDescending(x => x.Created);
                    break;
                default:
                    if (filter.Direction == SortDirection.Ascending)
                    {
                        prescriptions = prescriptions.OrderBy(x => x.LastModified);
                        break;
                    }
                    prescriptions = prescriptions.OrderByDescending(x => x.LastModified);
                    break;
            }
            
            // Response initialization.
            var response = new ResponsePrescriptionFilterViewModel();
            response.Total = await prescriptions.CountAsync();

            // Record is defined.
            if (filter.Records != null)
            {
                prescriptions = prescriptions.Skip(filter.Page*filter.Records.Value)
                    .Take(filter.Records.Value);
            }

            // Retrieve the list of results.
            response.Prescriptions = await prescriptions
                .ToListAsync();

            return response;
        }

        /// <summary>
        ///     Filter prescriptions by using specific conditions.
        /// </summary>
        /// <param name="prescriptions"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private IQueryable<Prescription> FilterPrescriptions(IQueryable<Prescription> prescriptions,
            FilterPrescriptionViewModel filter)
        {
            // Id is specified.
            if (filter.Id != null)
                prescriptions = prescriptions.Where(x => x.Id == filter.Id.Value);

            // Medical record is defined.
            if (filter.MedicalRecord != null)
                prescriptions = prescriptions.Where(x => x.MedicalRecordId == filter.MedicalRecord);

            // From is defined.
            if (filter.MinFrom != null) prescriptions = prescriptions.Where(x => x.From >= filter.MinFrom);
            if (filter.MaxFrom != null) prescriptions = prescriptions.Where(x => x.From <= filter.MaxFrom);

            // To is defined.
            if (filter.MinTo != null) prescriptions = prescriptions.Where(x => x.To >= filter.MinTo);
            if (filter.MaxTo != null) prescriptions = prescriptions.Where(x => x.To <= filter.MaxTo);

            // Created is defined.
            if (filter.MinCreated != null) prescriptions = prescriptions.Where(x => x.Created >= filter.MinCreated);
            if (filter.MaxCreated != null) prescriptions = prescriptions.Where(x => x.Created <= filter.MaxCreated);

            // Last modified is defined.
            if (filter.MinLastModified != null)
                prescriptions = prescriptions.Where(x => x.LastModified >= filter.MinLastModified);
            if (filter.MaxLastModified != null)
                prescriptions = prescriptions.Where(x => x.LastModified <= filter.MaxLastModified);

            return prescriptions;
        }

        /// <summary>
        ///     Base on the requester role to do exact filter function.
        /// </summary>
        /// <param name="prescriptions"></param>
        /// <param name="filter"></param>
        /// <param name="olivesHealthEntities"></param>
        /// <returns></returns>
        private IQueryable<Prescription> FilterPrescriptionByRequesterRole(IQueryable<Prescription> prescriptions,
            FilterPrescriptionViewModel filter, OlivesHealthEntities olivesHealthEntities)
        {
            // Requester is not defined.
            if (filter.Requester == null)
                throw new Exception("Requester must be specified.");

            // Patient only can see his/her records.
            if (filter.Requester.Role == (byte) Role.Patient)
            {
                prescriptions = prescriptions.Where(x => x.Owner == filter.Requester.Id);
                if (filter.Partner != null)
                    prescriptions = prescriptions.Where(x => x.Creator == filter.Partner.Value);

                return prescriptions;
            }

            // Doctor can see every record whose owner has connection to him/her.
            IQueryable<Relationship> relationships = olivesHealthEntities.Relationships;
            relationships = relationships.Where(x => x.Target == filter.Requester.Id);

            // Partner is specified. This means to be a patient
            // Only patient can send request to doctor, that means he/she is the source of relationship.
            if (filter.Partner != null)
                relationships = relationships.Where(x => x.Source == filter.Partner.Value);

            var results = from r in relationships
                from m in prescriptions
                where r.Source == m.Owner || r.Source == m.Creator
                select m;

            return results;
        }

        #endregion
    }
}