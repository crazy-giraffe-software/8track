//-----------------------------------------------------------------------
// <copyright file="AcoustIDTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ScenarioTests.AcousticID
{
    using System.Threading.Tasks;
    using System.Xml;
    using CrazyGiraffe.AudioIdentification;
    using CrazyGiraffe.AudioIdentification.AcoustID;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

    /// <summary>
    /// An end to end test for AcoustID.
    /// </summary>
    [TestClass]
    public class AcoustIDTests : AudioIdentificationTestBase
    {
        /// <summary>
        /// Create a session factory.
        /// </summary>
        /// <returns>ISessionFactory.</returns>
        protected override Task<ISessionFactory> GetSessionFactoryAsync()
        {
            string fileName = "AcoustIDClientId.xml";
            Logger.LogMessage(string.Concat("Processing client file: ", fileName, "..."));
            XmlDocument doc = ReadResourceAsXml(fileName);

            AcoustIDClientIdData clientIdData = new AcoustIDClientIdData()
            {
                APIKey = doc.SelectSingleNode("//APIKey")?.InnerText,
            };

            if (string.IsNullOrEmpty(clientIdData.APIKey) ||
                clientIdData.APIKey == "APIKey")
            {
                Assert.Inconclusive("AcoustIDClientId.xml should contain your account details. Register at https://acoustid.org/");
            }

            Logger.LogMessage("Create factory...");
            return Task.FromResult<ISessionFactory>(new AcoustIDSessionFactory(clientIdData));
        }
    }
}
