//-----------------------------------------------------------------------
// <copyright file="AcoustIDClient.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using Windows.Data.Json;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// Client for AcoustID account.
    /// </summary>
    public class AcoustIDClient
    {
        /// <summary>
        /// The client data.
        /// </summary>
        private readonly AcoustIDClientIdData clientdata;

        /// <summary>
        /// An HTTP filter to use with the client.
        /// </summary>
        private readonly IHttpFilter httpFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcoustIDClient"/> class.
        /// </summary>
        /// <param name="clientdata">The AcoustID client data.</param>
        /// <param name="httpFilter">An HTTP client filter.</param>
        public AcoustIDClient(AcoustIDClientIdData clientdata, IHttpFilter httpFilter)
            : this()
        {
            this.clientdata = clientdata;
            this.httpFilter = httpFilter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcoustIDClient"/> class.
        /// </summary>
        private AcoustIDClient()
        {
        }

        /// <summary>
        /// Get the track id from AcoustID.
        /// </summary>
        /// <param name="fingerprint">The fingerprint.</param>
        /// <param name="duration">The track duration.</param>
        /// <returns>The HTTP response.</returns>
        public Task<HttpRequestResult> QueryTrackIdAsync(string fingerprint, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(fingerprint))
            {
                return Task.FromException<HttpRequestResult>(new ArgumentNullException(nameof(fingerprint)));
            }

            // Build request.
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("format", "json");
            queryString.Add("client", this.clientdata.APIKey);
            queryString.Add("duration", duration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            queryString.Add("fingerprint", fingerprint);

            UriBuilder builder = new UriBuilder("https://api.acoustid.org/v2/lookup")
            {
                Query = queryString.ToString(),
            };

            // Send request.
            using (HttpClient httpClient = this.GetHttpClient())
            {
                return httpClient.TryGetAsync(builder.Uri).AsTask();
            }
        }

        /// <summary>
        /// Get the track info from AcoustID.
        /// </summary>
        /// <param name="id">The track id.</param>
        /// <returns>The HTTP response.</returns>
        public Task<HttpRequestResult> QueryTrackInfoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Task.FromException<HttpRequestResult>(new ArgumentNullException(nameof(id)));
            }

            // Build request.
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("inc", "artist-credits+releases+genres");
            queryString.Add("fmt", "json");

            UriBuilder builder = new UriBuilder($"https://musicbrainz.org/ws/2/recording/{id}")
            {
                Query = queryString.ToString(),
            };

            // Send request.
            using (HttpClient httpClient = this.GetHttpClient())
            {
                return httpClient.TryGetAsync(builder.Uri).AsTask();
            }
        }

        /// <summary>
        /// Parse the track id response.
        /// </summary>
        /// <param name="responseBody">The response body.</param>
        /// <returns>A <see cref="Task"/> containing a collection of track id responses.</returns>
        public Task<AcoustIDTrackIdResponse> ParseTrackIdResponseAync(string responseBody)
        {
            AcoustIDTrackIdResponse response = new AcoustIDTrackIdResponse()
            {
                Status = "error",
                Code = -1,
                Id = null,
                Score = -1,
            };

            JsonObject root = JsonObject.Parse(responseBody);
            do
            {
                try
                {
                    // Error:
                    // "error": {
                    //    "code": 4,
                    //    "message": "invalid API key"
                    // },
                    // "status": "error"

                    // Success:
                    // "status": "ok",
                    // "results": [{
                    //  "id": "9ff43b6a-4f16-427c-93c2-92307ca505e0",
                    //  "score": 1.0
                    // }]
                    if (!root.ContainsKey("status"))
                    {
                        Debug.WriteLine("ParseTrackResponseAync: status missing");
                        continue;
                    }

                    string status = root.GetNamedString("status");
                    if (status != "ok")
                    {
                        Debug.WriteLine($"ParseTrackResponseAync: status = {status}");

                        int code = -1;
                        if (root.ContainsKey("error"))
                        {
                            JsonObject error = root.GetNamedObject("error");
                            if (error.ContainsKey("code"))
                            {
                                code = (int)error.GetNamedNumber("code");
                            }

                            if (error.ContainsKey("message"))
                            {
                                Debug.WriteLine($"ParseTrackResponseAync: error code = {code}, message = {error.GetNamedString("message")}");
                            }
                        }

                        response = new AcoustIDTrackIdResponse()
                        {
                            Status = status,
                            Code = code,
                            Id = null,
                            Score = -1,
                        };

                        continue;
                    }

                    if (!root.ContainsKey("results"))
                    {
                        Debug.WriteLine("ParseTrackResponseAync: results missing");
                        continue;
                    }

                    JsonArray results = root.GetNamedArray("results");
                    if (results.Count > 0)
                    {
                        JsonObject result = results.GetObjectAt(0);
                        if (!result.ContainsKey("id"))
                        {
                            Debug.WriteLine("ParseTrackResponseAync: id missing");
                            continue;
                        }

                        string id = result.GetNamedString("id");

                        int score = -1;
                        if (result.ContainsKey("score"))
                        {
                            score = (int)(result.GetNamedNumber("score") * 100);
                        }

                        response = new AcoustIDTrackIdResponse()
                        {
                            Status = status,
                            Code = 0,
                            Id = id,
                            Score = score,
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ParseTrackResponseAync track exception: {ex.Message}");
                }
            }
            while (false);

            return Task.FromResult(response);
        }

        /// <summary>
        /// Parse the track info response.
        /// </summary>
        /// <param name="trackId">The track id.</param>
        /// <param name="responseBody">The response body.</param>
        /// <returns>A <see cref="Task"/> containing a collection of track responses.</returns>
        public Task<AcoustIDTrackResponse> ParseTrackInfoResponseAync(AcoustIDTrackIdResponse trackId, string responseBody)
        {
            string status = "ok";
            int code = 0;
            IList<IReadOnlyTrack> tracks = new List<IReadOnlyTrack>();

            do
            {
                Track trackRoot = new Track()
                {
                    MatchConfidence = trackId?.Score.ToString(CultureInfo.InvariantCulture),
                };

                JsonObject root = JsonObject.Parse(responseBody);

                // {"help":"For usage, please see: https://musicbrainz.org/development/mmd","error":"Not Found"}
                if (root.ContainsKey("error"))
                {
                    status = root.GetNamedString("error");
                    code = 1;
                    continue;
                }

                // id: "b9ad642e-b012-41c7-b72a-42cf4911f9ff",
                if (!root.ContainsKey("id"))
                {
                    Debug.WriteLine("ParseTrackResponseAync: id missing");
                    status = "error";
                    code = 2;
                    continue;
                }

                trackRoot.Identifier = root.GetNamedString("id");

                // title: "LAST ANGEL",
                if (!root.ContainsKey("title"))
                {
                    Debug.WriteLine("ParseTrackResponseAync: title missing");
                    status = "error";
                    code = 3;
                    continue;
                }

                trackRoot.Title = root.GetNamedString("title");

                // Get artist (first one)
                // artist-credit: [
                //    {
                //        name: "倖田來未",
                //        ...
                //    },
                //    ...
                // ],
                if (root.ContainsKey("artist-credit"))
                {
                    JsonArray artists = root.GetNamedArray("artist-credit");
                    if (artists.Count > 0)
                    {
                        JsonObject artist = artists.GetObjectAt(0);
                        trackRoot.Artist = artist.GetNamedString("name");
                    }
                }

                // length: 230000,
                if (root.ContainsKey("length"))
                {
                    trackRoot.Duration = (int)root.GetNamedNumber("length");
                }

                // genres: [
                //  { name: "blue-eyed soul", count: 2}
                // ],
                if (root.ContainsKey("genres"))
                {
                    JsonArray genres = root.GetNamedArray("genres");
                    if (genres.Count > 0)
                    {
                        JsonObject genre = genres.GetObjectAt(0);
                        trackRoot.Genre = genre.GetNamedString("name");
                    }
                }

                // releases: [
                //  {
                //     id: "c33dee6a-e053-4272-84ad-dfeef3f48c8a",
                //     title: "LAST ANGEL",
                //     /* some properties omitted to keep this example shorter, see the release results for the full format */
                //  },
                //  {
                //     id: "601a4558-e416-410b-a64c-857fc133b75c",
                //     title: "Kingdom",
                //     /* some properties omitted to keep this example shorter, see the release results for the full format */
                //  }
                // ]
                if (!root.ContainsKey("releases"))
                {
                    Debug.WriteLine("ParseTrackResponseAync: releases missing");
                    status = "error";
                    code = 4;
                    continue;
                }

                JsonArray releaseArray = root.GetNamedArray("releases");
                for (uint i = 0; i < releaseArray.Count; i++)
                {
                    try
                    {
                        Track track = new Track()
                        {
                            Identifier = trackRoot.Identifier,
                            Title = trackRoot.Title,
                            Artist = trackRoot.Artist,
                            Duration = trackRoot.Duration,
                            MatchConfidence = trackRoot.MatchConfidence,
                        };

                        JsonObject release = releaseArray.GetObjectAt(i);

                        // title: "LAST ANGEL",
                        track.Album = release.GetNamedString("title");

                        // Get cover art Uri from id..
                        string releaseId = release.GetNamedString("id");
                        track.CovertArtImage = new Uri($"http://coverartarchive.org/release/{releaseId}/front");

                        // Save track to list.
                        tracks.Add(track);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ParseTrackResponseAync track exception: {ex.Message}");
                    }
                }
            }
            while (false);

            return Task.FromResult(new AcoustIDTrackResponse()
            {
                Status = status,
                Code = code,
                Tracks = tracks,
            });
        }

        /// <summary>
        /// Get an HTTP client.
        /// </summary>
        /// <returns>An HTTP client.</returns>
        private HttpClient GetHttpClient()
        {
            if (this.httpFilter != null)
            {
                return new HttpClient(this.httpFilter);
            }

            return new HttpClient();
        }
    }
}
