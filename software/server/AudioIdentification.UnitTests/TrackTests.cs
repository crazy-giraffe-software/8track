//-----------------------------------------------------------------------
// <copyright file="TrackTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.UnitTests
{
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.UnitTests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// Test class to test <see cref="Track"/>.
    /// </summary>
    [TestClass]
    public class TrackTests
    {
        /// <summary>
        /// Test the ability create an <see cref="Track"/>.
        /// </summary>
        [TestMethod]
        public void TrackSuccess()
        {
            IReadOnlyTrack track = MockTrack.CreateRandom();
            Assert.IsNotNull(track, "track");

            Track newTrack = new Track()
            {
                Album = track.Album,
                Artist = track.Artist,
                CovertArtImage = track.CovertArtImage,
                CurrentPosition = track.CurrentPosition,
                Duration = track.Duration,
                Identifier = track.Identifier,
                Genre = track.Genre,
                MatchPosition = track.MatchPosition,
                MatchConfidence = track.MatchConfidence,
                Title = track.Title,
            };

            Assert.IsNotNull(newTrack, "newTrack");
            Assert.AreEqual(track.Album, newTrack.Album, "Album");
            Assert.AreEqual(track.Artist, newTrack.Artist, "Artist");
            Assert.AreEqual(track.CovertArtImage, newTrack.CovertArtImage, "CovertArtImage");
            Assert.AreEqual(track.CurrentPosition, newTrack.CurrentPosition, "CurrentPosition");
            Assert.AreEqual(track.Duration, newTrack.Duration, "Duration");
            Assert.AreEqual(track.Identifier, newTrack.Identifier, "Identifier");
            Assert.AreEqual(track.Genre, newTrack.Genre, "Genre");
            Assert.AreEqual(track.MatchPosition, newTrack.MatchPosition, "MatchPosition");
            Assert.AreEqual(track.MatchConfidence, newTrack.MatchConfidence, "MatchConfidence");
            Assert.AreEqual(track.Title, newTrack.Title, "Title");
        }

        /// <summary>
        /// Test the ability create an <see cref="Track"/> via serialization.
        /// </summary>
        [TestMethod]
        public void TrackSerialization()
        {
            IReadOnlyTrack track = MockTrack.CreateRandom();
            Assert.IsNotNull(track, "track");

            string asJson = JsonConvert.SerializeObject(track);
            Assert.IsNotNull(asJson, "asJson");

            Track newTrack = JsonConvert.DeserializeObject<Track>(asJson);
            Assert.IsNotNull(newTrack, "newTrack");

            Assert.AreEqual(track.Album, newTrack.Album, "Album");
            Assert.AreEqual(track.Artist, newTrack.Artist, "Artist");
            Assert.AreEqual(track.CovertArtImage, newTrack.CovertArtImage, "CovertArtImage");
            Assert.AreEqual(track.CurrentPosition, newTrack.CurrentPosition, "CurrentPosition");
            Assert.AreEqual(track.Duration, newTrack.Duration, "Duration");
            Assert.AreEqual(track.Identifier, newTrack.Identifier, "Identifier");
            Assert.AreEqual(track.Genre, newTrack.Genre, "Genre");
            Assert.AreEqual(track.MatchPosition, newTrack.MatchPosition, "MatchPosition");
            Assert.AreEqual(track.MatchConfidence, newTrack.MatchConfidence, "MatchConfidence");
            Assert.AreEqual(track.Title, newTrack.Title, "Title");
        }
    }
}
