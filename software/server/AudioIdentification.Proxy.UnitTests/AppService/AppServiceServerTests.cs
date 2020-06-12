//-----------------------------------------------------------------------
// <copyright file="AppServiceServerTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.AppService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioFrameProcessor;
    using CrazyGiraffe.AudioIdentification.Proxy.AppService;
    using CrazyGiraffe.AudioIdentification.Proxy.UnitTests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Windows.Foundation.Collections;
    using Windows.Media;
    using Windows.Media.MediaProperties;

    /// <summary>
    /// Test class to test <see cref="AppServiceServer"/>.
    /// </summary>
    [TestClass]
    public class AppServiceServerTests
    {
        /// <summary>
        /// Test the ability create an AppServiceServer.
        /// </summary>
        [TestMethod]
        public void AppServiceTest()
        {
            // Create an AppServiceServer.
            ISessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Verify the properties.
            Assert.AreEqual(0, appService.ActiveSessionCount);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to start a session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceStartSessionCommandTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start a session.
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet response = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(response.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, response[AppServiceServer.CommandStatusKey]);

            // Verify the session.
            Assert.AreEqual(1, appService.ActiveSessionCount);
            Assert.IsTrue(response.ContainsKey(AppServiceServer.SessionIdKey));
            Assert.AreEqual(sessionFactory.MockSession.SessionIdentifier, response[AppServiceServer.SessionIdKey]);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to end a session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceEndSessionCommandTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start a session.
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet startSessionResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startSessionResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startSessionResponse[AppServiceServer.CommandStatusKey]);

            // Verify the session.
            Assert.AreEqual(1, appService.ActiveSessionCount);
            Assert.AreEqual(sessionFactory.MockSession.SessionIdentifier, startSessionResponse[AppServiceServer.SessionIdKey]);

            // End the session.
            ValueSet endSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.EndSessionCommand },
                { AppServiceServer.SessionIdKey, sessionFactory.MockSession.SessionIdentifier },
            };

            ValueSet endSessionResponse = await appService.ProcessRequestAsync(endSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(endSessionResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, endSessionResponse[AppServiceServer.CommandStatusKey]);

            // Verify the session.
            Assert.AreEqual(0, appService.ActiveSessionCount);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to end a session with an invalid id.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceEndSessionCommandInvalidIdTest()
        {
            // Create an AppServiceServer.
            ISessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // End an invalid a session.
            ValueSet endSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.EndSessionCommand },
                { AppServiceServer.SessionIdKey, Guid.NewGuid().ToString() },
            };

            ValueSet endSessionResponse = await appService.ProcessRequestAsync(endSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(endSessionResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusFail, endSessionResponse[AppServiceServer.CommandStatusKey]);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to start a session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceSampleCommandTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start a session.
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);

            // Send a sample.
            WrappedAudioFrame audioFrame = WrappedAudioFrame.CreateFixed(0.1f);
            ValueSet sendSampleMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.SampleCommand },
                { AppServiceServer.SessionIdKey, sessionFactory.MockSession.SessionIdentifier },
                { AppServiceServer.SampleKey, ToAudioData(audioFrame.CurrentFrame, options) },
            };

            ValueSet sendSampleResponse = await appService.ProcessRequestAsync(sendSampleMessage).ConfigureAwait(false);
            Assert.IsTrue(sendSampleResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, sendSampleResponse[AppServiceServer.CommandStatusKey]);

            // Verify the session.
            Assert.AreEqual(1, appService.ActiveSessionCount);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to fire an event.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceEventTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start a session.
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);

            // Attach an event.
            ValueSet receivedMessage = null;
            appService.ResponseAvailable += (object s, ResponseAvailableEventArgs e) =>
            {
                receivedMessage = e.Response;
            };

            // Raise an event and verify.
            sessionFactory.MockSession.UpdateStatus(IdentifyStatus.Incomplete);
            Assert.IsNotNull(receivedMessage);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.CommandKey));
            Assert.AreEqual(AppServiceServer.StatusCommand, receivedMessage[AppServiceServer.CommandKey]);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.SessionIdKey));
            Assert.AreEqual(sessionFactory.MockSession.SessionIdentifier, receivedMessage[AppServiceServer.SessionIdKey]);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.StatusKey));
            Assert.AreEqual((int)IdentifyStatus.Incomplete, receivedMessage[AppServiceServer.StatusKey]);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to send the track response.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceTrackResponseTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start a session.
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);

            // Attach an event.
            ValueSet receivedMessage = null;
            appService.ResponseAvailable += (object s, ResponseAvailableEventArgs e) =>
            {
                receivedMessage = e.Response;
            };

            // Raise an event.
            IReadOnlyTrack mockTrack = MockTrack.CreateRandom();
            sessionFactory.MockSession.SetTracks(new List<IReadOnlyTrack>(new[] { mockTrack, mockTrack, mockTrack }));
            sessionFactory.MockSession.UpdateStatus(IdentifyStatus.Complete);

            // Verify response.
            Assert.IsNotNull(receivedMessage);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.CommandKey));
            Assert.AreEqual(AppServiceServer.StatusCommand, receivedMessage[AppServiceServer.CommandKey]);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.SessionIdKey));
            Assert.AreEqual(sessionFactory.MockSession.SessionIdentifier, receivedMessage[AppServiceServer.SessionIdKey]);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.StatusKey));
            Assert.AreEqual((int)IdentifyStatus.Complete, receivedMessage[AppServiceServer.StatusKey]);
            Assert.IsTrue(receivedMessage.ContainsKey(AppServiceServer.TrackCountKey));
            Assert.AreEqual(sessionFactory.MockSession.Tracks.Count.ToString(CultureInfo.InvariantCulture), receivedMessage[AppServiceServer.TrackCountKey]);

            // Check each track's fingerprint.
            for (int i = 0; i < sessionFactory.MockSession.Tracks.Count; i++)
            {
                string trackKey = string.Format(CultureInfo.InvariantCulture, AppServiceServer.TrackKeyFormat, i);
                Assert.IsTrue(receivedMessage.ContainsKey(trackKey));
                Assert.IsNotNull(receivedMessage[trackKey]);

                IReadOnlyTrack receivedTrack = JsonConvert.DeserializeObject<Track>(receivedMessage[trackKey] as string);
                Assert.AreEqual(mockTrack.Identifier, mockTrack.Identifier);
            }
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to end a session with an invalid id.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceInvalidCommandTest()
        {
            // Create an AppServiceServer.
            ISessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Send an invalid command.
            ValueSet invalidMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, Guid.NewGuid().ToString() },
            };

            ValueSet invalidResponse = await appService.ProcessRequestAsync(invalidMessage).ConfigureAwait(false);
            Assert.IsTrue(invalidResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusFail, invalidResponse[AppServiceServer.CommandStatusKey]);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to start a session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceMultipleSessionTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start multiple session.
            sessionFactory.SupportMultipleSession = true;
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.SessionIdKey));
            var session1Id = startResponse[AppServiceServer.SessionIdKey];

            startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.SessionIdKey));
            var session2Id = startResponse[AppServiceServer.SessionIdKey];

            startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.SessionIdKey));
            var session3Id = startResponse[AppServiceServer.SessionIdKey];

            // Verify the sessions.
            Assert.AreEqual(3, appService.ActiveSessionCount);
            Assert.IsNotNull(session1Id);
            Assert.IsNotNull(session2Id);
            Assert.IsNotNull(session3Id);

            // Send a sample to each session.
            WrappedAudioFrame audioFrame = WrappedAudioFrame.CreateFixed(0.1f);
            ValueSet sendSampleMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.SampleCommand },
                { AppServiceServer.SampleKey, ToAudioData(audioFrame.CurrentFrame, options) },
            };

            sendSampleMessage[AppServiceServer.SessionIdKey] = session1Id;
            ValueSet sendSampleResponse1 = await appService.ProcessRequestAsync(sendSampleMessage).ConfigureAwait(false);
            Assert.IsTrue(sendSampleResponse1.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, sendSampleResponse1[AppServiceServer.CommandStatusKey]);

            sendSampleMessage[AppServiceServer.SessionIdKey] = session2Id;
            ValueSet sendSampleResponse2 = await appService.ProcessRequestAsync(sendSampleMessage).ConfigureAwait(false);
            Assert.IsTrue(sendSampleResponse1.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, sendSampleResponse2[AppServiceServer.CommandStatusKey]);

            sendSampleMessage[AppServiceServer.SessionIdKey] = session3Id;
            ValueSet sendSampleResponse3 = await appService.ProcessRequestAsync(sendSampleMessage).ConfigureAwait(false);
            Assert.IsTrue(sendSampleResponse1.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, sendSampleResponse3[AppServiceServer.CommandStatusKey]);
        }

        /// <summary>
        /// Test the ability of the AppServiceServer to start a session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AppServiceMultipleSessionDisposeTest()
        {
            // Create an AppServiceServer.
            SessionOptions options = new SessionOptions();
            MockSessionFactory sessionFactory = new MockSessionFactory();
            AppServiceServer appService = new AppServiceServer(sessionFactory);

            // Start multiple session.
            sessionFactory.SupportMultipleSession = true;
            ValueSet startSessionMessage = new ValueSet
            {
                { AppServiceServer.CommandKey, AppServiceServer.StartSessionCommand },
                { AppServiceServer.OptionsKey, JsonConvert.SerializeObject(options) },
            };

            ValueSet startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.SessionIdKey));
            var session1Id = startResponse[AppServiceServer.SessionIdKey];

            startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.SessionIdKey));
            var session2Id = startResponse[AppServiceServer.SessionIdKey];

            startResponse = await appService.ProcessRequestAsync(startSessionMessage).ConfigureAwait(false);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.CommandStatusKey));
            Assert.AreEqual(AppServiceServer.CommandStatusOK, startResponse[AppServiceServer.CommandStatusKey]);
            Assert.IsTrue(startResponse.ContainsKey(AppServiceServer.SessionIdKey));
            var session3Id = startResponse[AppServiceServer.SessionIdKey];

            // Verify the sessions.
            Assert.AreEqual(3, appService.ActiveSessionCount);
            Assert.IsNotNull(session1Id);
            Assert.IsNotNull(session2Id);
            Assert.IsNotNull(session3Id);

            // Dispose of the service.
            appService.Dispose();
            Assert.AreEqual(0, appService.ActiveSessionCount);
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
