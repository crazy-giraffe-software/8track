// <copyright file="AcoustIDTrackIdResponse.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace CrazyGiraffe.AudioIdentification.AcoustID
{
    /// <summary>
    /// The response to parse a track id.
    /// </summary>
    public class AcoustIDTrackIdResponse
    {
        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the track id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        public int Score { get; set; }
    }
}
