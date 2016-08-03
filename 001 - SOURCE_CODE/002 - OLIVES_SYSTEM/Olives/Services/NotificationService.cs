﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using log4net;
using Microsoft.AspNet.SignalR;
using Olives.Interfaces;
using Shared.Interfaces;
using Shared.Models;

namespace Olives.Services
{
    public class NotificationService : INotificationService
    {
        private ILog Log
        {
            get { return DependencyResolver.Current.GetService<ILog>(); }
        }
        /// <summary>
        /// Intstance which provides access to real time connection database.
        /// </summary>
        private readonly IRepositoryRealTimeConnection _repositoryRealTimeConnection;
        
        /// <summary>
        /// Initialize an instance of NotificationService.
        /// </summary>
        /// <param name="repositoryRealTimeConnection"></param>
        public NotificationService(IRepositoryRealTimeConnection repositoryRealTimeConnection)
        {
            _repositoryRealTimeConnection = repositoryRealTimeConnection;
        }

        /// <summary>
        /// This function is for broadcasting a notification to specified client.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="signalrHub">Hub which is used for broadcasting notification</param>
        /// <returns></returns>
        public async Task<Notification> BroadcastNotificationAsync(Notification notification, IHubContext signalrHub)
        {
            // Database context initialization.
            var context = new OlivesHealthEntities();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Initialize the notification into database.
                    context.Notifications.Add(notification);

                    // Save the change asynchronously.
                    await context.SaveChangesAsync();

                    #region Notification broadcast

                    try
                    {
                        // Find all connections come from the recipient.
                        var connections =
                            await
                                _repositoryRealTimeConnection.FindRealTimeConnectionIndexesAsync(notification.Recipient,
                                    null, null);
                        signalrHub.Clients.Clients(connections).broadcastNotification(new
                        {
                            notification.Id,
                            notification.Type,
                            notification.Topic,
                            notification.Broadcaster,
                            notification.Recipient,
                            notification.Record,
                            notification.Message,
                            notification.Created
                        });
                    }
                    catch (Exception exception)
                    {
                        // As exception happens, log it and let the function continue running.
                        Log.Error(exception.Message, exception);
                    }

                    #endregion

                    // Submit the transaction.
                    transaction.Commit();

                    return notification;
                }
                catch (Exception exception)
                {
                    // Rollback the transaction.
                    transaction.Rollback();

                    // As exception happens, log it and let the function continue running.
                    Log.Error(exception.Message, exception);

                    return null;
                }
            }
        }
        
    }
}