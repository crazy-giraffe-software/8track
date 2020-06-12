//-----------------------------------------------------------------------
// <copyright file="AppServiceClient.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.AppService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Windows.ApplicationModel.AppService;
    using Windows.Foundation.Collections;

    /// <summary>
    /// The client for the application service.
    /// </summary>
    public class AppServiceClient : IAppServiceClient, IDisposable
    {
        /// <summary>
        /// Key for service connection failed.
        /// </summary>
        private const string ServiceFailedKey = "serviceFailed";

        /// <summary>
        /// The collection of sessions.
        /// </summary>
        private readonly Dictionary<string, ISessionProxy> sessions;

        /// <summary>
        /// Lock for the connection.
        /// </summary>
        private readonly object connectionLock = new object();

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceClient" /> class.
        /// </summary>
        public AppServiceClient()
        {
            this.sessions = new Dictionary<string, ISessionProxy>();
        }

        /// <inheritdoc/>
        public bool IsOpen
        {
            get
            {
                return this.ServiceConnection != null;
            }
        }

        /// <summary>
        /// Gets or sets a connection to the application service.
        /// </summary>
        protected AppServiceConnection ServiceConnection { get; set; }

        /// <inheritdoc/>
        public virtual async Task<bool> OpenAsync()
        {
            // Add the connection.
            bool isOpen = this.IsOpen;
            if (!isOpen)
            {
                //// Use the package family name of the running app, i.e. it is hosted by our own app.
                AppServiceConnection newServiceConnection = null;
                AppServiceConnectionStatus status = AppServiceConnectionStatus.Unknown;
                try
                {
                    newServiceConnection = new AppServiceConnection
                    {
                        AppServiceName = AppServiceServer.ServiceName,
                        PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName,
                    };

                    status = await newServiceConnection.OpenAsync();
                    if (status == AppServiceConnectionStatus.Success)
                    {
                        lock (this.connectionLock)
                        {
                            this.ServiceConnection = newServiceConnection;
                            newServiceConnection = null;
                            this.ServiceConnection.RequestReceived += this.ServiceConnectionRequestReceived;
                            this.ServiceConnection.ServiceClosed += this.ServiceConnectionServiceClosed;
                        }
                    }
                }
                finally
                {
                    if (newServiceConnection != null)
                    {
                        newServiceConnection.Dispose();
                    }
                }

                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Open app service: {0}", status.ToString()));
                return status == AppServiceConnectionStatus.Success;
            }

            return true;
        }

        /// <inheritdoc/>
        public virtual void Close()
        {
            lock (this.connectionLock)
            {
                if (this.IsOpen)
                {
                    Debug.WriteLine("Close app service");
                    this.ServiceConnection.Dispose();
                    this.ServiceConnection = null;
                }
            }

            this.sessions.Clear();
        }

        /// <inheritdoc/>
        public async Task<string> StartSessionAsync(ISessionProxy session, SessionOptions options)
        {
            string sessionId = string.Empty;
            if (this.IsOpen)
            {
                ValueSet sessionMessage = new ValueSet
                {
                    [AppServiceServer.CommandKey] = AppServiceServer.StartSessionCommand,
                    [AppServiceServer.OptionsKey] = JsonConvert.SerializeObject(options),
                };
                Debug.WriteLine("Start session in client");

                ValueSet response = await this.SendMessageAsync(sessionMessage).ConfigureAwait(false);
                if (MessageContainsErrors(response))
                {
                    // Start over.
                    this.Close();
                    return string.Empty;
                }
                else
                {
                    sessionId = response[AppServiceServer.SessionIdKey] as string;
                    bool added = this.sessions.TryAdd(sessionId, session);
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Received session in client: {0}, added={1}", sessionId, added));
                }
            }

            return sessionId;
        }

        /// <inheritdoc/>
        public async Task SendAudioSampleAsync(string sessionId, byte[] sample)
        {
            // Send the sample.
            if (this.IsOpen)
            {
                ValueSet sampleMessage = new ValueSet
                {
                    [AppServiceServer.CommandKey] = AppServiceServer.SampleCommand,
                    [AppServiceServer.SessionIdKey] = sessionId,
                    [AppServiceServer.SampleKey] = sample,
                };

                ValueSet response = await this.SendMessageAsync(sampleMessage).ConfigureAwait(false);
                if (MessageContainsErrors(response))
                {
                    // Start over
                    this.Close();
                }
            }
        }

        /// <inheritdoc/>
        public async Task EndSessionAsync(string sessionId)
        {
            if (this.IsOpen)
            {
                ValueSet sessionMessage = new ValueSet
                {
                    [AppServiceServer.CommandKey] = AppServiceServer.EndSessionCommand,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                ValueSet response = await this.SendMessageAsync(sessionMessage).ConfigureAwait(false);
                _ = MessageContainsErrors(response);
            }

            // Remove session, close connection on last session.
            if (!string.IsNullOrEmpty(sessionId))
            {
                if (this.sessions.ContainsKey(sessionId))
                {
                    this.sessions.Remove(sessionId);
                    if (this.sessions.Count < 1)
                    {
                        this.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Check for error response.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if the message is an error.</returns>
        protected static bool MessageContainsErrors(ValueSet message)
        {
            if (message != null && !message.ContainsKey(AppServiceClient.ServiceFailedKey))
            {
                if (message.ContainsKey(AppServiceServer.CommandStatusKey))
                {
                    string status = message[AppServiceServer.CommandStatusKey] as string;
                    if (!string.IsNullOrEmpty(status))
                    {
                        if (status == AppServiceServer.CommandStatusOK)
                        {
                            return false;
                        }
                        else
                        {
                            Debug.WriteLine(string.Format(
                                CultureInfo.InvariantCulture,
                                "Failed service response: {0}",
                                status));
                        }
                    }
                    else
                    {
                        Debug.WriteLine(string.Format(
                            CultureInfo.InvariantCulture,
                            "Invalid service response: {0}",
                            status));
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "Missing command status key: {0}",
                        AppServiceServer.CommandStatusKey));
                }
            }
            else
            {
                Debug.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "Contains service failed key: {0}",
                    AppServiceClient.ServiceFailedKey));
            }

            return true;
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        /// <param name="disposing">true of disposing; false is finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.ServiceConnection != null)
                    {
                        this.ServiceConnection.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Send a message to the application service.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The service response.</returns>
        protected virtual async Task<ValueSet> SendMessageAsync(ValueSet message)
        {
            if (message != null)
            {
                Debug.Assert(message.ContainsKey(AppServiceServer.CommandKey), "Message must contain a command");

                ValueSet responseMessage = new ValueSet
                {
                    { AppServiceClient.ServiceFailedKey, true },
                };

                AppServiceConnection connection = null;
                lock (this.connectionLock)
                {
                    connection = this.ServiceConnection;
                }

                if (connection != null)
                {
                    try
                    {
                        AppServiceResponse response = await connection.SendMessageAsync(message);
                        if (response.Status == AppServiceResponseStatus.Success)
                        {
                            responseMessage = response.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error sending: {0}", ex.ToString()));
                        throw;
                    }
                }

                return responseMessage;
            }

            return null;
        }

        /// <summary>
        /// Service connection send a response.
        /// </summary>
        /// <param name="message">The response message.</param>
        /// <returns>The client response.</returns>
        protected ValueSet ProcessRequestReceivedAsync(ValueSet message)
        {
            // Process request.
            ValueSet response = new ValueSet();
            if (message != null)
            {
                string command = message[AppServiceServer.CommandKey] as string;
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Process message from app service: {0}", command));

                switch (command)
                {
                    case AppServiceServer.StatusCommand:

                        string sessionId = message[AppServiceServer.SessionIdKey] as string;
                        IdentifyStatus idStatus = (IdentifyStatus)message[AppServiceServer.StatusKey];
                        List<IReadOnlyTrack> tracks = new List<IReadOnlyTrack>();

                        if (idStatus == IdentifyStatus.Complete)
                        {
                            if (message.ContainsKey(AppServiceServer.TrackCountKey))
                            {
                                if (int.TryParse(message[AppServiceServer.TrackCountKey] as string, out int trackCount))
                                {
                                    for (int i = 0; i < trackCount; i++)
                                    {
                                        string key = string.Format(CultureInfo.InvariantCulture, AppServiceServer.TrackKeyFormat, i);
                                        Track track = JsonConvert.DeserializeObject<Track>(message[key] as string);
                                        tracks.Add(track);
                                    }
                                }
                            }
                        }

                        // Send response.
                        ISessionProxy proxy;
                        if (this.sessions.TryGetValue(sessionId, out proxy))
                        {
                            proxy.ProcessTrackResponse(idStatus, tracks);
                        }

                        break;

                    default:
                        response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusFail);
                        break;
                }
            }

            // Return the data to the caller.
            return response;
        }

        /// <summary>
        /// Service connection send a response.
        /// </summary>
        /// <param name="sender">Sender of the event handler.</param>
        /// <param name="args">The event arguments.</param>
        private async void ServiceConnectionRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Process request.
            ValueSet response = this.ProcessRequestReceivedAsync(args.Request.Message);
            await sender.SendMessageAsync(response);
        }

        /// <summary>
        /// Service connection was closed, likely out from under us.
        /// </summary>
        /// <param name="sender">Sender of the event handler.</param>
        /// <param name="args">The event arguments.</param>
        private void ServiceConnectionServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "ServiceConnectionServiceClosed: {0}", args.Status.ToString()));
            this.Close();
        }
    }
}
