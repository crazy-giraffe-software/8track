//-----------------------------------------------------------------------
// <copyright file="SessionTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class to test <see cref="Session"/>.
    /// </summary>
    [TestClass]
    public class SessionTests
    {
        /// <summary>
        /// Test the ability to create a <see cref="Session"/>.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionSuccess()
        {
            ISession session = await CreateSessionAsync().ConfigureAwait(false);
            Assert.IsNotNull(session, "session");
            Assert.IsNotNull(session.SessionIdentifier, "SessionIdentifier");
            Assert.IsFalse(session.SessionIdentifier.Contains("{", StringComparison.InvariantCulture), "{");
            Assert.IsFalse(session.SessionIdentifier.Contains("}", StringComparison.InvariantCulture), "}");
            Assert.AreEqual(IdentifyStatus.Invalid, session.IdentificationStatus, "IdentificationStatus");
        }

        /// <summary>
        /// Test the ability to create a <see cref="Session"/>.
        /// </summary>
        [TestMethod]
        public void CreateSessionIdentifierSuccess()
        {
            string sessionIdentifier = Session.CreateSessionIdentifier();
            Assert.IsNotNull(sessionIdentifier, "SessionIdentifier");
            Assert.IsFalse(sessionIdentifier.Contains("{", StringComparison.InvariantCulture), "{");
            Assert.IsFalse(sessionIdentifier.Contains("}", StringComparison.InvariantCulture), "}");
        }

        /// <summary>
        /// Test the ability create an <see cref="Session"/> from a <see cref="SessionFactory"/>.
        /// </summary>
        /// <returns>A task which can be awaited which returns a session.</returns>
        private static async Task<ISession> CreateSessionAsync()
        {
            ISessionFactory factory = new SessionFactory();
            Assert.IsNotNull(factory, "factory");

            ushort audioSampleRate = 11025;
            ushort audioSampleSize = 8;
            ushort audioChannels = 1;
            SessionOptions options = new SessionOptions(audioSampleRate, audioSampleSize, audioChannels);
            Assert.IsNotNull(options, "options");

            ISession session = await factory.CreateSessionAsync(options);
            Assert.IsNotNull(session, "session");

            return session;
        }
    }
}
