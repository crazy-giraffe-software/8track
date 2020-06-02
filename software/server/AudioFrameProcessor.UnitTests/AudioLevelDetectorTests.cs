//-----------------------------------------------------------------------
// <copyright file="AudioLevelDetectorTests.cs" company="CrazyGiraffeSoftware.net">
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
    /// Tests for <see cref="AudioLevelDetector"/>.
    /// </summary>
    [TestClass]
    public class AudioLevelDetectorTests
    {
        /// <summary>
        /// Test the ability to create an AudioLevelDetector.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorCreateTest()
        {
            AudioEncodingProperties properties = new AudioEncodingProperties()
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            double thresholdValue = 0.01;
            TimeSpan thresholdTimeSpan = new TimeSpan(100);

            AudioLevelDetector detector = new AudioLevelDetector(properties, thresholdValue, thresholdTimeSpan);
            Assert.IsNotNull(detector);
        }

        /// <summary>
        /// Test the ability to create an AudioLevelDetector with null properties.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorCreateNullProperties()
        {
            double thresholdValue = 0.01;
            TimeSpan thresholdTimeSpan = new TimeSpan(100);

            Assert.ThrowsException<ArgumentException>(() => new AudioLevelDetector(null, thresholdValue, thresholdTimeSpan));
        }

        /// <summary>
        /// Test the ability to create an AudioLevelDetector with a 0 timespan.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorCreateZeroTimespan()
        {
            AudioEncodingProperties properties = new AudioEncodingProperties()
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            double thresholdValue = 0.01;
            TimeSpan thresholdTimeSpan;

            AudioLevelDetector detector = new AudioLevelDetector(properties, thresholdValue, thresholdTimeSpan);
            Assert.IsNotNull(detector);
        }

        /// <summary>
        /// Test the ability to detect a loud audio frame via AudioLevelDetector.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorAboveThresholdTest()
        {
            AudioEncodingProperties properties = new AudioEncodingProperties()
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            // The frame is 2048 byes, 512 (float) samples, 256 stereo (float) samples, approx 5.8ms at the rate specified.
            // A duration of 8ms should be unknown for 1 frame and above for 2 frames.
            float fixedValue = 0.2f;
            double thresholdValue = fixedValue / 2;

            TimeSpan thresholdTimeSpan = new TimeSpan(80000);
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            AudioLevelDetector detector = new AudioLevelDetector(properties, thresholdValue, thresholdTimeSpan);
            Assert.IsNotNull(detector);

            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status);
            detector.ProcessFrame(frame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status);
            detector.ProcessFrame(frame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.AboveThreshold, (int)detector.Status);
        }

        /// <summary>
        /// Test the ability to detect a quiet audio frame via AudioLevelDetector.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorBelowThresholdTest()
        {
            AudioEncodingProperties properties = new AudioEncodingProperties()
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            // The frame is 2048 byes, 512 (float) samples, 256 stereo (float) samples, approx 5.8ms at the rate specified.
            // A duration of 8ms should be unknown for 1 frame and below for 2 frames.
            float fixedValue = 0.2f;
            double thresholdValue = fixedValue * 2;

            TimeSpan thresholdTimeSpan = new TimeSpan(80000);
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            AudioLevelDetector detector = new AudioLevelDetector(properties, thresholdValue, thresholdTimeSpan);
            Assert.IsNotNull(detector);

            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status);
            detector.ProcessFrame(frame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status);
            detector.ProcessFrame(frame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.BelowThrehold, (int)detector.Status);
        }

        /// <summary>
        /// Test the ability to detect a transition from above to below and back.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorTransitionTest()
        {
            AudioEncodingProperties properties = new AudioEncodingProperties()
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            // The frame is 2048 byes, 512 (float) samples, 256 stereo (float) samples, approx 5.8ms at the rate specified.
            // A duration of 8ms should be unknown for 1 frame and above for 2 frames.
            float fixedValue = 0.2f;
            double thresholdValue = fixedValue;

            TimeSpan thresholdTimeSpan = new TimeSpan(80000);
            WrappedAudioFrame belowThreholdFrame = WrappedAudioFrame.CreateFixed(fixedValue / 2);
            WrappedAudioFrame aboveThreholdFrame = WrappedAudioFrame.CreateFixed(fixedValue * 2);

            AudioLevelDetector detector = new AudioLevelDetector(properties, thresholdValue, thresholdTimeSpan);
            Assert.IsNotNull(detector);

            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status, "A");
            detector.ProcessFrame(belowThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status, "B");
            detector.ProcessFrame(belowThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.BelowThrehold, (int)detector.Status, "C");
            detector.ProcessFrame(aboveThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.BelowThrehold, (int)detector.Status, "D");
            detector.ProcessFrame(aboveThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.AboveThreshold, (int)detector.Status, "E");
            detector.ProcessFrame(belowThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.AboveThreshold, (int)detector.Status, "F");
            detector.ProcessFrame(belowThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.BelowThrehold, (int)detector.Status, "G");
            detector.ProcessFrame(aboveThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.BelowThrehold, (int)detector.Status, "H");
            detector.ProcessFrame(aboveThreholdFrame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.AboveThreshold, (int)detector.Status, "I");
        }

        /// <summary>
        /// Test the ability to detect a quiet audio frame via AudioLevelDetector.
        /// </summary>
        [TestMethod]
        public void AudioLevelDetectorEventTest()
        {
            AudioEncodingProperties properties = new AudioEncodingProperties()
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            // The frame is 2048 byes, 512 (float) samples, 256 stereo (float) samples, approx 5.8ms at the rate specified.
            // A duration of 4ms should be above for 1 frame.
            float fixedValue = 0.2f;
            double thresholdValue = fixedValue / 2;

            TimeSpan thresholdTimeSpan = new TimeSpan(40000);
            WrappedAudioFrame frame = WrappedAudioFrame.CreateFixed(fixedValue);

            AudioLevelDetector detector = new AudioLevelDetector(properties, thresholdValue, thresholdTimeSpan);
            Assert.IsNotNull(detector);

            ThresholdStatus status = ThresholdStatus.Unknown;
            detector.ThreholdDetected += (AudioLevelDetector d, AudioThreholdDetectedEventArgs e) =>
            {
                status = e.Status;
            };

            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)detector.Status);
            Assert.AreEqual((int)ThresholdStatus.Unknown, (int)status);
            detector.ProcessFrame(frame.CurrentFrame);
            Assert.AreEqual((int)ThresholdStatus.AboveThreshold, (int)detector.Status);
            Assert.AreEqual((int)ThresholdStatus.AboveThreshold, (int)status);
        }
    }
}
