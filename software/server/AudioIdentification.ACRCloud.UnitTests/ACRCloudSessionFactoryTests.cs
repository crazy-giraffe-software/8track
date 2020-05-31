//-----------------------------------------------------------------------
// <copyright file="ACRCloudSessionFactoryTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ACRCloud.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.ACRCloud;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A class to test <see cref="ACRCloudSessionFactory"/>.
    /// </summary>
    [TestClass]
    public class ACRCloudSessionFactoryTests
    {
        /// <summary>
        /// Test the ability create an <see cref="ACRCloudSession"/> from a <see cref="ACRCloudSessionFactory"/>.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task ACRCloudSessionFactorySuccess()
        {
            ACRCloudClientIdData cientIdData = new ACRCloudClientIdData()
            {
                Host = "Host",
                AccessKey = "AccessKey",
                AccessSecret = "AccessSecret",
            };

            ISessionFactory factory = new ACRCloudSessionFactory(cientIdData);
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
