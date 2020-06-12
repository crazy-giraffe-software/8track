//-----------------------------------------------------------------------
// <copyright file="AppServiceTaskBase.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.AppService
{
    using System;
    using Windows.ApplicationModel.AppService;
    using Windows.ApplicationModel.Background;
    using Windows.Foundation.Collections;

    /// <summary>
    /// BAckgroud task for the application service.
    /// </summary>
    public abstract class AppServiceTaskBase : IBackgroundTask
    {
        /// <summary>
        /// A deferral.
        /// </summary>
        private BackgroundTaskDeferral backgroundTaskDeferral;

        /// <summary>
        /// The application service connection.
        /// </summary>
        private AppServiceConnection appServiceconnection;

        /// <summary>
        /// The app service instance.
        /// </summary>
        private AppServiceServer appService;

        /// <summary>
        /// Run the background service.
        /// </summary>
        /// <param name="taskInstance">A task instance.</param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Grab a deferral and listen for cancellation.
            this.backgroundTaskDeferral = taskInstance?.GetDeferral();
            taskInstance.Canceled += this.OnTaskCanceled;

            // Retrieve the connection and listen for incoming service requests.
            if (taskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                this.appServiceconnection = details.AppServiceConnection;
                this.appServiceconnection.RequestReceived += this.OnRequestReceived;
            }
        }

        /// <summary>
        /// Get the session factory.
        /// </summary>
        /// <returns>A session factory.</returns>
        protected abstract ISessionFactory GetSessionFactory();

        /// <summary>
        /// Handle incoming requests.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event arguments.</param>
        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral because we use an await-able API below to respond to the message
            // and we don't want this call to get canceled while we are waiting.
            var messageDeferral = args.GetDeferral();

            // Need a session factory?
            if (this.appService == null)
            {
                this.appService = new AppServiceServer(this.GetSessionFactory());
                this.appService.ResponseAvailable += this.AppServiceResponseAvailable;
            }

            // Get request and process.
            ValueSet message = args.Request.Message;
            if (message.Count > 0)
            {
                ValueSet returnData = await this.appService.ProcessRequestAsync(message).ConfigureAwait(false);

                // Return the data to the caller.
                await args.Request.SendResponseAsync(returnData);
            }

            // Complete the deferral so that the platform knows that we're done responding to the app service call.
            // If no more session, release task deferral.
            if (this.appService != null)
            {
                bool releaseTaksDeferral = this.appService.ActiveSessionCount == 0;
                messageDeferral.Complete();
                if (releaseTaksDeferral)
                {
                    this.appService.Dispose();
                    this.appService = null;
                    this.backgroundTaskDeferral?.Complete();
                }
            }
        }

        /// <summary>
        /// Handle cancellation.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="reason">The cancellation reason.</param>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.appService?.Dispose();
            this.backgroundTaskDeferral?.Complete();
        }

        /// <summary>
        /// Process state changed from the app service.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private async void AppServiceResponseAvailable(object sender, ResponseAvailableEventArgs eventArgs)
        {
            if (this.appServiceconnection != null)
            {
                await this.appServiceconnection.SendMessageAsync(eventArgs.Response);
            }
        }
    }
}
