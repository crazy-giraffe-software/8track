//-----------------------------------------------------------------------
// <copyright file="Session.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.Proxy.AppService;
    using Windows.Foundation;

    /// <summary>
    /// Session for identifying a song.
    /// </summary>
    public class Session : ISessionProxy
    {
        /// <summary>
        /// The application service client.
        /// </summary>
        private readonly IAppServiceClient client;

        /// <summary>
        /// Options for the session.
        /// </summary>
        private readonly SessionOptions options;

        /// <summary>
        /// Session id.
        /// </summary>
        private string sessionId;

        /// <summary>
        /// Samples for the session.
        /// </summary>
        private ConcurrentQueue<byte[]> sampleBuffer;

        /// <summary>
        ///  Task for sending samples.
        /// </summary>
        private Task sendSampleTask;

        /// <summary>
        /// The identified tracks.
        /// </summary>
        private List<IReadOnlyTrack> tracks;

        /// <summary>
        /// To detect redundant calls to Dispose().
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Session" /> class.
        /// </summary>
        /// <param name="client">The client to use for the proxy.</param>
        /// <param name="options">the session options.</param>
        internal Session(IAppServiceClient client, SessionOptions options)
        {
            this.client = client;
            this.options = options;
            this.sampleBuffer = new ConcurrentQueue<byte[]>();
            this.sendSampleTask = Task.CompletedTask;
            this.IdentificationStatus = IdentifyStatus.Invalid;
            this.SessionIdentifier = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public event StatusChangedEventHandler StatusChanged;

        /// <inheritdoc/>
        public string SessionIdentifier { get; private set; }

        /// <inheritdoc/>
        public IdentifyStatus IdentificationStatus { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void AddAudioSample(byte[] audioData)
        {
            // Queue samples and start task to drain the queue.
            if (this.IdentificationStatus != IdentifyStatus.Complete)
            {
                this.sampleBuffer.Enqueue(audioData);
                if (this.sendSampleTask.IsCompleted)
                {
                    this.sendSampleTask = Task.Run(this.SendSamplesAsync);
                }
            }
        }

        /// <inheritdoc/>
        public IAsyncOperation<IReadOnlyList<IReadOnlyTrack>> GetTracksAsync()
        {
            return Task.FromResult<IReadOnlyList<IReadOnlyTrack>>(this.tracks).AsAsyncOperation();
        }

        /// <inheritdoc/>
        public void ProcessTrackResponse(IdentifyStatus status, IEnumerable<IReadOnlyTrack> tracks)
        {
            bool changed = this.IdentificationStatus != status;
            this.IdentificationStatus = status;
            this.tracks = new List<IReadOnlyTrack>(tracks);

            if (changed)
            {
                this.StatusChanged?.Invoke(this, new StatusChangedEventArgs(status));
            }
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
                    if (this.client != null)
                    {
                        this.client.EndSessionAsync(this.sessionId);
                    }

                    this.sampleBuffer = new ConcurrentQueue<byte[]>();
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Send the samples to the application service.
        /// </summary>
        /// <returns>A Task that can be awaited.</returns>
        private async Task SendSamplesAsync()
        {
            try
            {
                // While the track has not been identified, send samples
                // to the application service.
                while (this.IdentificationStatus != IdentifyStatus.Complete)
                {
                    // Get next sample from the buffer. If empty, exit the loop.
                    if (this.sampleBuffer.IsEmpty)
                    {
                        break;
                    }

                    // If anything to send, send it.
                    if (this.sampleBuffer.TryDequeue(out byte[] nextSample))
                    {
                        // Re-open the client if needed. If so, get a new session if as well.
                        if (!this.client.IsOpen)
                        {
                            Debug.WriteLine("Open client in background thread.");
                            await this.client.OpenAsync().ConfigureAwait(false);

                            this.sessionId = string.Empty;
                        }

                        // Start session
                        if (string.IsNullOrEmpty(this.sessionId))
                        {
                            this.sessionId = await this.client.StartSessionAsync(this, this.options).ConfigureAwait(false);
                        }

                        // Send the sample. If status is completed,
                        // exit the loop and dump the remaining samples.
                        this.client.SendAudioSampleAsync(this.sessionId, nextSample);

                        // Check status and exit loop.
                        if (this.IdentificationStatus == IdentifyStatus.Complete)
                        {
                            // Completed. Clear all samples and return;
                            this.sampleBuffer = new ConcurrentQueue<byte[]>();
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If an exception was thrown, we'll need to start over.
                // Dump the samples and close the connection.
                this.client.Close();
                this.sampleBuffer = new ConcurrentQueue<byte[]>();

                // Re-throw any exceptions.
                throw;
            }
        }
    }
}
