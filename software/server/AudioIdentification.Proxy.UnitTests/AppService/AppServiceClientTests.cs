//-----------------------------------------------------------------------
// <copyright file="AppServiceClientTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.AppService
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioFrameProcessor;
    using CrazyGiraffe.AudioIdentification.Proxy.AppService;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Windows.ApplicationModel.AppService;
    using Windows.Foundation.Collections;
    using Windows.Media;
    using Windows.Media.MediaProperties;

    /// <summary>
    /// Test class to test <see cref="AppServiceClient"/>.
    /// </summary>
    [TestClass]
    public class AppServiceClientTests
    {
        /// <summary>
        /// Timeout for track id status.
        /// </summary>
        private const int TrackIdStatusTimeout = 15000;

        /// <summary>
        /// Test the ability to create an AppServiceClient.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task ClientCreate()
        {
            using (AppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call start session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task StartSessionSuccess()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                string receivedSessionId = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.StartSessionCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.OptionsKey));

                Assert.AreEqual(sessionId, receivedSessionId);
                Assert.IsTrue(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call start session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task StartSessionFailure()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusFail,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                string receivedSessionId = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.StartSessionCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.OptionsKey));

                Assert.IsTrue(string.IsNullOrEmpty(receivedSessionId));
                Assert.IsFalse(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call start session which returns a duplicate session id.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task StartSessionDuplicateSession()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                string receivedSessionId1 = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);
                string receivedSessionId2 = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.StartSessionCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.OptionsKey));

                Assert.AreEqual(sessionId, receivedSessionId1);
                Assert.AreEqual(sessionId, receivedSessionId2);
                Assert.IsTrue(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call start session with invalid arguments.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task StartSessionInvalidArgs()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                _ = await client.StartSessionAsync(null, options).ConfigureAwait(false);
                _ = await client.StartSessionAsync(mockSesssionProxy.Object, null).ConfigureAwait(false);
                Assert.IsTrue(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call send audio sample.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SendAudioSampleSuccess()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                _ = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                };

                WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                byte[] audioData = ToAudioData(frame.CurrentFrame, options);
                await client.SendAudioSampleAsync(sessionId, audioData).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.SampleCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.SessionIdKey));
                Assert.AreEqual(sessionId, client.Request[AppServiceServer.SessionIdKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.SampleKey));
            }
        }

        /// <summary>
        /// Test the ability to call send audio sample.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SendAudioSampleFailure()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                _ = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusFail,
                };

                WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                byte[] audioData = ToAudioData(frame.CurrentFrame, options);
                await client.SendAudioSampleAsync(sessionId, audioData).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.SampleCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.SessionIdKey));
                Assert.AreEqual(sessionId, client.Request[AppServiceServer.SessionIdKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.SampleKey));

                Assert.IsFalse(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call send audio sample with invalid arguments.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SendAudioSampleInvalidArgs()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                _ = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                };

                WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(0.1f);
                byte[] audioData = ToAudioData(frame.CurrentFrame, options);
                await client.SendAudioSampleAsync(null, audioData).ConfigureAwait(false);
                await client.SendAudioSampleAsync(sessionId, null).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Test the ability to call end session.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task EndSession()
        {
            using (TestableAppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                SessionOptions options = new SessionOptions(44100, 16, 2);
                Mock<ISessionProxy> mockSesssionProxy = new Mock<ISessionProxy>();
                string receivedSessionId = await client.StartSessionAsync(mockSesssionProxy.Object, options).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.StartSessionCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.OptionsKey));

                Assert.AreEqual(sessionId, receivedSessionId);
                Assert.IsTrue(client.IsOpen);

                client.Response = new ValueSet
                {
                    [AppServiceServer.CommandStatusKey] = AppServiceServer.CommandStatusOK,
                    [AppServiceServer.SessionIdKey] = sessionId,
                };

                await client.EndSessionAsync(sessionId).ConfigureAwait(false);

                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.CommandKey));
                Assert.AreEqual(AppServiceServer.EndSessionCommand, client.Request[AppServiceServer.CommandKey]);
                Assert.IsTrue(client.Request.Keys.Contains(AppServiceServer.SessionIdKey));
                Assert.AreEqual(sessionId, client.Request[AppServiceServer.SessionIdKey]);

                Assert.IsFalse(client.IsOpen);
            }
        }

        /// <summary>
        /// Test the ability to call end session with invalid arguments.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task EndSessionInvalidArgs()
        {
            using (AppServiceClient client = new TestableAppServiceClient())
            {
                Assert.IsNotNull(client);

                Assert.IsTrue(await client.OpenAsync().ConfigureAwait(false));
                Assert.IsTrue(client.IsOpen);

                string sessionId = Guid.NewGuid().ToString();
                await client.EndSessionAsync(null).ConfigureAwait(false);
                await client.EndSessionAsync(string.Empty).ConfigureAwait(false);
                Assert.IsTrue(client.IsOpen);
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

        /// <summary>
        /// A testable version of AppServiceClient.
        /// </summary>
        private class TestableAppServiceClient : AppServiceClient
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestableAppServiceClient" /> class.
            /// </summary>
            public TestableAppServiceClient()
            {
            }

            /// <summary>
            /// Gets or sets the request received.
            /// </summary>
            public ValueSet Request { get; set; }

            /// <summary>
            /// Gets or sets the response to SendMessageAsync.
            /// </summary>
            public ValueSet Response { get; set; }

            /// <summary>
            /// Check for error response.
            /// </summary>
            /// <param name="message">The message to check.</param>
            /// <returns>True if the message is an error.</returns>
            public static bool TestMessageContainsErrors(ValueSet message)
            {
                return MessageContainsErrors(message);
            }

            /// <inheritdoc/>
            public override Task<bool> OpenAsync()
            {
                // Create it but it's not used.
                this.ServiceConnection = new AppServiceConnection();

                return Task.FromResult(true);
            }

            /// <inheritdoc/>
            public override void Close()
            {
                this.ServiceConnection = null;
            }

            /// <summary>
            /// Service connection send a response.
            /// </summary>
            /// <param name="message">The response message.</param>
            /// <returns>The client response.</returns>
            public ValueSet TestProcessRequestReceivedAsync(ValueSet message)
            {
                return this.ProcessRequestReceivedAsync(message);
            }

            /// <inheritdoc/>
            protected override Task<ValueSet> SendMessageAsync(ValueSet message)
            {
                this.Request = message;
                return Task.FromResult(this.Response);
            }
        }
    }
}
