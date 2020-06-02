//-----------------------------------------------------------------------
// <copyright file="AcoustIDClientIdDataTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.AcoustID.UnitTests
{
    using CrazyGiraffe.AudioIdentification.AcoustID;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A class to test <see cref="AcoustIDClientIdData"/>.
    /// </summary>
    [TestClass]
    public class AcoustIDClientIdDataTests
    {
        /// <summary>
        /// Test the ability create an <see cref="AcoustIDClientIdData"/>.
        /// </summary>
        [TestMethod]
        public void ACRCloudClientIdDataSuccess()
        {
            string apikey = "APIKey";
            AcoustIDClientIdData cientIdData = new AcoustIDClientIdData()
            {
                APIKey = apikey,
            };

            Assert.IsNotNull(cientIdData, "options");
            Assert.AreEqual(apikey, cientIdData.APIKey, "ClientKey");
        }
    }
}
