//-----------------------------------------------------------------------
// <copyright file="GracenoteTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ScenarioTests.Gracenote
{
    using System.Threading.Tasks;
    using System.Xml;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.Gracenote;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

    /// <summary>
    /// An end to end test for Gracenote.
    /// </summary>
    [TestClass]
    public class GracenoteTests : AudioIdentificationTestBase
    {
        /// <summary>
        /// Create a session factory.
        /// </summary>
        /// <returns>ISessionFactory.</returns>
        protected override Task<ISessionFactory> GetSessionFactoryAsync()
        {
            Assert.Inconclusive("Seems to be a problem with the gracenote client ID");

            string licenseFileName = "License.txt";
            Logger.LogMessage(string.Concat("Processing license file: ", licenseFileName, "..."));
            string licenseData = ReadResourceAsString(licenseFileName);

            string clientIdFileName = "GracenoteClientId.xml";
            Logger.LogMessage(string.Concat("Processing client file: ", clientIdFileName, "..."));
            XmlDocument doc = ReadResourceAsXml(clientIdFileName);

            GracenoteClientIdData clientIdData = new GracenoteClientIdData()
            {
                ClientId = doc.SelectSingleNode("//ClientId")?.InnerText,
                ClientTag = doc.SelectSingleNode("//ClientTag")?.InnerText,
                AppVersion = doc.SelectSingleNode("//AppVersion")?.InnerText,
                License = licenseData,
            };

            if (string.IsNullOrEmpty(clientIdData.ClientId) ||
                clientIdData.ClientId == "ClientId" ||
                string.IsNullOrEmpty(clientIdData.ClientTag) ||
                clientIdData.ClientTag == "ClientTag" ||
                string.IsNullOrEmpty(clientIdData.AppVersion) ||
                clientIdData.AppVersion == "AppVersion" ||
                string.IsNullOrEmpty(clientIdData.License) ||
                clientIdData.License == "License")
            {
                Assert.Inconclusive("GracenoteClientId.xml should contain your account details. Register at https://www.gracenote.com/dev-zone/");
            }

            Logger.LogMessage("Create factory...");
            return Task.FromResult<ISessionFactory>(new GracenoteSessionFactory(clientIdData));
        }
    }
}
