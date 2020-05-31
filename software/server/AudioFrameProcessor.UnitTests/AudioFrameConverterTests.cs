//-----------------------------------------------------------------------
// <copyright file="AudioFrameConverterTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioFrameProcessor.UnitTests
{
    using System;
    using CrazyGiraffe.AudioFrameProcessor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Media.MediaProperties;

    /// <summary>
    /// Tests for <see cref="AudioFrameConverter"/>.
    /// </summary>
    [TestClass]
    public class AudioFrameConverterTests
    {
        /// <summary>
        /// Test the ability to create an AudioFrameConverter.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterCreateTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 16);
            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");
        }

        /// <summary>
        /// Test the ability to create an AudioFrameConverter with null properties.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterCreateNullProperties()
        {
            Assert.ThrowsException<ArgumentException>(() => new AudioFrameConverter(null));
        }

        /// <summary>
        /// Test the ability to create an AudioFrameConverter with invalid properties.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterCreateInvalidProperties()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreateFlac(44100, 2, 16);
            Assert.ThrowsException<ArgumentException>(() => new AudioFrameConverter(properties));
        }

        /// <summary>
        /// Test the ability to create an AudioFrameConverter with invalid sample size.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterCreateInvalidSampleSize()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 14);
            Assert.ThrowsException<ArgumentException>(() => new AudioFrameConverter(properties));
        }

        /// <summary>
        /// Test the ability to convert an audio frame via AudioFrameConverter.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterTo8BitByteArrayTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 8);
            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");

            float fixedValue = 0.5f;
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            byte[] bytes = converter.ToByteArray(frame.CurrentFrame);
            Assert.AreEqual((int)(frame.Capacity / 4), bytes.Length, "bytes.Length");

            byte[] expectedBytes = BitConverter.GetBytes((byte)(0xff * fixedValue));
            Assert.AreEqual(expectedBytes[0], bytes[0], "bytes[0]");
        }

        /// <summary>
        /// Test the ability to convert an audio frame via AudioFrameConverter.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterTo16BitByteArrayTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 16);
            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");

            float fixedValue = 0.5f;
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            byte[] bytes = converter.ToByteArray(frame.CurrentFrame);
            Assert.AreEqual((int)(frame.Capacity / 2), bytes.Length, "bytes.Length");

            byte[] expectedBytes = BitConverter.GetBytes((ushort)(0xffff * fixedValue));
            Assert.AreEqual(expectedBytes[0], bytes[0], "bytes[0]");
            Assert.AreEqual(expectedBytes[1], bytes[1], "bytes[1]");
        }

        /// <summary>
        /// Test the ability to convert an audio frame via AudioFrameConverter.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterTo24BitByteArrayTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 24);
            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");

            float fixedValue = 0.5f;
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            byte[] bytes = converter.ToByteArray(frame.CurrentFrame);
            Assert.AreEqual((int)(frame.Capacity / 4 * 3), bytes.Length, "bytes.Length");

            byte[] expectedBytes = BitConverter.GetBytes((uint)(0xffffff * fixedValue));
            Assert.AreEqual(expectedBytes[0], bytes[0], "bytes[0]");
            Assert.AreEqual(expectedBytes[1], bytes[1], "bytes[1]");
            Assert.AreEqual(expectedBytes[2], bytes[2], "bytes[2]");
        }

        /// <summary>
        /// Test the ability to convert an audio frame via AudioFrameConverter.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterTo32BitByteArrayTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 32);
            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");

            float fixedValue = 0.5f;
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            byte[] bytes = converter.ToByteArray(frame.CurrentFrame);
            Assert.AreEqual((int)frame.Capacity, bytes.Length, "bytes.Length");

            byte[] expectedBytes = BitConverter.GetBytes((uint)(0xffffffff * fixedValue));
            Assert.AreEqual(expectedBytes[0], bytes[0], "bytes[0]");
            Assert.AreEqual(expectedBytes[1], bytes[1], "bytes[1]");
            Assert.AreEqual(expectedBytes[2], bytes[2], "bytes[2]");
            Assert.AreEqual(expectedBytes[3], bytes[3], "bytes[3]");
        }

        /// <summary>
        /// Test the ability to convert an audio frame via AudioFrameConverter.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterToFloatByteArrayTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 32);
            properties.Subtype = "Float";

            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");

            float fixedValue = 0.5f;
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            byte[] bytes = converter.ToByteArray(frame.CurrentFrame);
            Assert.AreEqual((int)frame.Capacity, bytes.Length, "bytes.Length");

            byte[] expectedBytes = BitConverter.GetBytes(fixedValue);
            Assert.AreEqual(expectedBytes[0], bytes[0], "bytes[0]");
            Assert.AreEqual(expectedBytes[1], bytes[1], "bytes[1]");
            Assert.AreEqual(expectedBytes[2], bytes[2], "bytes[2]");
            Assert.AreEqual(expectedBytes[3], bytes[3], "bytes[3]");
        }

        /// <summary>
        /// Test the ability of AudioFrameConverter to handle a null audio frame.
        /// </summary>
        [TestMethod]
        public void AudioFrameConverterNullFrameTest()
        {
            AudioEncodingProperties properties = AudioEncodingProperties.CreatePcm(44100, 2, 16);
            AudioFrameConverter converter = new AudioFrameConverter(properties);
            Assert.IsNotNull(converter, "converter");

            byte[] bytes = converter.ToByteArray(null);
            Assert.IsNull(bytes, "bytes");
        }
    }
}
