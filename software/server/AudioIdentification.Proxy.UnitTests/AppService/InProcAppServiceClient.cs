//-----------------------------------------------------------------------
// <copyright file="InProcAppServiceClient.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.AppService
{
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification.Proxy.AppService;
    using Windows.ApplicationModel.AppService;
    using Windows.Foundation.Collections;

    /// <summary>
    ///  service client for the application service back-end, running in-process.
    /// </summary>
    public class InProcAppServiceClient : AppServiceClient
    {
        /// <summary>
        /// The service to use for the backing service.
        /// </summary>
        private readonly ISessionFactory backingService;

        /// <summary>
        /// True to initiate auto-close.
        /// </summary>
        private readonly bool autoClose;

        /// <summary>
        /// An in-process instance of the service.
        /// </summary>
        private AppServiceServer service;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcAppServiceClient" /> class.
        /// </summary>
        /// <param name="backingService">The service to use for the backing service.</param>
        /// <param name="autoClose">True to initiate auto-close.</param>
        public InProcAppServiceClient(ISessionFactory backingService, bool autoClose = false)
        {
            this.backingService = backingService;
            this.autoClose = autoClose;
            this.HasAutoClosed = false;
        }

        /// <summary>
        /// Gets a value indicating whether the connection has been auto-closed.
        /// </summary>
        public bool HasAutoClosed { get; private set; }

        /// <summary>
        /// Open a connection to the application service.
        /// </summary>
        /// <returns>True if the service was opened; false otherwise.</returns>
        public override Task<bool> OpenAsync()
        {
            this.Close();
            this.service = new AppServiceServer(this.backingService);
            this.service.ResponseAvailable += this.ServiceResponseAvailable;

            // Create it but it's not used.
            this.ServiceConnection = new AppServiceConnection();

            return Task.FromResult(true);
        }

        /// <summary>
        /// Open a connection to the application service.
        /// </summary>
        public override void Close()
        {
            this.ServiceConnection = null;
            this.service?.Dispose();
            this.service = null;
        }

        /// <summary>
        /// Send a message to the application service.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The service response.</returns>
        protected async override Task<ValueSet> SendMessageAsync(ValueSet message)
        {
            ValueSet result = await this.service.ProcessRequestAsync(message).ConfigureAwait(false);

            string command = message?[AppServiceServer.CommandKey] as string;
            if (command == AppServiceServer.SampleCommand)
            {
                if (this.autoClose && !this.HasAutoClosed)
                {
                    this.HasAutoClosed = true;
                    this.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Process state changed from the music id service.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ServiceResponseAvailable(object sender, ResponseAvailableEventArgs eventArgs)
        {
            this.ProcessRequestReceivedAsync(eventArgs.Response);
        }
    }
}