//-----------------------------------------------------------------------
// <copyright file="ACRCloudClientIdDataTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ACRCloud.UnitTests
{
    using CrazyGiraffe.AudioIdentification.ACRCloud;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A class for testing <see cref="ACRCloudClientIdData"/>.
    /// </summary>
    [TestClass]
    public class ACRCloudClientIdDataTests
    {
        /// <summary>
        /// Test the ability create an <see cref="ACRCloudClientIdData"/>.
        /// </summary>
        [TestMethod]
        public void ACRCloudClientIdDataSuccess()
        {
            string host = "Host";
            string accessKey = "AccessKey";
            string accessSecret = "AccessSecret";
            ACRCloudClientIdData cientIdData = new ACRCloudClientIdData()
            {
                Host = host,
                AccessKey = accessKey,
                AccessSecret = accessSecret,
            };

            Assert.IsNotNull(cientIdData, "options");
            Assert.AreEqual(host, cientIdData.Host, "Host");
            Assert.AreEqual(accessKey, cientIdData.AccessKey, "AccessKey");
            Assert.AreEqual(accessSecret, cientIdData.AccessSecret, "AccessSecret");
        }
    }
}
