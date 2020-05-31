//-----------------------------------------------------------------------
// <copyright file="ACRCloudSessionTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ACRCloud.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using ACRCloudRecognitionTest;
    using CrazyGiraffe.AudioFrameProcessor;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.ACRCloud;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Foundation;
    using Windows.Media.MediaProperties;
    using Windows.Security.Cryptography;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// Tests for <see cref="ACRCloudSession"/>.
    /// </summary>
    [TestClass]
    public class ACRCloudSessionTests
    {
        /// <summary>
        /// Test the ability to create a <see cref="ACRCloudSession"/>.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ACRCloudSessionSuccess()
        {
            ISession session = await CreateSessionAsync().ConfigureAwait(true);
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
            ACRCloudExtrTool reference = new ACRCloudExtrTool();
            Assert.IsNotNull(reference, "reference");

            using (TestHttpFilter filter = new TestHttpFilter())
            using (HttpResponseMessage okResponse = new HttpResponseMessage(HttpStatusCode.Ok))
                using (HttpStringContent trackContent = new HttpStringContent(ACRCloudClientTests.GetCanonicalTrackResponse()))
            {
                okResponse.Content = trackContent;
                filter.Responses.Add(okResponse);

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

                uint blockSize = 1764; // 176400 bytes per second @ 44.1k, 2 channels, 16 bits per sample, or 1764 bytes per 10 ms.
                uint blocksCount = 12 * 100; // 12 seconds @ 10ms each.
                WrappedAudioFrame frame = WrappedAudioFrame.CreateRandom(blockSize * blocksCount);

                SessionOptions options = GetSessionOptions(audioSampleSize: 32);
                AudioEncodingProperties encodingProperties = AudioEncodingProperties.CreatePcm(options.SampleRate, options.ChannelCount, options.SampleSize);
                AudioFrameConverter converter = new AudioFrameConverter(encodingProperties);
                byte[] audioData = converter.ToByteArray(frame.CurrentFrame);

                byte[] fileHeader = GetFileHeader(options, blockSize * blocksCount);
                byte[] fileData = fileHeader.Concat(audioData).ToArray();

                byte[] referenceFingerprintBytes = reference.CreateFingerprintByFileBuffer(fileData, fileData.Length, 0, 12, false);
                string referenceFingerprint = CryptographicBuffer.EncodeToBase64String(
                    CryptographicBuffer.CreateFromByteArray(referenceFingerprintBytes));

                byte[] audioContent = audioData.ToArray();
                session.AddAudioSample(audioContent);

                Assert.IsTrue(completeStatusTaskCompletionSource.Task.Wait(5000), "Task.Wait");
                Assert.IsTrue(completeStatusTaskCompletionSource.Task.Result, "Task.Result");
                Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus, "session.IdentificationStatus");

                var tracks = await session.GetTracksAsync();
                Assert.AreNotEqual(0, tracks.Count, "tracks.Count");
            }
        }

        /// <summary>
        /// Test the ability to call AddAudioSample and ensure it buffers correctly..
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task AddAudioSampleBuffering()
        {
            // Create responses: no result, no result, valid result.
            using (HttpStringContent noResultContent = new HttpStringContent("{\"status\":{\"msg\":\"No result\",\"version\":\"1.0\",\"code\":1001}}"))
            using (HttpStringContent successResultContent = new HttpStringContent(ACRCloudClientTests.GetCanonicalTrackResponse()))
            using (TestHttpFilter filter = new TestHttpFilter())
            using (HttpResponseMessage noResultResponse = new HttpResponseMessage(HttpStatusCode.Ok) { Content = noResultContent, })
            using (HttpResponseMessage successResultResponse = new HttpResponseMessage(HttpStatusCode.Ok) { Content = successResultContent, })
            {
                filter.Responses.Add(noResultResponse);
                filter.Responses.Add(successResultResponse);

                List<DateTime> requestsReceievedAt = new List<DateTime>();
                filter.RequestReceived += (sender, e) =>
                {
                    requestsReceievedAt.Add(DateTime.UtcNow);
                };

                ISession session = await CreateSessionAsync(httpFilter: filter).ConfigureAwait(true);
                Assert.IsNotNull(session, "session");

                // Feed (fake) samples to the session.
                uint blockSize = 1764; // 176400 bytes per second @ 44.1k, 2 channels, 16 bits per sample, or 1764 bytes per 10 ms.
                WrappedAudioFrame frame = WrappedAudioFrame.CreateRandom(blockSize);

                SessionOptions options = GetSessionOptions(audioSampleSize: 32);
                AudioEncodingProperties encodingProperties = AudioEncodingProperties.CreatePcm(options.SampleRate, options.ChannelCount, options.SampleSize);
                AudioFrameConverter converter = new AudioFrameConverter(encodingProperties);
                byte[] fileDataBlock = converter.ToByteArray(frame.CurrentFrame);

                int blocksCount = 12 * 100; // 12 seconds @ 10ms each.
                for (int i = 0; i < blocksCount; i++)
                {
                    // Simulate real-time delivery of samples. This is to ensure
                    // the session send only the samples is needs to send in the alloted time.
                    session.AddAudioSample(fileDataBlock);
                    Thread.Sleep(10);
                }

                Assert.AreEqual(filter.Responses.Count, filter.Requests.Count, "filter.Requests.Count");
                Assert.AreEqual(filter.Responses.Count, requestsReceievedAt.Count, "requestsReceievedAt.Count");
            }
        }

        /// <summary>
        /// Create a new session using the default options.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="accessKey">Access Key.</param>
        /// <param name="accessSecret">Access Secret.</param>
        /// <param name="httpFilter">IHttpFilter.</param>
        /// <returns>ISession.</returns>
        private static async Task<ISession> CreateSessionAsync(
            string host = "host",
            string accessKey = "access_key",
            string accessSecret = "access_secret",
            IHttpFilter httpFilter = null)
        {
            ACRCloudClientIdData cientIdData = new ACRCloudClientIdData()
            {
                Host = host,
                AccessKey = accessKey,
                AccessSecret = accessSecret,
            };

            ISessionFactory factory = new ACRCloudSessionFactory(cientIdData, httpFilter);
            Assert.IsNotNull(factory, "factory");

            SessionOptions options = GetSessionOptions();
            Assert.IsNotNull(options, "options");

            ISession session = await factory.CreateSessionAsync(options);
            Assert.IsNotNull(session, "session");

            return session;
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
            ushort audioChannels = 2)
        {
            return new SessionOptions(audioSampleRate, audioSampleSize, audioChannels);
        }

        /// <summary>
        /// The a WAV file header.
        /// </summary>
        /// <param name="options">Audio options.</param>
        /// <param name="audioContentSize">Size of content.</param>
        /// <returns>A byte array with the WAV file header.</returns>
        private static byte[] GetFileHeader(SessionOptions options, uint audioContentSize)
        {
            List<byte> headerContent = new List<byte>();

            // Get the size of the content.
            uint subChunk2Size = audioContentSize;

            // From: http://soundfile.sapp.org/doc/WaveFormat/
            //
            // Offset  Size  Name             Description
            // 0       4     ChunkID          Contains the letters "RIFF" in ASCII form (0x52494646 big - endian form).
            headerContent.Add(0x52);
            headerContent.Add(0x49);
            headerContent.Add(0x46);
            headerContent.Add(0x46);

            // 4       4     ChunkSize        36 + SubChunk2Size, or more precisely : 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
            byte[] bytes = BitConverter.GetBytes((uint)(36 + subChunk2Size));
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);
            headerContent.Add(bytes[2]);
            headerContent.Add(bytes[3]);

            // 8       4     Format           Contains the letters "WAVE" (0x57415645 big - endian form).
            headerContent.Add(0x57);
            headerContent.Add(0x41);
            headerContent.Add(0x56);
            headerContent.Add(0x45);

            // 12      4     Subchunk1ID      Contains the letters "fmt " (0x666d7420 big - endian form).
            headerContent.Add(0x66);
            headerContent.Add(0x6d);
            headerContent.Add(0x74);
            headerContent.Add(0x20);

            // 16      4     Subchunk1Size    16 for PCM. This is the size of the rest of the Sub-chunk which follows this number.
            bytes = BitConverter.GetBytes(16U);
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);
            headerContent.Add(bytes[2]);
            headerContent.Add(bytes[3]);

            // 20      2     AudioFormat      PCM = 1 (i.e.Linear quantization) Values other than 1 indicate some form of compression.
            bytes = BitConverter.GetBytes((ushort)1);
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);

            // 22      2     NumChannels      Mono = 1, Stereo = 2, etc.
            bytes = BitConverter.GetBytes(options.ChannelCount);
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);

            // 24      4     SampleRate       8000, 44100, etc.
            bytes = BitConverter.GetBytes((uint)options.SampleRate);
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);
            headerContent.Add(bytes[2]);
            headerContent.Add(bytes[3]);

            // 28      4     ByteRate         SampleRate * NumChannels * BitsPerSample / 8
            bytes = BitConverter.GetBytes((uint)(options.SampleRate * options.ChannelCount * options.SampleSize / 8));
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);
            headerContent.Add(bytes[2]);
            headerContent.Add(bytes[3]);

            // 32      2     BlockAlign       NumChannels * BitsPerSample / 8. The number of bytes for one sample including all channels.
            bytes = BitConverter.GetBytes((ushort)(options.ChannelCount * options.SampleSize / 8));
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);

            // 34      2     BitsPerSample    8 bits = 8, 16 bits = 16, etc.
            bytes = BitConverter.GetBytes(options.SampleSize);
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);

            // 36      4     Subchunk2ID      Contains the letters "data" (0x64617461 big - endian form).
            headerContent.Add(0x64);
            headerContent.Add(0x61);
            headerContent.Add(0x74);
            headerContent.Add(0x61);

            // 40      4     Subchunk2Size    NumSamples * NumChannels * BitsPerSample / 8. This is the number of bytes in the data.
            bytes = BitConverter.GetBytes(subChunk2Size);
            headerContent.Add(bytes[0]);
            headerContent.Add(bytes[1]);
            headerContent.Add(bytes[2]);
            headerContent.Add(bytes[3]);

            return headerContent.ToArray();
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
