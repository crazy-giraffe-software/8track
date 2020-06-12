//-----------------------------------------------------------------------
// <copyright file="MockSession.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using Windows.Foundation;

    /// <summary>
    /// Mock session for identifying a song.
    /// </summary>
    public class MockSession : ISession
    {
        /// <summary>
        /// Number of frames needed to identify.
        /// </summary>
        private readonly int neededFrames;

        /// <summary>
        /// Gets or sets the resulting track count.
        /// </summary>
        private readonly int resultingTrackCount;

        /// <summary>
        /// The internal status for identifying tracks.
        /// </summary>
        private IdentifyStatus internalStatus;

        /// <summary>
        /// Number of frames added.
        /// </summary>
        private int addedFrames;

        /// <summary>
        /// Whether to fail the track attempt.
        /// </summary>
        private bool failTrackAttempt;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSession" /> class.
        /// </summary>
        /// <param name="options">the session options.</param>
        /// <param name="neededFrames">Number of frames needed to identify.</param>
        /// <param name="resultingTrackCount">The resulting track count.</param>
        /// <param name="failTrackAttempt">Whether to fail the track attempt.</param>
        internal MockSession(SessionOptions options, int neededFrames = 1, int resultingTrackCount = 1, bool failTrackAttempt = false)
        {
            _ = options;
            this.neededFrames = neededFrames;
            this.addedFrames = 0;
            this.failTrackAttempt = failTrackAttempt;
            this.resultingTrackCount = resultingTrackCount;
            this.internalStatus = IdentifyStatus.Invalid;
            this.IdentificationStatus = IdentifyStatus.Invalid;
            this.SessionIdentifier = Guid.NewGuid().ToString();
            this.Tracks = new List<IReadOnlyTrack>();
        }

        /// <inheritdoc/>
        public event StatusChangedEventHandler StatusChanged;

        /// <inheritdoc/>
        public string SessionIdentifier { get; private set; }

        /// <inheritdoc/>
        public IdentifyStatus IdentificationStatus { get; private set; }

        /// <summary>
        /// Gets a collection of tracks.
        /// </summary>
        public List<IReadOnlyTrack> Tracks { get; private set; }

        /// <inheritdoc/>
        public void AddAudioSample(byte[] audioData)
        {
            if (this.internalStatus == IdentifyStatus.Invalid)
            {
                this.internalStatus = IdentifyStatus.Incomplete;
                this.UpdateStatus(IdentifyStatus.Incomplete);
            }

            // If not complete, add sample data
            if (this.internalStatus == IdentifyStatus.Incomplete)
            {
                if (++this.addedFrames >= this.neededFrames)
                {
                    // Add tracks and identifier.
                    this.Tracks = new List<IReadOnlyTrack>(Enumerable.Repeat(MockTrack.CreateRandom(), this.resultingTrackCount));

                    // End the fingerprint session and get the track.
                    this.internalStatus = IdentifyStatus.Complete;
                    this.UpdateStatus(IdentifyStatus.Complete);
                }
            }
        }

        /// <summary>
        /// Sets a collection of tracks.
        /// </summary>
        /// <param name="tracks">The tracks to use.</param>
        public void SetTracks(IEnumerable<IReadOnlyTrack> tracks)
        {
            this.Tracks = new List<IReadOnlyTrack>(tracks);
        }

        /// <inheritdoc/>
        public IAsyncOperation<IReadOnlyList<IReadOnlyTrack>> GetTracksAsync()
        {
            if (this.failTrackAttempt)
            {
                this.failTrackAttempt = false;
                return Task.FromResult<IReadOnlyList<IReadOnlyTrack>>(null).AsAsyncOperation();
            }

            return Task.FromResult<IReadOnlyList<IReadOnlyTrack>>(this.Tracks).AsAsyncOperation();
        }

        /// <summary>
        /// Update TrackIdState from SampleStatus.
        /// </summary>
        /// <param name="newStatus">The new status.</param>
        public void UpdateStatus(IdentifyStatus newStatus)
        {
            // Update.
            bool changed = this.IdentificationStatus != newStatus;
            this.IdentificationStatus = newStatus;

            if (changed)
            {
                this.StatusChanged?.Invoke(this, new StatusChangedEventArgs(newStatus));
            }
        }
    }
}
