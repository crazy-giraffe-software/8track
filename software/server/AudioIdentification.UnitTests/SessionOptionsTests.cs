//-----------------------------------------------------------------------
// <copyright file="SessionOptionsTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.UnitTests
{
    using CrazyGiraffe.AudioIdentification;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// Test class to test <see cref="SessionOptions"/>.
    /// </summary>
    [TestClass]
    public class SessionOptionsTests
    {
        /// <summary>
        /// Test the ability create an <see cref="SessionOptions"/>.
        /// </summary>
        [TestMethod]
        public void SessionOptionsSuccess()
        {
            ushort audioSampleRate = 11025;
            ushort audioSampleSize = 8;
            ushort audioChannels = 1;
            SessionOptions options = new SessionOptions(audioSampleRate, audioSampleSize, audioChannels);

            Assert.IsNotNull(options, "options");
            Assert.AreEqual(audioChannels, options.ChannelCount, "ChannelCount");
            Assert.AreEqual(audioSampleRate, options.SampleRate, "SampleRate");
            Assert.AreEqual(audioSampleSize, options.SampleSize, "SampleSize");
        }

        /// <summary>
        /// Test the ability create an <see cref="SessionOptions"/> via serialization.
        /// </summary>
        [TestMethod]
        public void SessionOptionsSerialization()
        {
            ushort audioSampleRate = 32000;
            ushort audioSampleSize = 8;
            ushort audioChannels = 1;
            SessionOptions options = new SessionOptions(audioSampleRate, audioSampleSize, audioChannels);
            Assert.IsNotNull(options, "options");

            string asJson = JsonConvert.SerializeObject(options);
            Assert.IsNotNull(asJson, "asJson");

            SessionOptions newOptions = JsonConvert.DeserializeObject<SessionOptions>(asJson);
            Assert.IsNotNull(newOptions, "newOptions");

            Assert.AreEqual(audioChannels, newOptions.ChannelCount, "ChannelCount");
            Assert.AreEqual(audioSampleRate, newOptions.SampleRate, "SampleRate");
            Assert.AreEqual(audioSampleSize, newOptions.SampleSize, "SampleSize");
        }
    }
}
