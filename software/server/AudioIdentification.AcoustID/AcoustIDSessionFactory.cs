//-----------------------------------------------------------------------
// <copyright file="AcoustIDSessionFactory.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using Windows.Foundation;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// AcoustID session factory.
    /// </summary>
    public class AcoustIDSessionFactory : ISessionFactory
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
        /// Initializes a new instance of the <see cref="AcoustIDSessionFactory"/> class.
        /// </summary>
        /// <param name="clientdata">The AcoustID client data.</param>
        public AcoustIDSessionFactory(AcoustIDClientIdData clientdata)
            : this(clientdata, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcoustIDSessionFactory"/> class.
        /// </summary>
        /// <param name="clientdata">The AcoustID client data.</param>
        /// <param name="httpFilter">An HTTP client filter.</param>
        public AcoustIDSessionFactory(AcoustIDClientIdData clientdata, IHttpFilter httpFilter)
        {
            this.clientdata = clientdata;
            this.httpFilter = httpFilter;
        }

        /// <summary>
        /// Create a new session to identify a track.
        /// </summary>
        /// <param name="options">Options for the session.</param>
        /// <returns>A new session to identify a track.</returns>
        public IAsyncOperation<ISession> CreateSessionAsync(SessionOptions options)
        {
            ISession session = new AcoustIDSession(this.clientdata, this.httpFilter, options);
            return Task.FromResult(session).AsAsyncOperation();
        }
    }
}
