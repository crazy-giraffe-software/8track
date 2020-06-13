//-----------------------------------------------------------------------
// <copyright file="AudioCaptureTests.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCaptureUnitTests
{
    using System;
    using System.Threading.Tasks;
    using CrazyGiraffe.AudioCapture;
    using CrazyGiraffe.AudioFrameProcessor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Media;

    /// <summary>
    /// Test class to test <see cref="AudioCapture"/>.
    /// </summary>
    /// <remarks>
    /// This uses an audio capture device. You must allow the usage of the "Microphone" on first run.
    /// </remarks>
    [TestClass]
    public class AudioCaptureTests
    {
        /// <summary>
        /// The time allowed for the capture to start and/or stop in milliseconds.
        /// </summary>
        private const int CaptureStartStopTimeout = 5000;

        /// <summary>
        /// The time allowed for calling ProcessAudioframe in milliseconds.
        /// </summary>
        private const int ProcessAudioFrameTimeout = 1000;

        /// <summary>
        /// Test the ability create a capture without exception.
        /// </summary>
        [TestMethod]
        public void AudioCaptureTest()
        {
            // Create a capture.
            using (IAudioCapturePipeline capture = new AudioCapturePipeline())
            {
                Assert.IsNotNull(capture);
                Assert.IsNull(capture.CaptureDeviceIdentifier);
                Assert.IsNull(capture.RenderDeviceIdentifier);
            }
        }

        /// <summary>
        /// Test the ability create, start and stop a capture without exception.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public void AudioCaptureStartStopTest()
        {
            // Create a capture.
            using (IAudioCapturePipeline capture = new AudioCapturePipeline())
            {
                capture.CaptureDeviceIdentifier = Utilities.GetInputDevice().Id;
                capture.RenderDeviceIdentifier = Utilities.GetOutputDevice().Id;

                // Connect an event handler for status changes.
                var eventTaskCompletionSource = new TaskCompletionSource<bool>();
                capture.CaptureStateChanged += new EventHandler((object o, EventArgs e) =>
                {
                    eventTaskCompletionSource.TrySetResult(true);
                });

                // Start the capture.
                eventTaskCompletionSource = new TaskCompletionSource<bool>();
                Assert.IsTrue(capture.StartAsync().Wait(CaptureStartStopTimeout), "StartAsync");
                Assert.IsTrue(eventTaskCompletionSource.Task.Wait(CaptureStartStopTimeout), "StateChange");
                Assert.AreEqual(AudioCaptureState.Running, capture.CaptureState);

                // Stop the capture.
                eventTaskCompletionSource = new TaskCompletionSource<bool>();
                Assert.IsTrue(capture.StopAsync().Wait(CaptureStartStopTimeout), "StopAsync");
                Assert.IsTrue(eventTaskCompletionSource.Task.Wait(CaptureStartStopTimeout), "StateChange");
                Assert.AreEqual(AudioCaptureState.Idle, capture.CaptureState);
            }
        }

        /// <summary>
        /// Test the ability create and start a capture twice without exception.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public void AudioCaptureStartWithoutStopTest()
        {
            // Create a capture.
            using (IAudioCapturePipeline capture = new AudioCapturePipeline())
            {
                capture.CaptureDeviceIdentifier = Utilities.GetInputDevice().Id;
                capture.RenderDeviceIdentifier = Utilities.GetOutputDevice().Id;

                // Start the capture twice.
                Assert.IsTrue(capture.StartAsync().Wait(CaptureStartStopTimeout), "StartAsync");
                Assert.IsTrue(capture.StartAsync().Wait(CaptureStartStopTimeout), "StartAsync2");
                Assert.IsTrue(capture.StopAsync().Wait(CaptureStartStopTimeout), "StopAsync");
            }
        }

        /// <summary>
        /// Test the ability create and stop a capture twice without exception.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public void AudioCaptureStopWithoutStartTest()
        {
            // Create a capture.
            using (IAudioCapturePipeline capture = new AudioCapturePipeline())
            {
                capture.CaptureDeviceIdentifier = Utilities.GetInputDevice().Id;
                capture.RenderDeviceIdentifier = Utilities.GetOutputDevice().Id;

                // Stop the capture twice.
                Assert.IsTrue(capture.StopAsync().Wait(CaptureStartStopTimeout));
                Assert.IsTrue(capture.StopAsync().Wait(CaptureStartStopTimeout));
            }
        }

        /// <summary>
        /// Test the ability create, start and stop a capture without exception
        /// without setting the capture device (use default).
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public void AudioCaptureNoCaptureDeviceTest()
        {
            // Create a capture.
            using (IAudioCapturePipeline capture = new AudioCapturePipeline())
            {
                capture.RenderDeviceIdentifier = Utilities.GetOutputDevice().Id;

                // Start and stop the capture.
                Assert.IsTrue(capture.StartAsync().Wait(CaptureStartStopTimeout), "Start");
                Assert.IsTrue(capture.StopAsync().Wait(CaptureStartStopTimeout), "Stop");
            }
        }

        /// <summary>
        /// Test the ability create, start and stop a capture without exception
        /// without setting the render device (use default).
        /// </summary>
        [TestMethod]
        public void AudioCaptureNoRenderDeviceTest()
        {
            // Create a capture.
            using (IAudioCapturePipeline capture = new AudioCapturePipeline())
            {
                capture.CaptureDeviceIdentifier = Utilities.GetInputDevice().Id;

                // Start and stop the capture.
                Assert.IsTrue(capture.StartAsync().Wait(CaptureStartStopTimeout));
                Assert.IsTrue(capture.StopAsync().Wait(CaptureStartStopTimeout));
            }
        }

        /// <summary>
        /// Test the capture for the ability to pass data to the music id as expected.
        /// </summary>
        [TestMethod]
        public void AudioCapturePluginTest()
        {
            // Create a capture.
            using (TestableAudioCapture capture = new TestableAudioCapture())
            {
                TestableAudioCapturePlugin plugin = new TestableAudioCapturePlugin();
                capture.CapturePlugins.Add(plugin);

                // Connect an event handler for status changes.
                var pluginTaskCompletionSource = new TaskCompletionSource<bool>();
                plugin.ProcessAudioFrameTaskCompletionSource = pluginTaskCompletionSource;

                // Inject an audio frame.
                WrappedAudioFrame mockFrame = WrappedAudioFrame.CreateFixed(0.01f);
                capture.InjectAudioFrame(mockFrame.CurrentFrame);

                // Now, wait for the frame to be processed.
                Assert.IsTrue(pluginTaskCompletionSource.Task.Wait(ProcessAudioFrameTimeout));
            }
        }

        /// <summary>
        /// Class for testing AudioCapture.
        /// </summary>
        private class TestableAudioCapture : AudioCapturePipeline
        {
            /// <summary>
            /// Finalizes an instance of the <see cref="TestableAudioCapture"/> class.
            /// </summary>
            ~TestableAudioCapture()
            {
                // Need to call this to stop the server.
                this.StopAsync().Wait();
            }

            /// <summary>
            /// Test the process audio data method.
            /// </summary>
            /// <param name="frame">The audio frame to process.</param>
            public void InjectAudioFrame(AudioFrame frame)
            {
                this.ProcessAudioFrame(frame);
            }
        }

        /// <summary>
        /// Class for testing AudioCapture.
        /// </summary>
        private class TestableAudioCapturePlugin : IAudioCapturePlugin
        {
            /// <summary>
            /// Gets or sets a task completion source to indicate that ProcessAudioFrame was called.
            /// </summary>
            public TaskCompletionSource<bool> ProcessAudioFrameTaskCompletionSource { get; set; }

            /// <summary>
            /// Process audio data from the graph.
            /// </summary>
            /// <param name="frame">The audio frame to process.</param>
            public void ProcessAudioFrame(AudioFrame frame)
            {
                if (this.ProcessAudioFrameTaskCompletionSource != null)
                {
                    this.ProcessAudioFrameTaskCompletionSource.TrySetResult(true);
                }
            }
        }
    }
}
