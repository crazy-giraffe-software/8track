//-----------------------------------------------------------------------
// <copyright file="AppServiceServer.cs" company="CrazyGiraffeSoftware.net">
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
    using Windows.Foundation.Collections;

    /// <summary>
    /// The audio identification application service.
    /// </summary>
    public class AppServiceServer
    {
        /// <summary>
        /// Service name.
        /// </summary>
        public const string ServiceName = "com.crazygiraffe.audioidentification";

        /// <summary>
        /// Key for commands.
        /// </summary>
        public const string CommandKey = "command";

        /// <summary>
        /// Value for start session command.
        /// </summary>
        public const string StartSessionCommand = "startSession";

        /// <summary>
        /// Value for sample command.
        /// </summary>
        public const string SampleCommand = "addSample";

        /// <summary>
        /// Value for status command.
        /// </summary>
        public const string StatusCommand = "status";

        /// <summary>
        /// Value for end session command.
        /// </summary>
        public const string EndSessionCommand = "endSession";

        /// <summary>
        /// Key for command status.
        /// </summary>
        public const string CommandStatusKey = "commandStatus";

        /// <summary>
        /// Key for command status OK.
        /// </summary>
        public const string CommandStatusOK = "ok";

        /// <summary>
        /// Key for command status fail.
        /// </summary>
        public const string CommandStatusFail = "fail";

        /// <summary>
        /// Key for options.
        /// </summary>
        public const string OptionsKey = "options";

        /// <summary>
        /// Key for session id.
        /// </summary>
        public const string SessionIdKey = "sessionId";

        /// <summary>
        /// Key for sample.
        /// </summary>
        public const string SampleKey = "sample";

        /// <summary>
        /// Key for status.
        /// </summary>
        public const string StatusKey = "status";

        /// <summary>
        /// Key for track count.
        /// </summary>
        public const string TrackCountKey = "trackCount";

        /// <summary>
        /// Format for track key.
        /// </summary>
        public const string TrackKeyFormat = "track-{0}";

        /// <summary>
        /// The session factory.
        /// </summary>
        private readonly ISessionFactory sessionFactory;

        /// <summary>
        /// The collection of sessions.
        /// </summary>
        private readonly Dictionary<string, ISession> sessions;

        /// <summary>
        /// To detect redundant calls to Dispose().
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceServer" /> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory instance.</param>
        public AppServiceServer(ISessionFactory sessionFactory)
        {
            this.sessions = new Dictionary<string, ISession>();
            this.sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Event handler for session state changed.
        /// </summary>
        public event EventHandler<ResponseAvailableEventArgs> ResponseAvailable;

        /// <summary>
        /// Gets the number of active sessions.
        /// </summary>
        public int ActiveSessionCount
        {
            get
            {
                return this.sessions.Count;
            }
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        /// <summary>
        /// Handle incoming requests.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>The response ValueSet.</returns>
        public async Task<ValueSet> ProcessRequestAsync(ValueSet message)
        {
            // Process request.
            ValueSet response = new ValueSet();
            if (message != null)
            {
                string command = message[AppServiceServer.CommandKey] as string;
                ISession session = this.GetSession(message);

                switch (command)
                {
                    case AppServiceServer.StartSessionCommand:
                        SessionOptions options = JsonConvert.DeserializeObject<SessionOptions>(message[AppServiceServer.OptionsKey] as string);
                        session = await this.sessionFactory.CreateSessionAsync(options);

                        if (session != null)
                        {
                            // Add an event handler
                            session.StatusChanged += this.SessionStatusChanged;

                            // Track a sessionId.
                            this.sessions.Add(session.SessionIdentifier, session);
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Open session in app service: {0}", session.SessionIdentifier));

                            response.Add(AppServiceServer.SessionIdKey, session.SessionIdentifier);
                            response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusOK);
                        }
                        else
                        {
                            Debug.WriteLine("Open session in app service failed");
                            response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusFail);
                        }

                        break;

                    case AppServiceServer.SampleCommand:
                        if (session != null)
                        {
                            byte[] sample = message[AppServiceServer.SampleKey] as byte[];
                            session.AddAudioSample(sample);
                            response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusOK);
                            break;
                        }

                        response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusFail);
                        break;

                    case AppServiceServer.EndSessionCommand:
                        if (session != null)
                        {
                            session.StatusChanged -= this.SessionStatusChanged;

                            string sessionId = message[AppServiceServer.SessionIdKey] as string;
                            this.sessions.Remove(sessionId);

                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Remove session: {0}", sessionId));
                            response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusOK);
                            break;
                        }

                        response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusFail);
                        break;

                    default:
                        Debug.WriteLine("Bad command");
                        response.Add(AppServiceServer.CommandStatusKey, AppServiceServer.CommandStatusFail);
                        break;
                }
            }

            // Return the data to the caller.
            return response;
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
                    // Clean up sessions.
                    ISession[] sessions = new ISession[this.sessions.Count];
                    this.sessions.Values.CopyTo(sessions, 0);
                    this.sessions.Clear();

                    foreach (ISession session in sessions)
                    {
                        session.StatusChanged -= this.SessionStatusChanged;
                    }
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Get a session from an id.
        /// </summary>
        /// <param name="message">The message that contains the session Id.</param>
        /// <returns>A session that matches session Id.</returns>
        private ISession GetSession(ValueSet message)
        {
            ISession session = null;
            if (message.ContainsKey(AppServiceServer.SessionIdKey))
            {
                string sessionId = message[AppServiceServer.SessionIdKey] as string;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    if (this.sessions.ContainsKey(sessionId))
                    {
                        session = this.sessions[sessionId];
                    }
                }
            }

            return session;
        }

        /// <summary>
        /// Process state changed from the session.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private async void SessionStatusChanged(object sender, StatusChangedEventArgs eventArgs)
        {
            ISession session = (ISession)sender;
            if (this.sessions.ContainsValue(session))
            {
                ValueSet sessionMessage = new ValueSet
                {
                    [AppServiceServer.CommandKey] = AppServiceServer.StatusCommand,
                    [AppServiceServer.SessionIdKey] = session.SessionIdentifier,
                };

                IdentifyStatus status = session.IdentificationStatus;
                sessionMessage[AppServiceServer.StatusKey] = (int)status;

                if (status == IdentifyStatus.Complete)
                {
                    int trackCount = 0;
                    IReadOnlyList<IReadOnlyTrack> tracks = await session.GetTracksAsync();
                    foreach (IReadOnlyTrack track in tracks)
                    {
                        string key = string.Format(CultureInfo.InvariantCulture, AppServiceServer.TrackKeyFormat, trackCount++);
                        sessionMessage[key] = JsonConvert.SerializeObject(track);
                    }

                    sessionMessage[AppServiceServer.TrackCountKey] = trackCount.ToString(CultureInfo.InvariantCulture);
                }

                this.ResponseAvailable?.Invoke(this, new ResponseAvailableEventArgs(sessionMessage));
            }
        }
    }
}
