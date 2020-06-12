//-----------------------------------------------------------------------
// <copyright file="SessionFactory.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.Proxy.AppService;
    using Windows.Foundation;

    /// <summary>
    /// AudioIdentification session factory.
    /// </summary>
    public class SessionFactory : ISessionFactory, IDisposable
    {
        /// <summary>
        /// The application service client.
        /// </summary>
        private readonly IAppServiceClient client;

        /// <summary>
        /// A collection of created sessions.
        /// </summary>
        private readonly IList<ISessionProxy> sessions;

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionFactory" /> class.
        /// </summary>
        /// <param name="client">The client to use for the proxy.</param>
        public SessionFactory(IAppServiceClient client)
        {
            this.client = client;
            this.sessions = new List<ISessionProxy>();
        }

        /// <inheritdoc/>
        public IAsyncOperation<ISession> CreateSessionAsync(SessionOptions options)
        {
            ISessionProxy session = null;
            try
            {
                session = new Proxy.Session(this.client, options);

                this.sessions.Add(session);
                return Task.FromResult<ISession>(session).AsAsyncOperation();
            }
            finally
            {
                if (session != null)
                {
                    session.Dispose();
                }
            }
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        /// <param name="disposing">true of disposing; false is finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (ISessionProxy proxy in this.sessions)
                    {
                        proxy.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}
