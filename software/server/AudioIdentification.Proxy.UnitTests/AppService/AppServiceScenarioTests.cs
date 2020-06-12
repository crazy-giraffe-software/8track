//-----------------------------------------------------------------------
// <copyright file="AppServiceScenarioTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.AppService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioFrameProcessor;
    using CrazyGiraffe.AudioIdentification.Proxy.UnitTests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Media;
    using Windows.Media.MediaProperties;

    /// <summary>
    /// Test class to test <see cref="Proxy.Session"/> through the app server to a mock session.
    /// </summary>
    [TestClass]
    public class AppServiceScenarioTests
    {
        /// <summary>
        /// Timeout for track id status.
        /// </summary>
        private const int TrackIdStatusTimeout = 15000;

        /// <summary>
        /// Test the ability to Id a song using the proxy classes (but in-proc).
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionProxyTest()
        {
            // start the session.  We know the format of our sample files.
            int neededFrames = 20;
            ISessionFactory backingService = new MockSessionFactory()
            {
                SupportMultipleSession = true,
                NeededFrames = neededFrames,
            };

            SessionOptions options = new SessionOptions(44100, 16, 2);

            using (InProcAppServiceClient client = new InProcAppServiceClient(backingService))
            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                // Connect an event handler for track completion.
                var trackIdTaskCompletionSource = new TaskCompletionSource<bool>();
                session.StatusChanged += (object sender, StatusChangedEventArgs eventArgs) =>
                {
                    if (session.IdentificationStatus == IdentifyStatus.Complete)
                    {
                        trackIdTaskCompletionSource.TrySetResult(true);
                    }
                };

                // Feed in some samples.
                Task sampleTask = Task.Run(() =>
                {
                    WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                    byte[] audioData = ToAudioData(frame.CurrentFrame, options);

                    for (int i = 0; i < neededFrames; i++)
                    {
                        session.AddAudioSample(audioData);
                    }
                });

                // Verify completion.
                await sampleTask.ConfigureAwait(false);
                Assert.IsTrue(trackIdTaskCompletionSource.Task.Wait(TrackIdStatusTimeout));
                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus);

                // Verify track info.
                IReadOnlyList<IReadOnlyTrack> tracks = await session.GetTracksAsync();
                Assert.AreEqual(1, tracks.Count);

                Assert.IsNotNull(tracks[0]);
                Assert.IsTrue(!string.IsNullOrEmpty(tracks[0].Title));
                Assert.IsTrue(!string.IsNullOrEmpty(tracks[0].Album));
                Assert.IsTrue(!string.IsNullOrEmpty(tracks[0].Artist));
            }
        }

        /// <summary>
        /// Test the ability to Id a song using the proxy classes (but in-proc).
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionMultipleTrackProxyTest()
        {
            // start the session.  We know the format of our sample files.
            int neededFrames = 20;
            int trackCount = 4;
            ISessionFactory backingService = new MockSessionFactory()
            {
                SupportMultipleSession = true,
                NeededFrames = neededFrames,
                ResultingTrackCount = trackCount,
            };

            SessionOptions options = new SessionOptions(44100, 16, 2);

            using (InProcAppServiceClient client = new InProcAppServiceClient(backingService))
            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                // Connect an event handler for track completion.
                var trackIdTaskCompletionSource = new TaskCompletionSource<bool>();
                session.StatusChanged += (object sender, StatusChangedEventArgs eventArgs) =>
                {
                    if (session.IdentificationStatus == IdentifyStatus.Complete)
                    {
                        trackIdTaskCompletionSource.TrySetResult(true);
                    }
                };

                // Feed in some samples.
                Task sampleTask = Task.Run(() =>
                {
                    WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                    byte[] audioData = ToAudioData(frame.CurrentFrame, options);

                    for (int i = 0; i < neededFrames; i++)
                    {
                        session.AddAudioSample(audioData);
                    }
                });

                // Verify completion.
                await sampleTask.ConfigureAwait(false);
                Assert.IsTrue(trackIdTaskCompletionSource.Task.Wait(TrackIdStatusTimeout));
                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus);

                // Verify track info.
                IReadOnlyList<IReadOnlyTrack> tracks = await session.GetTracksAsync();
                Assert.AreEqual(trackCount, tracks.Count);

                foreach (IReadOnlyTrack track in tracks)
                {
                    Assert.IsNotNull(track);
                    Assert.IsTrue(!string.IsNullOrEmpty(track.Title));
                    Assert.IsTrue(!string.IsNullOrEmpty(track.Album));
                    Assert.IsTrue(!string.IsNullOrEmpty(track.Artist));
                }
            }
        }

        /// <summary>
        /// Test the ability to Id a song using the proxy classes (but in-proc)
        /// and recover when it crashes.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionProxyRecoverTest()
        {
            // start the session.  We know the format of our sample files.
            int neededFrames = 10;
            ISessionFactory backingService = new MockSessionFactory()
            {
                SupportMultipleSession = true,
                NeededFrames = neededFrames,
            };

            SessionOptions options = new SessionOptions(44100, 16, 2);

            using (InProcAppServiceClient client = new InProcAppServiceClient(backingService, autoClose: true))
            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                // Connect an event handler for track completion.
                var trackIdTaskCompletionSource = new TaskCompletionSource<bool>();
                session.StatusChanged += (object sender, StatusChangedEventArgs eventArgs) =>
                {
                    if (session.IdentificationStatus == IdentifyStatus.Complete)
                    {
                        trackIdTaskCompletionSource.TrySetResult(true);
                    }
                };

                // Feed in some samples.
                Task sampleTask = Task.Run(() =>
                {
                    WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                    byte[] audioData = ToAudioData(frame.CurrentFrame, options);

                    // Send in more than the needed frames due to auto-close.
                    for (int i = 0; i < neededFrames * 2; i++)
                    {
                        session.AddAudioSample(audioData);
                    }
                });

                // Verify completion.
                await sampleTask.ConfigureAwait(false);
                Assert.IsTrue(trackIdTaskCompletionSource.Task.Wait(TrackIdStatusTimeout), "Event triggered");
                Assert.IsTrue(client.HasAutoClosed, "Auto-closed");
                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus);

                // Verify track info.
                IReadOnlyList<IReadOnlyTrack> tracks = await session.GetTracksAsync();
                Assert.AreEqual(1, tracks.Count);

                Assert.IsNotNull(tracks[0]);
                Assert.IsTrue(!string.IsNullOrEmpty(tracks[0].Title));
                Assert.IsTrue(!string.IsNullOrEmpty(tracks[0].Album));
                Assert.IsTrue(!string.IsNullOrEmpty(tracks[0].Artist));
            }
        }

        /// <summary>
        /// Convert an audio frame to a byte array.
        /// </summary>
        /// <param name="frame">The audio frame.</param>
        /// <param name="options">The session options.</param>
        /// <returns>The frame as a byte array.</returns>
        private static byte[] ToAudioData(AudioFrame frame, SessionOptions options)
        {
            if (frame == null || options == null)
            {
                return null;
            }

            AudioEncodingProperties encodingProperties = AudioEncodingProperties.CreatePcm(
                options.SampleRate,
                options.ChannelCount,
                options.SampleSize);

            AudioFrameConverter converter = new AudioFrameConverter(encodingProperties);
            return converter.ToByteArray(frame);
        }
    }
}
