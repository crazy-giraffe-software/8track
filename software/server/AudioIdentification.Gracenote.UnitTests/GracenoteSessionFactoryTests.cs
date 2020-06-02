// <copyright file="GracenoteSessionFactoryTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Gracenote.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using System.Xml;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.Gracenote;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A class to test <see cref="GracenoteSessionFactory"/>.
    /// </summary>
    [TestClass]
    public class GracenoteSessionFactoryTests
    {
        /// <summary>
        /// Test the ability create an <see cref="GracenoteSession"/> from a <see cref="GracenoteSessionFactory"/>.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task GracenoteSessionFactorySuccess()
        {
            string licenseFileName = "License.txt";
            string licenseData = ResourceHelper.ReadResourceAsString(licenseFileName);

            string clientIdFileName = "GracenoteClientId.xml";
            XmlDocument doc = ResourceHelper.ReadResourceAsXml(clientIdFileName);

            GracenoteClientIdData clientIdData = new GracenoteClientIdData()
            {
                ClientId = doc.SelectSingleNode("//ClientId")?.InnerText,
                ClientTag = doc.SelectSingleNode("//ClientTag")?.InnerText,
                AppVersion = doc.SelectSingleNode("//AppVersion")?.InnerText,
                License = licenseData,
            };

            ISessionFactory factory = new GracenoteSessionFactory(clientIdData);
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
