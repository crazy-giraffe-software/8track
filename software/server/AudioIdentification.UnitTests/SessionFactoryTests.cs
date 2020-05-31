//-----------------------------------------------------------------------
// <copyright file="SessionFactoryTests.cs" company="CrazyGiraffeSoftware.net">
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
    /// Test class to test <see cref="SessionFactory"/>.
    /// </summary>
    [TestClass]
    public class SessionFactoryTests
    {
        /// <summary>
        /// Test the ability create an <see cref="Session"/> from a <see cref="SessionFactory"/>.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task SessionFactorySuccess()
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
        }
    }
}
