//-----------------------------------------------------------------------
// <copyright file="StatusChangedEventArgs.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioIdentification
{
    /// <summary>
    /// Status for the sample.
    /// </summary>
    public enum class IdentifyStatus
    {
        /// <summary>
        /// Sample is invalid; initial data is needed.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Sample is incomplete; more data is needed.
        /// </summary>
        Incomplete = 1,

        /// <summary>
        ///  Sample is complete.
        /// </summary>
        Complete = 2,

        /// <summary>
        ///  Sample is not identifiable.
        /// </summary>
        Error = 3,
    };

    /// <summary>
    ///  A state changed event argument class.
    /// </summary>
    public ref class StatusChangedEventArgs sealed
    {
    public:
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusChangedEventArgs" /> class.
        /// </summary>
        StatusChangedEventArgs();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusChangedEventArgs" /> class.
        /// </summary>
        /// <param name="status">The sample status.</param>
        StatusChangedEventArgs(CrazyGiraffe::AudioIdentification::IdentifyStatus status);

        /// <summary>
        /// Gets the sample status.
        /// </summary>
        property CrazyGiraffe::AudioIdentification::IdentifyStatus Status
        {
            CrazyGiraffe::AudioIdentification::IdentifyStatus get();
        }

    private:
        /// <summary>
        /// The sample status.
        /// </summary>
        CrazyGiraffe::AudioIdentification::IdentifyStatus m_status;
    };
} }
