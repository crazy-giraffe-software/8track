//-----------------------------------------------------------------------
// <copyright file="AcoustIDSession.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using global::AcoustID;
    using global::AcoustID.Chromaprint;
    using Windows.Foundation;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// AcoustID session.
    /// </summary>
    public class AcoustIDSession : ISession
    {
        /// <summary>
        /// The client.
        /// </summary>
        private readonly AcoustIDClient client;

        /// <summary>
        /// The session options.
        /// </summary>
        private readonly SessionOptions options;

        /// <summary>
        /// The fingerprint tool.
        /// </summary>
        private ChromaContext context;

        /// <summary>
        /// The fingerprint.
        /// </summary>
        private string fingerprint;

        /// <summary>
        /// The duration of audio samples.
        /// </summary>
        private TimeSpan duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcoustIDSession"/> class.
        /// </summary>
        /// <param name="clientdata">The AcoustID client data.</param>
        /// <param name="httpFilter">An HTTP client filter.</param>
        /// <param name="options">Options for the session.</param>
        internal AcoustIDSession(AcoustIDClientIdData clientdata, IHttpFilter httpFilter, SessionOptions options)
            : this()
        {
            this.client = new AcoustIDClient(clientdata, httpFilter);
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcoustIDSession"/> class.
        /// </summary>
        private AcoustIDSession()
        {
            this.SessionIdentifier = Guid.NewGuid().ToString();
            this.fingerprint = null;
            this.duration = TimeSpan.Zero;
        }

        /// <inheritdoc/>
        public event StatusChangedEventHandler StatusChanged;

        /// <inheritdoc/>
        public string SessionIdentifier { get; private set; }

        /// <inheritdoc/>
        public IdentifyStatus IdentificationStatus { get; private set; }

        /// <inheritdoc/>
        public IAsyncOperation<IReadOnlyList<IReadOnlyTrack>> GetTracksAsync()
        {
            return this.GetTracksInternalAsync().AsAsyncOperation();
        }

        /// <inheritdoc/>
        public void AddAudioSample(byte[] audioData)
        {
            if (this.context == null)
            {
                this.context = new ChromaContext(ChromaprintAlgorithm.TEST2);
                this.context.Start(this.options.SampleRate, this.options.ChannelCount);
                this.UpdateStatus(IdentifyStatus.Incomplete);
            }

            if (this.IdentificationStatus != IdentifyStatus.Complete &&
                this.IdentificationStatus != IdentifyStatus.Error)
            {
                if (audioData == null)
                {
                    this.context.Finish();
                    try
                    {
                        this.fingerprint = this.context.GetFingerprint();
                    }
                    catch (NullReferenceException ex)
                    {
                        Debug.WriteLine($"AddAudioSample: GetFingerprint threw exception: {ex.Message}");
                    }

                    this.UpdateStatus(string.IsNullOrEmpty(this.fingerprint)
                        ? IdentifyStatus.Error
                        : IdentifyStatus.Complete);
                }
                else
                {
                    int shortLength = audioData.Length / 1;
                    short[] shortData = new short[shortLength];
                    for (int i = 0, k = 0; i < audioData.Length; i += 2, k++)
                    {
                        shortData[k] = BitConverter.ToInt16(audioData, i);
                    }

                    this.context.Feed(shortData, shortLength);

                    double milliseconds = 1000.0 * (double)shortLength / (double)this.options.SampleRate / (double)this.options.ChannelCount;
                    this.duration = this.duration.Add(TimeSpan.FromMilliseconds((int)milliseconds));
                }
            }
        }

        /// <summary>
        /// Update TrackIdState from SampleStatus.
        /// </summary>
        /// <param name="newStatus">The new status.</param>
        protected void UpdateStatus(IdentifyStatus newStatus)
        {
            // Update.
            bool changed = this.IdentificationStatus != newStatus;
            this.IdentificationStatus = newStatus;

            if (changed)
            {
                this.StatusChanged?.Invoke(this, new StatusChangedEventArgs(newStatus));
            }
        }

        /// <summary>
        /// Get the tracks from the fingerprint and duration.
        /// </summary>
        /// <returns>A collection of tracks.</returns>
        private async Task<IReadOnlyList<IReadOnlyTrack>> GetTracksInternalAsync()
        {
            List<IReadOnlyTrack> readOnlyTracks = new List<IReadOnlyTrack>();

            if (!string.IsNullOrEmpty(this.fingerprint))
            {
                do
                {
                    HttpRequestResult result = await this.client.QueryTrackIdAsync(this.fingerprint, this.duration).ConfigureAwait(false);
                    if (!(result?.Succeeded ?? false))
                    {
                        continue;
                    }

                    if (!(result?.ResponseMessage?.IsSuccessStatusCode ?? false))
                    {
                        continue;
                    }

                    string responseBody = await result.ResponseMessage.Content.ReadAsStringAsync();
                    AcoustIDTrackIdResponse trackId = await this.client.ParseTrackIdResponseAync(responseBody).ConfigureAwait(false);
                    if (trackId?.Code == 0)
                    {
                        result = await this.client.QueryTrackInfoAsync(trackId.Id).ConfigureAwait(false);
                        if (!(result?.Succeeded ?? false))
                        {
                            continue;
                        }

                        if (!(result?.ResponseMessage?.IsSuccessStatusCode ?? false))
                        {
                            continue;
                        }

                        responseBody = await result.ResponseMessage.Content.ReadAsStringAsync();
                        AcoustIDTrackResponse response = await this.client.ParseTrackInfoResponseAync(trackId, responseBody).ConfigureAwait(false);
                        if (response.Code == 0)
                        {
                            readOnlyTracks = response.Tracks.ToList();
                        }
                    }
                }
                while (false);
            }

            return readOnlyTracks;
        }
    }
}
