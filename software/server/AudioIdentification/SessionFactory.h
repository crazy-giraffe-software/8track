//-----------------------------------------------------------------------
// <copyright file="SessionFactory.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

#include "Session.h"
#include "SessionOptions.h"

namespace CrazyGiraffe { namespace AudioIdentification
{
    /// <summary>
    /// AudioIdentification session interface.
    /// </summary>
    public interface class ISessionFactory
    {
    public:
        /// <summary>
        /// Create a new session to identify a track.
        /// </summary>
        /// <param name="options">Options for the session.</param>
        /// <returns>A new session to identify a track.</returns>
        Windows::Foundation::IAsyncOperation<CrazyGiraffe::AudioIdentification::ISession^>^
            CreateSessionAsync(CrazyGiraffe::AudioIdentification::SessionOptions^ options);
    };

    /// <summary>
    /// AudioIdentification session factory.
    /// </summary>
    public ref class SessionFactory sealed : public  ISessionFactory
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="SessionFactory" /> class.
        /// </summary>
        SessionFactory();

        /// <summary>
        /// Create a new session to identify a track.
        /// </summary>
        /// <param name="options">Options for the session.</param>
        /// <returns>A new session to identify a track.</returns>
        virtual Windows::Foundation::IAsyncOperation<CrazyGiraffe::AudioIdentification::ISession^>^
            CreateSessionAsync(CrazyGiraffe::AudioIdentification::SessionOptions^ options);
    };
} }
