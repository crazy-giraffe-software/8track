// <copyright file="AcoustIDTrackResponse.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace CrazyGiraffe.AudioIdentification.AcoustID
{
    using System.Collections.Generic;

    /// <summary>
    /// The response to parse a track.
    /// </summary>
    public class AcoustIDTrackResponse
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
        /// Gets or sets the tracks.
        /// </summary>
        public IEnumerable<IReadOnlyTrack> Tracks { get; set; }
    }
}
