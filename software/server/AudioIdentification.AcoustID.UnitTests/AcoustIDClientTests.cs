//-----------------------------------------------------------------------
// <copyright file="AcoustIDClientTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.AcoustID;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Foundation;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// Tests for <see cref="AcoustIDClient"/>.
    /// </summary>
    [TestClass]
    public class AcoustIDClientTests
    {
        /// <summary>
        /// The example string from: https://acoustid.org/webservice.
        /// </summary>
        /// <returns>A canonical track response.</returns>
        public static string GetCanonicalTrackIdResponse()
        {
            return CanonicalTrackIdResponse;
        }

        /// <summary>
        /// The example string from: https://musicbrainz.org/doc/Development/JSON_Web_Service.
        /// </summary>
        /// <returns>A canonical track response.</returns>
        public static string GetCanonicalTrackResponse()
        {
            return CanonicalTrackResponse;
        }

        /// <summary>
        /// Test the ability to call QueryTrackIdAsync.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task QueryTrackIdAsyncSuccess()
        {
            using (TestHttpFilter filter = new TestHttpFilter())
            using (HttpResponseMessage emptyResponse = new HttpResponseMessage(HttpStatusCode.Ok) { Content = new HttpStringContent(string.Empty) })
            {
                string apikey = "apikey";
                AcoustIDClient client = this.CreateClient(apikey, filter);
                filter.Response = emptyResponse;

                string fingerprint = "fingerprint";
                int durationSeconds = 120;
                HttpRequestResult result = await client.QueryTrackIdAsync(fingerprint, TimeSpan.FromSeconds(durationSeconds)).ConfigureAwait(false);
                Assert.IsNotNull(result, "result");

                Assert.IsNotNull(filter.Request, "filter.Request");
                Assert.IsNotNull(filter.Request, "filter.Request");
                Assert.AreEqual(HttpMethod.Get.Method, filter.Request.Method.Method, "filter.Request.Method");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), "https://api.acoustid.org/v2/lookup", "inc");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), $"client={apikey}", "apikey");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), $"fingerprint={fingerprint}", "fingerprint");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), $"duration={durationSeconds}", "duration");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), "format=json", "format");
            }
        }

        /// <summary>
        /// Test the ability to call QueryTrackIdAsync with invalid argument.
        /// </summary>
        [TestMethod]
        public void QueryTrackIdAsyncInvalidArgument()
        {
            AcoustIDClient client = this.CreateClient();
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.QueryTrackIdAsync(string.Empty, TimeSpan.Zero).ConfigureAwait(true));
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.QueryTrackIdAsync(string.Empty, TimeSpan.Zero).ConfigureAwait(true));
        }

        /// <summary>
        /// Test the ability to call QueryTrackInfoAsync.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task QueryTrackInfoAsyncSuccess()
        {
            using (TestHttpFilter filter = new TestHttpFilter())
            using (HttpResponseMessage emptyResponse = new HttpResponseMessage(HttpStatusCode.Ok) { Content = new HttpStringContent(string.Empty) })
            {
                string apikey = "apikey";
                AcoustIDClient client = this.CreateClient(apikey, filter);
                filter.Response = emptyResponse;

                string id = Guid.NewGuid().ToString();
                HttpRequestResult result = await client.QueryTrackInfoAsync(id).ConfigureAwait(false);
                Assert.IsNotNull(result, "result");

                Assert.IsNotNull(filter.Request, "filter.Request");
                Assert.IsNotNull(filter.Request, "filter.Request");
                Assert.AreEqual(HttpMethod.Get.Method, filter.Request.Method.Method, "filter.Request.Method");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), "https://musicbrainz.org/ws/2/recording/", "inc");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), $"{id}", "id");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), $"inc=artist-credits%2breleases%2bgenres", "inc");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), "fmt=json", "fmt");
            }
        }

        /// <summary>
        /// Test the ability to call QueryTrackInfoAsync with invalid arguments.
        /// </summary>
        [TestMethod]
        public void QueryTrackInfoAsyncInvalidArgument()
        {
            AcoustIDClient client = this.CreateClient();
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.QueryTrackInfoAsync(null).ConfigureAwait(false));
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.QueryTrackInfoAsync(string.Empty).ConfigureAwait(false));
        }

        /// <summary>
        /// Test the ability to call ParseTrackIdResponseAync with the status missing.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackIdResponseAyncMissingStatus()
        {
            AcoustIDClient client = this.CreateClient();

            string content = "{ \"results\":[{ \"id\": \"9ff43b6a-4f16-427c-93c2-92307ca505e0\", \"score\": 1.0 }] }";
            AcoustIDTrackIdResponse response = await client.ParseTrackIdResponseAync(content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(-1, response.Code, "response.Code");
            Assert.IsNull(response.Id, "response.Id");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a bad status code.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackIdResponseAyncErrorStatus()
        {
            AcoustIDClient client = this.CreateClient();

            string content = "{ \"status\": \"error\", \"error\": { \"code\": 4, \"message\": \"invalid API key\" } }";
            AcoustIDTrackIdResponse response = await client.ParseTrackIdResponseAync(content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(4, response.Code, "response.Code");
            Assert.IsNull(response.Id, "response.Id");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with missing results.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackIdResponseAyncMissingResults()
        {
            AcoustIDClient client = this.CreateClient();

            string content = "{ \"status\": \"ok\" }";
            AcoustIDTrackIdResponse response = await client.ParseTrackIdResponseAync(content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(-1, response.Code, "response.Code");
            Assert.IsNull(response.Id, "response.Id");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with missing results.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackIdResponseAyncMissingId()
        {
            AcoustIDClient client = this.CreateClient();

            string content = "{ \"status\": \"ok\", \"results\":[{ \"score\": 1.0 }] }";
            AcoustIDTrackIdResponse response = await client.ParseTrackIdResponseAync(content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(-1, response.Code, "response.Code");
            Assert.IsNull(response.Id, "response.Id");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with missing score.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackIdResponseAyncMissingScore()
        {
            AcoustIDClient client = this.CreateClient();

            string content = "{ \"status\": \"ok\", \"results\":[{ \"id\": \"9ff43b6a-4f16-427c-93c2-92307ca505e0\" }] }";
            AcoustIDTrackIdResponse response = await client.ParseTrackIdResponseAync(content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("ok", response.Status, "response.Status");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual("9ff43b6a-4f16-427c-93c2-92307ca505e0", response.Id, "response.Id");
            Assert.AreEqual(-1, response.Score, "response.Score");
        }

        /// <summary>
        /// Test the ability to call ParseTrackIdResponseAync with a valid response.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackIdResponseAyncSuccess()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse response = await client.ParseTrackIdResponseAync(CanonicalTrackIdResponse).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("ok", response.Status, "response.Status");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual("9ff43b6a-4f16-427c-93c2-92307ca505e0", response.Id, "response.Id");
            Assert.AreEqual(100, response.Score, "response.Score");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with an error.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncError()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9ffNotAGuid",
                Score = 100,
            };

            string content = "{ \"error\": \"Invalid mbid.\" }";
            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Invalid mbid.", response.Status, "response.Status");
            Assert.AreEqual(1, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count(), "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a missing id.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncMissingId()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9ff",
                Score = 100,
            };

            string content = "{ \"title\": \"LAST ANGEL\" }";
            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(2, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count(), "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a missing title.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncMissingTitle()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9ff",
                Score = 100,
            };

            string content = "{ \"id\": \"b9ad642e-b012-41c7-b72a-42cf4911f9ff\" }";
            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(3, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count(), "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a missing releases array.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncMissingReleases()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9ff",
                Score = 100,
            };

            string content = "{ \"id\": \"b9ad642e-b012-41c7-b72a-42cf4911f9ff\", \"title\": \"LAST ANGEL\" }";
            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("error", response.Status, "response.Status");
            Assert.AreEqual(4, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count(), "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a empty releases array.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncEmptyReleases()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9ff",
                Score = 100,
            };

            string content = "{ \"id\": \"b9ad642e-b012-41c7-b72a-42cf4911f9ff\", \"title\": \"LAST ANGEL\", \"releases\": [] }";
            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("ok", response.Status, "response.Status");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count(), "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a valid response.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncSuccess()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9ff",
                Score = 89,
            };

            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, CanonicalTrackResponse).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("ok", response.Status, "response.Status");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual(2, response.Tracks.Count(), "response.Tracks.Count");

            IReadOnlyTrack track1 = response.Tracks.ToList()[0];
            Assert.AreEqual("b9ad642e-b012-41c7-b72a-42cf4911f9ff", track1.Identifier, "track1.Identifier");
            Assert.AreEqual("LAST ANGEL", track1.Title, "track1.Title");
            Assert.AreEqual("LAST ANGEL", track1.Album, "track1.Album");
            Assert.AreEqual("倖田來未", track1.Artist, "track1.Artist");
            Assert.AreEqual(230000, track1.Duration, "track1.Duration");
            Assert.AreEqual("89", track1.MatchConfidence, "track1.MatchConfidence");
            StringAssert.Contains(track1.CovertArtImage.ToString(), "c33dee6a-e053-4272-84ad-dfeef3f48c8a", "track1.CovertArtImage");

            IReadOnlyTrack track2 = response.Tracks.ToList()[1];
            Assert.AreEqual("b9ad642e-b012-41c7-b72a-42cf4911f9ff", track2.Identifier, "track2.Identifier");
            Assert.AreEqual("LAST ANGEL", track2.Title, "track2.Title");
            Assert.AreEqual("Kingdom", track2.Album, "track2.Album");
            Assert.AreEqual("倖田來未", track2.Artist, "track2.Artist");
            Assert.AreEqual(230000, track2.Duration, "track2.Duration");
            Assert.AreEqual("89", track2.MatchConfidence, "track2.MatchConfidence");
            StringAssert.Contains(track2.CovertArtImage.ToString(), "601a4558-e416-410b-a64c-857fc133b75c", "track2.CovertArtImage");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a result with no matches.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncNoResults()
        {
            AcoustIDClient client = this.CreateClient();

            AcoustIDTrackIdResponse trackId = new AcoustIDTrackIdResponse()
            {
                Id = "b9ad642e-b012-41c7-b72a-42cf4911f9fe",
                Score = 89,
            };

            string content = "{\"help\":\"For usage, please see: https://musicbrainz.org/development/mmd\",\"error\":\"Not Found\"}";
            AcoustIDTrackResponse response = await client.ParseTrackInfoResponseAync(trackId, content).ConfigureAwait(false);
            Assert.IsNotNull(response, "response");
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Not Found", response.Status, "response.Status");
            Assert.AreEqual(1, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count(), "response.Tracks.Count");
        }

        /// <summary>
        /// Create a test client.
        /// </summary>
        /// <param name="apikey">APIKey.</param>
        /// <param name="httpFilter">IHttpFilter.</param>
        /// <returns>AcoustIDClient.</returns>
        private AcoustIDClient CreateClient(string apikey = "ClientKey", IHttpFilter httpFilter = null)
        {
            AcoustIDClientIdData clientdata = new AcoustIDClientIdData()
            {
                APIKey = apikey,
            };

            AcoustIDClient client = new AcoustIDClient(clientdata, httpFilter);
            return client;
        }

        /// <summary>
        /// The example string from: https://acoustid.org/webservice.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "I don't want to.")]
        private static readonly string CanonicalTrackIdResponse = string.Concat(
        "{",
        "  \"status\": \"ok\",",
        "  \"results\": [{",
        "    \"id\": \"9ff43b6a-4f16-427c-93c2-92307ca505e0\",",
        "    \"score\": 1.0",
        "  }]",
        "}");

        /// <summary>
        /// The example string from: https://musicbrainz.org/doc/Development/JSON_Web_Service.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "I don't want to.")]
        private static readonly string CanonicalTrackResponse = string.Concat(
        "{",
        "    \"id\": \"b9ad642e-b012-41c7-b72a-42cf4911f9ff\",",
        "    \"title\": \"LAST ANGEL\",",
        "    \"artist-credit\": [",
        "      {",
        "        \"name\": \"倖田來未\",",
        "        \"joinphrase\": \" feat. \",",
        "        \"artist\": {",
        "          \"id\": \"455641ea-fff4-49f6-8fb4-49f961d8f1ac\",",
        "          \"name\": \"倖田來未\",",
        "          \"disambiguation\": \"\",",
        "          \"sort-name\": \"Koda, Kumi\"",
        "        }",
        "      },",
        "      {",
        "        \"name\": \"東方神起\",",
        "        \"joinphrase\": \"\",",
        "        \"artist\": {",
        "          \"id\": \"05cbaf37-6dc2-4f71-a0ce-d633447d90c3\",",
        "          \"name\": \"東方神起\",",
        "          \"disambiguation\": \"\",",
        "          \"sort-name\": \"Tohoshinki\"",
        "        }",
        "      }",
        "    ],",
        "    \"disambiguation\": \"\",",
        "    \"length\": 230000,",
        "    \"video\": false,",
        "    \"genres\": [],",
        "    \"releases\": [",
        "      {",
        "        \"id\": \"c33dee6a-e053-4272-84ad-dfeef3f48c8a\",",
        "        \"title\": \"LAST ANGEL\"",
        "      },",
        "      {",
        "        \"id\": \"601a4558-e416-410b-a64c-857fc133b75c\",",
        "        \"title\": \"Kingdom\"",
        "      }",
        "    ]",
        "}");

        /// <summary>
        /// A test IHttpFilter for capturing/redirecting HTTP requests.
        /// </summary>
        private class TestHttpFilter : IHttpFilter
        {
            /// <summary>
            /// Gets the most recent request received.
            /// </summary>
            public HttpRequestMessage Request { get; private set; }

            /// <summary>
            ///  Gets or sets the response to provide.
            /// </summary>
            public HttpResponseMessage Response { get; set; }

            /// <inheritdocs />
            public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
            {
                this.Request = request;
                return AsyncInfo.Run((CancellationToken cancellationToken, IProgress<HttpProgress> progress) =>
                {
                    progress.Report(default);

                    try
                    {
                        this.Response.RequestMessage = request;
                        return Task.FromResult(this.Response);
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
