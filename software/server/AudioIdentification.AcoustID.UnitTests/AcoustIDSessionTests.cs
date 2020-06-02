// <copyright file="AcoustIDSessionTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioFrameProcessor;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.AcoustID;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Foundation;
    using Windows.Media.MediaProperties;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// A class to test <see cref="AcoustIDSession"/>.
    /// </summary>
    [TestClass]
    public class AcoustIDSessionTests
    {
        /// <summary>
        /// Test the ability to create a <see cref="AcoustIDSession"/>.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task AcoustIDSessionSuccess()
        {
            ISession session = await CreateSessionAsync().ConfigureAwait(false);
            Assert.IsNotNull(session, "session");
            Assert.IsNotNull(session.SessionIdentifier, "SessionIdentifier");
            Assert.AreEqual(IdentifyStatus.Invalid, session.IdentificationStatus, "IdentificationStatus");
        }

        /// <summary>
        /// Test the ability to call AddAudioSample and get a fingerprint.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task AddAudioSampleFingerprint()
        {
            using (TestHttpFilter filter = new TestHttpFilter())
            using (HttpResponseMessage okResponse1 = new HttpResponseMessage(HttpStatusCode.Ok))
            using (HttpResponseMessage okResponse2 = new HttpResponseMessage(HttpStatusCode.Ok))
            using (HttpStringContent trackIdContent = new HttpStringContent(AcoustIDClientTests.GetCanonicalTrackIdResponse()))
            using (HttpStringContent trackContent = new HttpStringContent(AcoustIDClientTests.GetCanonicalTrackResponse()))
            {
                okResponse1.Content = trackIdContent;
                okResponse2.Content = trackContent;
                filter.Responses.Add(okResponse1);
                filter.Responses.Add(okResponse2);

                ISession session = await CreateSessionAsync(httpFilter: filter).ConfigureAwait(false);
                Assert.IsNotNull(session, "session");

                var completeStatusTaskCompletionSource = new TaskCompletionSource<bool>();
                session.StatusChanged += (sender, e) =>
                {
                    if (e.Status == IdentifyStatus.Complete)
                    {
                        completeStatusTaskCompletionSource.TrySetResult(true);
                    }
                };

                WrappedAudioFrame frame = WrappedAudioFrame.CreateBlank(4096);
                SessionOptions options = GetSessionOptions();
                AudioEncodingProperties encodingProperties = AudioEncodingProperties.CreatePcm(options.SampleRate, options.ChannelCount, options.SampleSize);
                AudioFrameConverter converter = new AudioFrameConverter(encodingProperties);
                byte[] audioData = converter.ToByteArray(frame.CurrentFrame);

                for (int i = 0; i < 130; i++)
                {
                    session.AddAudioSample(audioData);
                }

                session.AddAudioSample(null);

                Assert.IsTrue(completeStatusTaskCompletionSource.Task.Wait(1000), "Task.Wait");
                Assert.IsTrue(completeStatusTaskCompletionSource.Task.Result, "Task.Result");
                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus, "session.IdentificationStatus");

                var tracks = await session.GetTracksAsync();
                Assert.AreNotEqual(0, tracks.Count, "tracks.Count");

                string request1 = filter.Requests[0].RequestUri.ToString();
                StringAssert.Contains(request1, "fingerprint=");
            }
        }

        /// <summary>
        /// Test the ability to call AddAudioSample with a null array.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task AddAudioSampleNullSample()
        {
            ISession session = await CreateSessionAsync().ConfigureAwait(true);
            Assert.IsNotNull(session, "session");
            session.AddAudioSample(null);
        }

        /// <summary>
        /// Get the session options.
        /// </summary>
        /// <param name="audioSampleRate">Sample rate.</param>
        /// <param name="audioSampleSize">Sample size.</param>
        /// <param name="audioChannels">Channel count.</param>
        /// <returns>SessionOptions.</returns>
        private static SessionOptions GetSessionOptions(
            ushort audioSampleRate = 44100,
            ushort audioSampleSize = 16,
            ushort audioChannels = 1)
        {
            return new SessionOptions(audioSampleRate, audioSampleSize, audioChannels);
        }

        /// <summary>
        /// Create a new session using the default options.
        /// </summary>
        /// <param name="apikey">APIKey.</param>
        /// <param name="httpFilter">IHttpFilter.</param>
        /// <returns>ISession.</returns>
        private static async Task<ISession> CreateSessionAsync(string apikey = "ClientKey", IHttpFilter httpFilter = null)
        {
            AcoustIDClientIdData clientdata = new AcoustIDClientIdData()
            {
                APIKey = apikey,
            };

            ISessionFactory factory = new AcoustIDSessionFactory(clientdata, httpFilter);
            Assert.IsNotNull(factory, "factory");

            SessionOptions options = GetSessionOptions();
            Assert.IsNotNull(options, "options");

            ISession session = await factory.CreateSessionAsync(options);
            Assert.IsNotNull(session, "session");

            return session;
        }

        /// <summary>
        /// A test IHttpFilter for capturing/redirecting HTTP requests.
        /// </summary>
        private class TestHttpFilter : IHttpFilter
        {
            /// <summary>
            /// An event that client can be use to be notified whenever
            /// pipeline is started to stopped.
            /// </summary>
            public event EventHandler<EventArgs> RequestReceived;

            /// <summary>
            /// Gets the most recent request received.
            /// </summary>
            public IList<HttpRequestMessage> Requests { get; private set; } = new List<HttpRequestMessage>();

            /// <summary>
            ///  Gets the responses to provide.
            /// </summary>
            public IList<HttpResponseMessage> Responses { get; } = new List<HttpResponseMessage>();

            /// <summary>
            ///  Gets or sets the responses to provide.
            /// </summary>
            private int ResponsesIndex { get; set; } = 0;

            /// <inheritdocs />
            public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
            {
                this.Requests.Add(request);
                this.RequestReceived?.Invoke(this, new EventArgs());
                return AsyncInfo.Run((CancellationToken cancellationToken, IProgress<HttpProgress> progress) =>
                {
                    progress.Report(default);

                    try
                    {
                        HttpResponseMessage response = this.Responses.Count > 0 ? this.Responses[this.ResponsesIndex] : null;
                        if (response != null)
                        {
                            response.RequestMessage = request;
                        }

                        if (this.Responses.Count > this.ResponsesIndex + 1)
                        {
                            this.ResponsesIndex++;
                        }

                        return Task.FromResult(response);
                    }
                    finally
                    {
                        progress.Report(default);
                    }
                });
            }

            /// <inheritdocs />
            public void Dispose()
            {
            }
        }
    }
}
