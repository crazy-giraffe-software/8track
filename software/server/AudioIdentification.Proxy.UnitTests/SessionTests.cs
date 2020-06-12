//-----------------------------------------------------------------------
// <copyright file="SessionTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioFrameProcessor;
    using CrazyGiraffe.AudioIdentification.Proxy.AppService;
    using CrazyGiraffe.AudioIdentification.Proxy.UnitTests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Windows.Media;
    using Windows.Media.MediaProperties;

    /// <summary>
    /// Test class to test <see cref="Proxy.Session"/>.
    /// </summary>
    [TestClass]
    public class SessionTests
    {
        /// <summary>
        /// Timeout for track id status.
        /// </summary>
        private const int TrackIdStatusTimeout = 15000;

        /// <summary>
        /// Test the ability to create a proxy client.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionSuccess()
        {
            Mock<IAppServiceClient> client = new Mock<IAppServiceClient>();
            SessionOptions options = new SessionOptions(44100, 16, 2);

            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client.Object))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                Assert.IsNotNull(session, "session");
                Assert.IsNotNull(session.SessionIdentifier, "SessionIdentifier");
                Assert.IsFalse(session.SessionIdentifier.Contains("{", StringComparison.InvariantCulture), "{");
                Assert.IsFalse(session.SessionIdentifier.Contains("}", StringComparison.InvariantCulture), "}");
                Assert.AreEqual(IdentifyStatus.Invalid, session.IdentificationStatus, "IdentificationStatus");
            }
        }

        /// <summary>
        /// Test the ability to send samples through the proxy client.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionSendSamples()
        {
            Mock<IAppServiceClient> client = new Mock<IAppServiceClient>();
            SessionOptions options = new SessionOptions(44100, 16, 2);

            bool isOpen = false;
            client.Setup(x => x.IsOpen).Returns(isOpen);
            client.Setup(x => x.OpenAsync()).ReturnsAsync(true).Callback(() => isOpen = true);
            client.Setup(x => x.StartSessionAsync(It.IsAny<ISessionProxy>(), It.IsAny<SessionOptions>())).ReturnsAsync(Guid.NewGuid().ToString());
            client.Setup(x => x.SendAudioSampleAsync(It.IsAny<string>(), It.IsAny<byte[]>()));

            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client.Object))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                // Feed in some samples.
                Task sampleTask = Task.Run(() =>
                {
                    WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                    byte[] audioData = ToAudioData(frame.CurrentFrame, options);

                    for (int i = 0; i < 20; i++)
                    {
                        session.AddAudioSample(audioData);
                    }
                });

                await sampleTask.ConfigureAwait(false);
                Thread.Sleep(100);

                client.Verify(x => x.IsOpen, Times.AtLeastOnce());
                client.Verify(x => x.OpenAsync(), Times.AtLeastOnce());
                client.Verify(x => x.StartSessionAsync(It.IsAny<ISessionProxy>(), It.IsAny<SessionOptions>()), Times.AtLeastOnce());
                client.Verify(x => x.SendAudioSampleAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.AtLeastOnce());
            }
        }

        /// <summary>
        /// Test the ability to return tracks to the proxy client.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionProcessTracks()
        {
            Mock<IAppServiceClient> client = new Mock<IAppServiceClient>();
            SessionOptions options = new SessionOptions(44100, 16, 2);

            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client.Object))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                Proxy.Session proxySession = session as Proxy.Session;
                Assert.IsNotNull(proxySession);

                IReadOnlyTrack mockTrack = MockTrack.CreateRandom();
                IEnumerable<IReadOnlyTrack> tracks = new List<IReadOnlyTrack>(new[] { mockTrack, mockTrack, mockTrack });
                proxySession.ProcessTrackResponse(IdentifyStatus.Complete, tracks);

                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus, "IdentificationStatus");

                // Verify track info.
                IEnumerable<IReadOnlyTrack> proxyTracks = await proxySession.GetTracksAsync();
                Assert.AreEqual(tracks.Count(), proxyTracks.Count(), "proxyTracks.Count()");
            }
        }

        /// <summary>
        /// Test the ability to return tracks to generate events.
        /// and recover when it crashes.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionStatusChanged()
        {
            Mock<IAppServiceClient> client = new Mock<IAppServiceClient>();
            SessionOptions options = new SessionOptions(44100, 16, 2);

            using (Proxy.SessionFactory sessionFactory = new Proxy.SessionFactory(client.Object))
            {
                ISession session = await sessionFactory.CreateSessionAsync(options);
                Assert.IsNotNull(session);

                Proxy.Session proxySession = session as Proxy.Session;
                Assert.IsNotNull(proxySession);

                // Connect an event handler for track completion.
                var trackIdTaskIncompleteCompletionSource = new TaskCompletionSource<bool>();
                var trackIdTaskCompleteCompletionSource = new TaskCompletionSource<bool>();
                session.StatusChanged += (object sender, StatusChangedEventArgs eventArgs) =>
                {
                    if (session.IdentificationStatus == IdentifyStatus.Incomplete)
                    {
                        trackIdTaskIncompleteCompletionSource.TrySetResult(true);
                    }
                    else if (session.IdentificationStatus == IdentifyStatus.Complete)
                    {
                        trackIdTaskCompleteCompletionSource.TrySetResult(true);
                    }
                };

                Assert.AreEqual(IdentifyStatus.Invalid, session.IdentificationStatus);
                proxySession.ProcessTrackResponse(IdentifyStatus.Incomplete, new List<IReadOnlyTrack>());

                // Verify incompletion.
                Assert.IsTrue(trackIdTaskIncompleteCompletionSource.Task.Wait(TrackIdStatusTimeout), "Event triggered (1)");
                Assert.AreEqual(IdentifyStatus.Incomplete, session.IdentificationStatus);

                IReadOnlyTrack mockTrack = MockTrack.CreateRandom();
                IEnumerable<IReadOnlyTrack> tracks = new List<IReadOnlyTrack>(new[] { mockTrack, mockTrack, mockTrack });
                proxySession.ProcessTrackResponse(IdentifyStatus.Complete, tracks);

                // Verify completion.
                Assert.IsTrue(trackIdTaskCompleteCompletionSource.Task.Wait(TrackIdStatusTimeout), "Event triggered (2)");
                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus);
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
