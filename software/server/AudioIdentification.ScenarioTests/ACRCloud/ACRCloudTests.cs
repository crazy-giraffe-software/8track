//-----------------------------------------------------------------------
// <copyright file="ACRCloudTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ScenarioTests.ACRCloud
{
    using System.Threading.Tasks;
    using System.Xml;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.ACRCloud;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

    /// <summary>
    /// An end to end test for ACRCloud.
    /// </summary>
    [TestClass]
    public class ACRCloudTests : AudioIdentificationTestBase
    {
        /// <summary>
        /// Create a session factory.
        /// </summary>
        /// <returns>ISessionFactory.</returns>
        protected override Task<ISessionFactory> GetSessionFactoryAsync()
        {
            string fileName = "ACRCloudClientId.xml";
            Logger.LogMessage(string.Concat("Processing client file: ", fileName, "..."));
            XmlDocument doc = ReadResourceAsXml(fileName);

            ACRCloudClientIdData clientIdData = new ACRCloudClientIdData()
            {
                Host = doc.SelectSingleNode("//Host")?.InnerText,
                AccessKey = doc.SelectSingleNode("//AccessKey")?.InnerText,
                AccessSecret = doc.SelectSingleNode("//AccessSecret")?.InnerText,
            };

            if (string.IsNullOrEmpty(clientIdData.Host) ||
                clientIdData.Host == "Host" ||
                string.IsNullOrEmpty(clientIdData.AccessKey) ||
                clientIdData.AccessKey == "AccessKey" ||
                string.IsNullOrEmpty(clientIdData.AccessSecret) ||
                clientIdData.AccessSecret == "AccessSecret")
            {
                Assert.Inconclusive("ACRCloudClientId.xml should contain your account details. Register at https://console.acrcloud.com/signup");
            }

            Logger.LogMessage("Create factory...");
            return Task.FromResult<ISessionFactory>(new ACRCloudSessionFactory(clientIdData));
        }
    }
}
