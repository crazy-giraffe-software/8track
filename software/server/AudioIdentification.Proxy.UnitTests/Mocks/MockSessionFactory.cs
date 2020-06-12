//-----------------------------------------------------------------------
// <copyright file="MockSessionFactory.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.UnitTests.Mocks
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using Windows.Foundation;

    /// <summary>
    /// Mock AudioIdentification session factory.
    /// </summary>
    public class MockSessionFactory : ISessionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockSessionFactory"/> class.
        /// </summary>
        public MockSessionFactory()
        {
            this.SupportMultipleSession = false;
            this.NeededFrames = 1;
            this.ResultingTrackCount = 1;
            this.FailTrackAttempt = false;
        }

        /// <summary>
        /// Gets the identifier for the session.
        /// </summary>
        public MockSession MockSession { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple sessions are supported.
        /// </summary>
        public bool SupportMultipleSession { get; set; }

        /// <summary>
        /// Gets or sets number of frames needed to identify.
        /// </summary>
        public int NeededFrames { get; set; }

        /// <summary>
        /// Gets or sets the resulting track count.
        /// </summary>
        public int ResultingTrackCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to fail the first track attempt.
        /// </summary>
        public bool FailTrackAttempt { get; set; }

        /// <inheritdoc/>
        public IAsyncOperation<ISession> CreateSessionAsync(SessionOptions options)
        {
            if (this.MockSession == null)
            {
                this.MockSession = new MockSession(options, this.NeededFrames, this.ResultingTrackCount, this.FailTrackAttempt);
                return Task.FromResult<ISession>(this.MockSession).AsAsyncOperation();
            }

            if (!this.SupportMultipleSession)
            {
                // Always return the member session.
                return Task.FromResult<ISession>(this.MockSession).AsAsyncOperation();
            }

            // Return a new session.
            return Task.FromResult<ISession>(new MockSession(options)).AsAsyncOperation();
        }
    }
}
