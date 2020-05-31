// <copyright file="GracenoteSessionTests.cs" company="CrazyGiraffeSoftware.net">
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
    using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

    /// <summary>
    /// A class to test <see cref="GracenoteSession"/>.
    /// </summary>
    [TestClass]
    public class GracenoteSessionTests
    {
        /// <summary>
        /// Test the ability to create a <see cref="GracenoteSession"/>.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task GracenoteSessionSuccess()
        {
            ISession session = await CreateSessionAsync().ConfigureAwait(false);
            Assert.IsNotNull(session, "session");
            Assert.IsNotNull(session.SessionIdentifier, "SessionIdentifier");
            Assert.AreEqual(IdentifyStatus.Invalid, session.IdentificationStatus, "IdentificationStatus");
        }

        /// <summary>
        /// Get the session options.
        /// </summary>
        /// <param name="audioSampleRate">Sample rate.</param>
        /// <param name="audioSampleSize">Sample size.</param>
        /// <param name="audioChannels">Channel count.</param>
        /// <returns>SessionOptions.</returns>
        private static SessionOptions GetSessionOptions(
            ushort audioSampleRate = 44100,
            ushort audioSampleSize = 16,
            ushort audioChannels = 2)
        {
            return new SessionOptions(audioSampleRate, audioSampleSize, audioChannels);
        }

        /// <summary>
        /// Create a new session using the default options.
        /// </summary>
        /// <returns>ISession.</returns>
        private static async Task<ISession> CreateSessionAsync()
        {
            string licenseFileName = "License.txt";
            Logger.LogMessage(string.Concat("Processing license file: ", licenseFileName, "..."));
            string licenseData = ResourceHelper.ReadResourceAsString(licenseFileName);

            string clientIdFileName = "GracenoteClientId.xml";
            Logger.LogMessage(string.Concat("Processing client file: ", clientIdFileName, "..."));
            XmlDocument doc = ResourceHelper.ReadResourceAsXml(clientIdFileName);

            GracenoteClientIdData clientIdData = new GracenoteClientIdData()
            {
                ClientId = doc.SelectSingleNode("//ClientId")?.InnerText,
                ClientTag = doc.SelectSingleNode("//ClientTag")?.InnerText,
                AppVersion = doc.SelectSingleNode("//AppVersion")?.InnerText,
                License = licenseData,
            };

            Assert.AreEqual("1965581575", clientIdData.ClientId, "clientIdData.ClientId");
            Assert.AreEqual("981037DD61C65554E5C1547086EC1376", clientIdData.ClientTag, "clientIdData.ClientTag");

            ISessionFactory factory = new GracenoteSessionFactory(clientIdData);
            Assert.IsNotNull(factory, "factory");

            SessionOptions options = GetSessionOptions();
            Assert.IsNotNull(options, "options");

            ISession session = await factory.CreateSessionAsync(options);
            Assert.IsNotNull(session, "session");

            return session;
        }
    }
}
