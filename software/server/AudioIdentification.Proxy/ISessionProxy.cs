//-----------------------------------------------------------------------
// <copyright file="ISessionProxy.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for a session proxy.
    /// </summary>
    public interface ISessionProxy : ISession, IDisposable
    {
        /// <summary>
        /// Update the track status, track and track identifier for a session.
        /// </summary>
        /// <param name="idStatus">The identification status.</param>
        /// <param name="tracks">The tracks.</param>
        void ProcessTrackResponse(IdentifyStatus idStatus, IEnumerable<IReadOnlyTrack> tracks);
    }
}
