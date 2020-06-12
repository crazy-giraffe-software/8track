//-----------------------------------------------------------------------
// <copyright file="MockTrack.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.Mocks
{
    using System;
    using CrazyGiraffe.AudioIdentification;

    /// <summary>
    /// Mock Track info for a matched song.
    /// </summary>
    internal class MockTrack : IReadOnlyTrack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockTrack" /> class.
        /// </summary>
        public MockTrack()
        {
            this.Identifier = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the identifier of the track.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the title of the track.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the name of the artist for the track.
        /// </summary>
        public string Artist { get; private set; }

        /// <summary>
        /// Gets the title of the track's album.
        /// </summary>
        public string Album { get; private set; }

        /// <summary>
        /// Gets the genre of the track.
        /// </summary>
        public string Genre { get; private set; }

        /// <summary>
        /// Gets the cover art Uri.
        /// </summary>
        public Uri CovertArtImage { get; private set; }

        /// <summary>
        /// Gets the match confidence of the track.
        /// </summary>
        public string MatchConfidence { get; private set; }

        /// <summary>
        /// Gets the duration of the track in milliseconds.
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        /// Gets the match position of the track in milliseconds.
        /// </summary>
        public int MatchPosition { get; private set; }

        /// <summary>
        /// Gets the current position of the track in milliseconds.
        /// </summary>
        public int CurrentPosition { get; private set; }

        /// <summary>
        /// Create a track with random data.
        /// </summary>
        /// <returns>A track with random data.</returns>
        public static IReadOnlyTrack CreateRandom()
        {
            Random random = new Random();
            int duration = random.Next(100000);

            MockTrack track = new MockTrack
            {
                Title = Guid.NewGuid().ToString(),
                Artist = Guid.NewGuid().ToString(),
                Album = Guid.NewGuid().ToString(),
                Genre = Guid.NewGuid().ToString(),
                CovertArtImage = GetTestJpgUri(),
                MatchConfidence = Guid.NewGuid().ToString(),
                Duration = duration,
                MatchPosition = random.Next(duration),
                CurrentPosition = random.Next(duration),
            };

            return track;
        }

        /// <summary>
        /// Get a test Jpg file as a storage file.
        /// </summary>
        /// <returns>An Uri pointing to a local jpg image.</returns>
        private static Uri GetTestJpgUri()
        {
            return new Uri("ms-appx:///Media/FunnyPic.jpg");
        }
    }
}
