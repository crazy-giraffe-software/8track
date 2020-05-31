//-----------------------------------------------------------------------
// <copyright file="GracenoteClientIdDataTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Gracenote.UnitTests
{
    using CrazyGiraffe.AudioIdentification.Gracenote;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A class to test <see cref="GracenoteClientIdData"/>.
    /// </summary>
    [TestClass]
    public class GracenoteClientIdDataTests
    {
        /// <summary>
        /// Test the ability create an <see cref="GracenoteClientIdData"/>.
        /// </summary>
        [TestMethod]
        public void GracenoteClientIdDataSuccess()
        {
            string clientId = "ClientId";
            string clientTag = "ClientTag";
            string appVersion = "AppVersion";
            string license = "License";
            GracenoteClientIdData cientIdData = new GracenoteClientIdData()
            {
                ClientId = clientId,
                ClientTag = clientTag,
                AppVersion = appVersion,
                License = license,
            };

            Assert.IsNotNull(cientIdData, "options");
            Assert.AreEqual(clientId, cientIdData.ClientId, "ClientId");
            Assert.AreEqual(clientTag, cientIdData.ClientTag, "ClientTag");
            Assert.AreEqual(appVersion, cientIdData.AppVersion, "AppVersion");
            Assert.AreEqual(license, cientIdData.License, "License");
        }
    }
}
