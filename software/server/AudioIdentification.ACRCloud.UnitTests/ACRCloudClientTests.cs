//-----------------------------------------------------------------------
// <copyright file="ACRCloudClientTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ACRCloud.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ACRCloudRecognitionTest;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.ACRCloud;
    using HttpMultipartParser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Foundation;
    using Windows.Storage.Streams;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using StreamContent = System.Net.Http.StreamContent;

    /// <summary>
    /// Tests for <see cref="ACRCloudClient"/>.
    /// </summary>
    [TestClass]
    public class ACRCloudClientTests
    {
        /// <summary>
        /// The example string from: https://docs.acrcloud.com/docs/acrcloud/metadata/music/.
        /// </summary>
        /// <returns>A canonical track response.</returns>
        public static string GetCanonicalTrackResponse()
        {
            return CanonicalTrackResponse;
        }

        /// <summary>
        /// Test the ability to call CreateSignature.
        /// </summary>
        [TestMethod]
        public void CreateSignatureSuccess()
        {
            string host = "host";
            string accessKey = "access_key";
            string accessSecret = "access_secret";
            ACRCloudRecognizer reference = CreateReferenceClient(host, accessKey, accessSecret);

            string input = "input";
            string key = "key";
            string referenceHash = reference.EncryptByHMACSHA1(input, key);

            ACRCloudClient client = CreateClient(host, accessKey, accessSecret);
            string testHash = client.CreateSignature(input, key);
            Assert.AreEqual(referenceHash, testHash, "testHash");
        }

        /// <summary>
        /// Test the ability to call CreateSignature with invalid arguments.
        /// </summary>
        [TestMethod]
        public void CreateSignatureInvalidArgs()
        {
            ACRCloudClient client = CreateClient();
            Assert.ThrowsException<ArgumentNullException>(() => client.CreateSignature(null, "key"));
            Assert.ThrowsException<ArgumentNullException>(() => client.CreateSignature("input", null));
        }

        /// <summary>
        /// Test the ability to call QueryTrackInfoAsync.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task QueryTrackInfoAsyncSuccess()
        {
            string host = "host";
            string accessKey = "access_key";
            string accessSecret = "access_secret";
            ACRCloudRecognizer reference = CreateReferenceClient(host, accessKey, accessSecret);

            using (TestHttpFilter filter = new TestHttpFilter())
            using (HttpResponseMessage emptyResponse = new HttpResponseMessage(HttpStatusCode.Ok) { Content = new HttpStringContent(string.Empty) })
            {
                ACRCloudClient client = CreateClient(host, accessKey, accessSecret, filter);
                filter.Response = emptyResponse;

                byte[] fingerprint = Encoding.UTF8.GetBytes("fingerprint");
                HttpRequestResult result = null;
                using (MemoryStream memoryStream = new MemoryStream(100))
                {
                    memoryStream.Write(fingerprint, 0, fingerprint.Length);

                    IBuffer buffer = WindowsRuntimeBufferExtensions.GetWindowsRuntimeBuffer(memoryStream);
                    result = await client.QueryTrackInfoAsync(buffer);
                    Assert.IsNotNull(result, "result");
                }

                Assert.IsNotNull(filter.Request, "filter.Request");
                Assert.AreEqual(HttpMethod.Post.Method, filter.Request.Method.Method, "filter.Request.Method");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), host, "host");
                StringAssert.Contains(filter.Request.RequestUri.ToString(), "/v1/identify", "path");

                IHttpContent requestContent = filter.Request.Content;
                HttpMultipartFormDataContent formContent = requestContent as HttpMultipartFormDataContent;
                Assert.IsNotNull(formContent, "formContent");

                IBuffer formContentBuffer = await formContent.ReadAsBufferAsync();
                Stream testStream = WindowsRuntimeBufferExtensions.AsStream(formContentBuffer);
                MultipartFormDataParser testRequest = MultipartFormDataParser.Parse(testStream);

                string testBoundry = requestContent.Headers.ContentType.ToString();
                int lastUnderstorce = testBoundry.LastIndexOf('_');
                string testTimeStamp = testBoundry.Substring(lastUnderstorce + 1);
                long testTicks = Convert.ToInt64(testTimeStamp, 16);
                DateTime testDateTime = new DateTime(testTicks, DateTimeKind.Utc);
                string testTimestamp = testRequest.Parameters.Where(x => x.Name == "timestamp").FirstOrDefault().Data;

                MultipartFormDataParser referenceRequest = null;
                using (StreamContent referenceStreamContent = reference.Recognize(fingerprint, testDateTime, testTimestamp))
                {
                    Stream referenceStream = await referenceStreamContent.ReadAsStreamAsync().ConfigureAwait(true);
                    referenceRequest = MultipartFormDataParser.Parse(referenceStream);
                }

                Assert.AreEqual(referenceRequest.Files.Count, testRequest.Files.Count, "testRequest.Files.Count");
                Assert.AreEqual(referenceRequest.Parameters.Count, testRequest.Parameters.Count, "testRequest.Parameters.Count");

                foreach (FilePart referenceFile in referenceRequest.Files)
                {
                    FilePart testFile = testRequest.Files.Where(x => x.Name == referenceFile.Name).FirstOrDefault();
                    Assert.IsNotNull(testFile, "testFile");

                    Assert.AreEqual(referenceFile.ContentDisposition, testFile.ContentDisposition, "testFile.ContentDisposition");
                    Assert.AreEqual(referenceFile.ContentType, testFile.ContentType, "testFile.ContentType");
                    Assert.AreEqual(referenceFile.FileName, testFile.FileName, "testFile.FileName");
                }

                foreach (ParameterPart referenceParameter in referenceRequest.Parameters)
                {
                    ParameterPart testParameter = testRequest.Parameters.Where(x => x.Name == referenceParameter.Name).FirstOrDefault();
                    Assert.IsNotNull(testParameter, "testParameter");

                    Assert.AreEqual(referenceParameter.Data, testParameter.Data, "testParameter.Data");
                }
            }
        }

        /// <summary>
        /// Test the ability to call QueryTrackInfoAsync with invalid arguments.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task QueryTrackInfoAsyncInvalidArgument()
        {
            ACRCloudClient client = CreateClient();
            HttpRequestResult result = await client.QueryTrackInfoAsync(null);
            Assert.IsNull(result, "result");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with the status missing.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncMissingStatus()
        {
            ACRCloudClient client = CreateClient();

            string content = "{ \"result_type\":0 }";
            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(content);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual(-1, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count, "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a bad status code.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncBadStatusCode()
        {
            ACRCloudClient client = CreateClient();

            string content = "{ \"result_type\":0, \"status\":{ \"msg\":\"Success\", \"version\":\"1.0\", \"code\":1 } }";
            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(content);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Success", response.Message, "response.Message");
            Assert.AreEqual("1.0", response.Version, "response.Version");
            Assert.AreEqual(1, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count, "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with missing meta data.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncMissingMetadata()
        {
            ACRCloudClient client = CreateClient();

            string content = "{ \"result_type\":0, \"status\":{ \"msg\":\"Success\", \"version\":\"1.0\", \"code\":0 } }";
            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(content);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Success", response.Message, "response.Message");
            Assert.AreEqual("1.0", response.Version, "response.Version");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count, "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with missing music.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncMissingMusic()
        {
            ACRCloudClient client = CreateClient();

            string content = "{ \"result_type\":0, \"status\":{ \"msg\":\"Success\", \"version\":\"1.0\", \"code\":0 }, \"metadata\":{ } }";
            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(content);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Success", response.Message, "response.Message");
            Assert.AreEqual("1.0", response.Version, "response.Version");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count, "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with empty music.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncEmptyMusic()
        {
            ACRCloudClient client = CreateClient();

            string content = "{ \"result_type\":0, \"status\":{ \"msg\":\"Success\", \"version\":\"1.0\", \"code\":0 }, \"metadata\":{ \"music\" : [ ] } }";
            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(content);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Success", response.Message, "response.Message");
            Assert.AreEqual("1.0", response.Version, "response.Version");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count, "response.Tracks.Count");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a valid response.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncSuccess()
        {
            ACRCloudClient client = CreateClient();

            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(CanonicalTrackResponse);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("Success", response.Message, "response.Message");
            Assert.AreEqual("1.0", response.Version, "response.Version");
            Assert.AreEqual(0, response.Code, "response.Code");
            Assert.AreEqual(1, response.Tracks.Count, "response.Tracks.Count");

            IReadOnlyTrack track = response.Tracks[0];
            Assert.AreEqual("6049f11da7095e8bb8266871d4a70873", track.Identifier, "track.Identifier");
            Assert.AreEqual("Hello", track.Title, "track.Title");
            Assert.AreEqual("Hello", track.Album, "track.Album");
            Assert.AreEqual("Adele", track.Artist, "track.Artist");
            Assert.AreEqual(295000, track.Duration, "track.Duration");
            Assert.AreEqual(9040, track.CurrentPosition, "track.CurrentPosition");
            Assert.AreEqual(9040, track.MatchPosition, "track.MatchPosition");
            Assert.AreEqual("100", track.MatchConfidence, "track.MatchConfidence");
            StringAssert.Contains(track.CovertArtImage.ToString(), "0a8e8d55-4b83-4f8a-9732-fbb5ded9f344", "track.CovertArtImage");
        }

        /// <summary>
        /// Test the ability to call ParseTrackResponseAync with a result with no matches.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        [TestMethod]
        public async Task ParseTrackResponseAyncNoResults()
        {
            ACRCloudClient client = CreateClient();

            string content = "{\"status\":{\"msg\":\"No result\",\"version\":\"1.0\",\"code\":1001}}";
            ACRCloudTrackResponse response = await client.ParseTrackResponseAync(content);
            Assert.IsNotNull(response, "response");
            Assert.AreEqual("No result", response.Message, "response.Message");
            Assert.AreEqual("1.0", response.Version, "response.Version");
            Assert.AreEqual(1001, response.Code, "response.Code");
            Assert.AreEqual(0, response.Tracks.Count, "response.Tracks.Count");
        }

        /// <summary>
        /// Create a reference client.
        /// </summary>
        /// <param name="host">host.</param>
        /// <param name="accessKey">accessKey.</param>
        /// <param name="accessSecret">accessSecret.</param>
        /// <param name="webCreator">A function to create a web request.</param>
        /// <returns>ACRCloudRecognizer.</returns>
        private static ACRCloudRecognizer CreateReferenceClient(
                string host = "host",
                string accessKey = "access_key",
                string accessSecret = "access_secret",
                Func<string, System.Net.HttpWebRequest> webCreator = null)
        {
            Dictionary<string, object> config = new Dictionary<string, object>
            {
                { "host", host },
                { "access_key", accessKey },
                { "access_secret", accessSecret },
            };

            if (webCreator != null)
            {
                config.Add("web_creator", webCreator);
            }

            ACRCloudRecognizer reference = new ACRCloudRecognizer(config);
            return reference;
        }

        /// <summary>
        /// Create a test client.
        /// </summary>
        /// <param name="host">host.</param>
        /// <param name="accessKey">accessKey.</param>
        /// <param name="accessSecret">accessSecret.</param>
        /// <param name="httpFilter">HTTP filter.</param>
        /// <returns>ACRCloudClient.</returns>
        private static ACRCloudClient CreateClient(
            string host = "host",
            string accessKey = "access_key",
            string accessSecret = "access_secret",
            IHttpFilter httpFilter = null)
        {
            ACRCloudClientIdData clientdata = new ACRCloudClientIdData()
            {
                Host = host,
                AccessKey = accessKey,
                AccessSecret = accessSecret,
            };

            ACRCloudClient client = new ACRCloudClient(clientdata, httpFilter);
            return client;
        }

        /// <summary>
        /// The example string from: https://docs.acrcloud.com/docs/acrcloud/metadata/music/.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "I don't want to.")]
        private static readonly string CanonicalTrackResponse = string.Concat(
        "{",
        "   \"metadata\":",
        "    {",
        "        \"timestamp_utc\":\"2020-01-19 02:58:28\",",
        "        \"music\":[",
        "            {",
        "                \"db_begin_time_offset_ms\":0,",
        "                \"db_end_time_offset_ms\":9280,",
        "                \"sample_begin_time_offset_ms\":0,",
        "                \"sample_end_time_offset_ms\":9280,",
        "                \"play_offset_ms\":9040,",
        "                \"artists\":[",
        "                    {",
        "                        \"name\":\"Adele\"",
        "                    }",
        "                ],",
        "                \"lyrics\":{",
        "                    \"copyrights\":[",
        "                        \"Sony/ATV Music Publishing LLC\", ",
        "                        \"Universal Music Publishing Group\"",
        "                    ]",
        "                },",
        "                \"acrid\":\"6049f11da7095e8bb8266871d4a70873\",",
        "                \"album\":{",
        "                    \"name\":\"Hello\"",
        "                },",
        "                \"rights_claim\": [",
        "                    {\"rights_owner\":\"WMG\",\"rights_claim_policy\":\"monetize\", \"territories\":[\"AD\",\"AE\",\"AF\"]}, ",
        "                    {\"rights_owner\":\"SME\",\"excluded_territories\":[\"AB\",\"AC\"]}",
        "               ],",
        "               \"external_ids\":{",
        "                    \"iswc\":\"T-917.819.808-8\",",
        "                    \"isrc\":\"GBBKS1500214\",",
        "                    \"upc\":\"886445581959\"",
        "                },",
        "                \"result_from\":3,",
        "                \"contributors\":{",
        "                    \"composers\":[",
        "                        \"Adele Adkins\",",
        "                        \"Greg Kurstin\"",
        "                    ],",
        "                    \"lyricists\":[",
        "                        \"ADELE ADKINS\",",
        "                        \"GREGORY KURSTIN\"",
        "                    ]",
        "                },",
        "                \"title\":\"Hello\",",
        "                \"language\":\"en\",",
        "                \"duration_ms\":295000,",
        "                \"external_metadata\":{",
        "                    \"musicbrainz\":[",
        "                        {",
        "                            \"track\":{",
        "                                \"id\":\"0a8e8d55-4b83-4f8a-9732-fbb5ded9f344\"",
        "                            }",
        "                        }",
        "                    ],",
        "                    \"deezer\":{",
        "                        \"track\":{",
        "                            \"id\":\"110265034\"",
        "                        },",
        "                        \"artists\":[",
        "                            {",
        "                                \"id\":\"75798\"",
        "                            }",
        "                        ],",
        "                        \"album\":{",
        "                            \"id\":\"11483764\"",
        "                        }",
        "                    },",
        "                    \"spotify\":{",
        "                        \"track\":{",
        "                            \"id\":\"4aebBr4JAihzJQR0CiIZJv\"",
        "                        },",
        "                        \"artists\":[",
        "                            {",
        "                                \"id\":\"4dpARuHxo51G3z768sgnrY\"",
        "                            }",
        "                        ],",
        "                        \"album\":{",
        "                            \"id\":\"7uwTHXmFa1Ebi5flqBosig\"",
        "                        }",
        "                    },",
        "                    \"musicstory\":{",
        "                        \"track\":{",
        "                            \"id\":\"13106540\"",
        "                        },",
        "                        \"album\":{",
        "                            \"id\":\"931271\"",
        "                        }",
        "                    },",
        "                    \"youtube\":{",
        "                        \"vid\":\"YQHsXMglC9A\"",
        "                    }",
        "                },",
        "                \"score\":100,",
        "                \"release_date\":\"2015-10-23\"",
        "            }",
        "        ]",
        "    },",
        "    \"status\":{",
        "        \"msg\":\"Success\",",
        "        \"version\":\"1.0\",",
        "        \"code\":0",
        "    },",
        "    \"result_type\":0",
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

            /// <inheritdoc />
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

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }
    }
}
