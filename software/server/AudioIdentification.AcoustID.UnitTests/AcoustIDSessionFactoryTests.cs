// <copyright file="AcoustIDSessionFactoryTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.AcoustID;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A class to test <see cref="AcoustIDSessionFactory"/>.
    /// </summary>
    [TestClass]
    public class AcoustIDSessionFactoryTests
    {
        /// <summary>
        /// Test the ability create an <see cref="AcoustIDSession"/> from a <see cref="AcoustIDSessionFactory"/>.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task AcoustIDSessionFactorySuccess()
        {
            AcoustIDClientIdData cientIdData = new AcoustIDClientIdData()
            {
                APIKey = "APIKey",
            };

            ISessionFactory factory = new AcoustIDSessionFactory(cientIdData);
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
