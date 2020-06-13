//-----------------------------------------------------------------------
// <copyright file="AudioCapturePipeline.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioCapture
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Media;
    using Windows.Media.Audio;
    using Windows.Media.MediaProperties;
    using Windows.Media.Render;

    /// <summary>
    /// Class for capturing audio.
    /// </summary>
    public class AudioCapturePipeline : IAudioCapturePipeline
    {
        /// <summary>
        /// The time allow for the server to start and/or stop in milliseconds.
        /// </summary>
        private const int CaptureStartStopTimeout = 1000;

        /// <summary>
        /// The audio graph for capturing audio samples.
        /// </summary>
        private AudioGraph audioGraph;

        /// <summary>
        /// A frame output node for capturing samples.
        /// </summary>
        private AudioFrameOutputNode frameOutputNode;

        /// <summary>
        /// To detect redundant calls to Dispose().
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioCapturePipeline" /> class.
        /// </summary>
        public AudioCapturePipeline()
        {
            // Capture is idle.
            this.CaptureState = AudioCaptureState.Idle;

            // Set some default encoding properties.
            this.EncodingProperties = new AudioEncodingProperties
            {
                SampleRate = 44100,
                BitsPerSample = 16,
                ChannelCount = 2,
            };

            // Start with an empty list of plug-ins.
            this.CapturePlugins = new List<IAudioCapturePlugin>();
        }

        /// <inheritdoc />
        public event EventHandler CaptureStateChanged;

        /// <inheritdoc />
        public AudioCaptureState CaptureState { get; private set; }

        /// <inheritdoc />
        public AudioEncodingProperties EncodingProperties { get; private set; }

        /// <inheritdoc />
        public string CaptureDeviceIdentifier { get; set; }

        /// <inheritdoc />
        public string RenderDeviceIdentifier { get; set; }

        /// <inheritdoc />
        public IList<IAudioCapturePlugin> CapturePlugins { get; private set; }

        /// <inheritdoc />
        public async Task StartAsync()
        {
            if (this.CaptureState == AudioCaptureState.Idle)
            {
                try
                {
                    this.UpdateCaptureState(AudioCaptureState.Starting);
                    await this.StartInternalAsync().ConfigureAwait(false);
                    this.UpdateCaptureState(AudioCaptureState.Running);
                }
                catch (Exception)
                {
                    this.UpdateCaptureState(AudioCaptureState.Idle);
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (this.CaptureState == AudioCaptureState.Running)
            {
                try
                {
                    this.UpdateCaptureState(AudioCaptureState.Stopping);
                    await this.StopInternalAsync().ConfigureAwait(false);
                }
                finally
                {
                    this.UpdateCaptureState(AudioCaptureState.Idle);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        /// <param name="disposing">true of disposing; false is finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (this.CaptureState == AudioCaptureState.Running)
                {
                    int graphTimeout = 5000;
                    this.StopInternalAsync().Wait(graphTimeout);
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Process audio frame from the graph.
        /// </summary>
        /// <param name="frame">The audio frame.</param>
        protected void ProcessAudioFrame(AudioFrame frame)
        {
            foreach (IAudioCapturePlugin plugin in this.CapturePlugins)
            {
                try
                {
                    plugin.ProcessAudioFrame(frame);
                }
                catch (Exception ex)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "IAudioCapturePlugin {0} threw exception={1}.", plugin.GetType().Name, ex.Message);
                    Debug.WriteLine(message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get an audio device from an identifier.
        /// </summary>
        /// <param name="identifier">The device identifier.</param>
        /// <returns>An audio device or null.</returns>
        private static async Task<DeviceInformation> GetAudioDeviceAsync(string identifier)
        {
            // Convert Id to device.
            DeviceInformation audioDevice = null;
            if (!string.IsNullOrEmpty(identifier))
            {
                try
                {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Get capture device from id={0}.", identifier));
                    audioDevice = await AudioDevice.GetDeviceAsync(identifier).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "GetDeviceAsync failed={0}.", ex.Message);
                    Debug.WriteLine(message);
                    throw;
                }
            }

            return audioDevice;
        }

        /// <summary>
        /// Start the audio capture.
        /// </summary>
        /// <returns>A task on which to wait for completion.</returns>
        private Task StartInternalAsync()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(CaptureStartStopTimeout);

                // Create the graph
                return Task.Run(
                    async () =>
                    {
                        // Stop graph is needed
                        await this.StopInternalAsync().ConfigureAwait(false);
                        if (this.audioGraph != null)
                        {
                            this.audioGraph.Dispose();
                            this.audioGraph = null;
                        }

                        Debug.WriteLine("Creating audio graph.");
                        AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Other)
                        {
                            QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency,
                            PrimaryRenderDevice = await GetAudioDeviceAsync(this.RenderDeviceIdentifier).ConfigureAwait(false),
                        };

                        CreateAudioGraphResult graphResult = await AudioGraph.CreateAsync(settings);
                        if (graphResult.Status != AudioGraphCreationStatus.Success)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "graphResult.Status={0}.", graphResult.Status.ToString());
                            Debug.WriteLine(message);
                            throw new Exception(message);
                        }

                        this.audioGraph = graphResult.Graph;
                        this.EncodingProperties = this.audioGraph.EncodingProperties;

                        // Create an input node
                        Debug.WriteLine("Creating input node.");
                        CreateAudioDeviceInputNodeResult inputNodeResult = await this.audioGraph.CreateDeviceInputNodeAsync(
                            Windows.Media.Capture.MediaCategory.Other,
                            this.EncodingProperties,
                            await GetAudioDeviceAsync(this.CaptureDeviceIdentifier).ConfigureAwait(false));

                        if (inputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "inputNodeResult.Status={0}.", inputNodeResult.Status.ToString());
                            Debug.WriteLine(message);
                            throw new Exception(message);
                        }

                        AudioDeviceInputNode inputNode = inputNodeResult.DeviceInputNode;

                        // Create default output node
                        Debug.WriteLine("Creating output node.");
                        CreateAudioDeviceOutputNodeResult outputNodeResult = await this.audioGraph.CreateDeviceOutputNodeAsync();
                        if (outputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "outputNodeResult.Status={0}.", outputNodeResult.Status.ToString());
                            Debug.WriteLine(message);
                            throw new Exception(message);
                        }

                        AudioDeviceOutputNode outputNode = outputNodeResult.DeviceOutputNode;

                        // Create a frame capture node
                        Debug.WriteLine("Creating frame output node.");
                        try
                        {
                            this.frameOutputNode = this.audioGraph.CreateFrameOutputNode(this.EncodingProperties);
                            this.audioGraph.QuantumStarted += this.AudioGraphQuantumStarted;
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "frameOutputNode failed={0}.", ex.Message);
                            Debug.WriteLine(message);
                            throw new Exception(message, ex);
                        }

                        // Connect inputs to outputs
                        Debug.WriteLine("Connecting nodes.");
                        try
                        {
                            inputNode.AddOutgoingConnection(this.frameOutputNode);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "inputNode.AddOutgoingConnection(this.frameOutputNode)={0}.", ex.Message);
                            Debug.WriteLine(message);
                            throw new Exception(message, ex);
                        }

                        try
                        {
                            inputNode.AddOutgoingConnection(outputNode);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "inputNode.AddOutgoingConnection(this.outputNode)={0}.", ex.Message);
                            Debug.WriteLine(message);
                            throw new Exception(message, ex);
                        }

                        // Start the graph
                        try
                        {
                            this.audioGraph.Start();
                            Debug.WriteLine("Graph started.");
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, "audioGraph.Start failed={0}", ex.Message);
                            Debug.WriteLine(message);
                            throw new Exception(message, ex);
                        }
                    },
                    cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Stop the audio capture.
        /// </summary>
        /// <returns>A task on which to wait for completion.</returns>
        private Task StopInternalAsync()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(CaptureStartStopTimeout);

                return Task.Run(
                    () =>
                    {
                        // Stop graph if needed.
                        if (this.audioGraph != null)
                        {
                            this.audioGraph.Stop();
                            Debug.WriteLine("Graph stopped.");
                        }
                    },
                    cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Process audio data from the graph.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="args">The event arguments.</param>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Don;t throw exception to audio framework.")]
        private void AudioGraphQuantumStarted(AudioGraph sender, object args)
        {
            if (this.CaptureState == AudioCaptureState.Running)
            {
                try
                {
                    using (AudioFrame frame = this.frameOutputNode.GetFrame())
                    {
                        this.ProcessAudioFrame(frame);
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "AudioGraphQuantumStarted threw exception={0}.", ex.Message);
                    Debug.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Update pipeline state.
        /// </summary>
        /// <param name="newState">The new state for the pipeline.</param>
        private void UpdateCaptureState(AudioCaptureState newState)
        {
            // Update.
            bool changed = this.CaptureState != newState;
            this.CaptureState = newState;

            if (changed)
            {
                this.CaptureStateChanged?.Invoke(this, new EventArgs());
            }
        }
    }
}
