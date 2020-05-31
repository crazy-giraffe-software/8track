//-----------------------------------------------------------------------
// <copyright file="AudioIdentificationTestBase.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.ScenarioTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using CrazyGiraffe.AudioIdentification;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

    /// <summary>
    /// Base class for identifying audio.
    /// </summary>
    public abstract class AudioIdentificationTestBase
    {
        /// <summary>
        /// Test the ability to match Nirvana's Smells Like Teem Spirit.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task IdenitfySmellsLikeTeenSpirit()
        {
            ISession session = await this.ProcessWavFileAsync(@"teen_spirit_14s.wav").ConfigureAwait(false);
            Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus, "status");

            IReadOnlyList<IReadOnlyTrack> tracks = await session.GetTracksAsync();
            Assert.AreNotEqual(0, tracks.Count, "tracks.Count");

            IReadOnlyTrack track = tracks.Where(t => t.Title.Contains("Smells Like", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            Assert.IsNotNull(track, "track");

            Assert.AreEqual("SMELLS LIKE TEEN SPIRIT", track.Title.ToUpperInvariant(), "track.Title");
            Assert.AreEqual("NIRVANA", track.Album.ToUpperInvariant(), "track.Album");
            Assert.AreEqual("NIRVANA", track.Artist.ToUpperInvariant(), "track.Artist");
        }

        /// <summary>
        /// Test the ability to match Dead Milkmen's (I know ) Where the Tarantula Lives.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        [TestMethod]
        public async Task IdenitfyWhereTheTarantulaLives()
        {
            ISession session = await this.ProcessWavFileAsync(@"WhereTheTarantulaLives.wav").ConfigureAwait(false);
            Assert.AreEqual(IdentifyStatus.Complete, session.IdentificationStatus, "status");

            IReadOnlyList<IReadOnlyTrack> tracks = await session.GetTracksAsync();
            Assert.AreNotEqual(0, tracks.Count, "tracks.Count");

            IReadOnlyTrack track = tracks.Where(t => t.Title.Contains("Tarantula", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            Assert.IsNotNull(track, "track");

            Assert.AreEqual("WHERE THE TARANTULA LIVES", track.Title.ToUpperInvariant(), "track.Title");
            Assert.AreEqual("WHERE THE TARANTULA LIVES", track.Album.ToUpperInvariant(), "track.Album");
            Assert.AreEqual("THE DEAD MILKMEN", track.Artist.ToUpperInvariant(), "track.Artist");
        }

        /// <summary>
        /// Reads the contents of an resource file.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>byte array.</returns>
        protected static byte[] ReadResourceAsByteArray(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
              .Single(str => str.EndsWith(name, StringComparison.InvariantCulture));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream.Length > int.MaxValue)
                {
                    throw new ArgumentException(string.Concat("Stream is too long to read: ", stream.Length));
                }

                int streamLength = (int)stream.Length;
                byte[] buffer = new byte[streamLength];
                for (int i = 0; i < streamLength; i += 100)
                {
                    int bytesLeft = streamLength - i;
                    int size = bytesLeft < 100 ? bytesLeft : 100;
                    stream.Read(buffer, i, size);
                }

                return buffer;
            }
        }

        /// <summary>
        /// Reads the contents of an resource file.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>byte array.</returns>
        protected static string ReadResourceAsString(string name)
        {
            byte[] buffer = ReadResourceAsByteArray(name);
            if (buffer.Length > 2 && buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                return Encoding.UTF8.GetString(buffer.Skip(3).ToArray());
            }

            return Encoding.UTF7.GetString(buffer);
        }

        /// <summary>
        /// Reads the contents of an resource file.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>XmlDocument.</returns>
        protected static XmlDocument ReadResourceAsXml(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
              .Single(str => str.EndsWith(name, StringComparison.InvariantCulture));

            XmlDocument doc = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                doc.Load(stream);
            }

            return doc;
        }

        /// <summary>
        /// Print the session results.
        /// </summary>
        /// <param name="session">The session from which to print results.</param>
        /// <returns>A task which can be awaited.</returns>
        protected static async Task PrintResultsAsync(ISession session)
        {
            try
            {
                Logger.LogMessage(string.Concat("Session Id: ", session?.SessionIdentifier.ToString()));
                Logger.LogMessage(string.Concat("Session Status ", Enum.GetName(typeof(IdentifyStatus), session?.IdentificationStatus)));
                if (session?.IdentificationStatus == IdentifyStatus.Complete)
                {
                    var tracks = await session?.GetTracksAsync();
                    Logger.LogMessage(string.Concat("Track Count: ", tracks.Count));
                    Logger.LogMessage("---------------------------------------------");
                    foreach (IReadOnlyTrack track in tracks)
                    {
                        Logger.LogMessage(string.Concat("Track Fingerprint: ", track.Identifier));
                        Logger.LogMessage("---------------------------------------------");
                        Logger.LogMessage(string.Concat("Track Title: ", track.Title));
                        Logger.LogMessage(string.Concat("Track Album: ", track.Album));
                        Logger.LogMessage(string.Concat("Track Artist: ", track.Artist));
                        Logger.LogMessage(string.Concat("Track Genre: ", track.Genre));
                        Logger.LogMessage(string.Concat("Track CovertArtImage: ", track.CovertArtImage));
                        Logger.LogMessage(string.Concat("Track MatchConfidence: ", track.MatchConfidence));
                        Logger.LogMessage(string.Concat("Track Duration: ", track.Duration));
                        Logger.LogMessage(string.Concat("Track CurrentPosition: ", track.CurrentPosition));
                        Logger.LogMessage(string.Concat("Track MatchPosition: ", track.MatchPosition));
                        Logger.LogMessage("---------------------------------------------");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(string.Concat("Unable to read tracks: ", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Create a session factory.
        /// </summary>
        /// <returns>ISessionFactory.</returns>
        protected abstract Task<ISessionFactory> GetSessionFactoryAsync();

        /// <summary>
        ///  Process a WAV file.
        /// </summary>
        /// <param name="fileName">The WAV file name.</param>
        /// <param name="realtime">True to run the samples in real time, false otherwise.</param>
        /// <returns>A task which can be awaited to receive a session.</returns>
        protected async Task<ISession> ProcessWavFileAsync(string fileName, bool realtime = true)
        {
            ISessionFactory factory;
            ISession session;

            try
            {
                factory = await this.GetSessionFactoryAsync().ConfigureAwait(false);
                Assert.IsNotNull(factory, "factory");
            }
            catch (Exception ex)
            {
                Logger.LogMessage(string.Concat("Unable to create factory: ", ex.Message));
                Assert.AreEqual(null, ex?.ToString(), "factory: ex?.Message");
                throw;
            }

            Logger.LogMessage(string.Concat("Processing file: ", fileName, "..."));
            byte[] fileData = ReadResourceAsByteArray(fileName);

            // Check for PCM file.
            int audioFormat = BitConverter.ToInt16(fileData, 20);
            Assert.AreEqual(1, audioFormat, string.Concat("unable to read non-PCM file: ", audioFormat.ToString(CultureInfo.InvariantCulture)));

            // Read parameters.
            short channelCount = BitConverter.ToInt16(fileData, 22);
            Assert.AreEqual(2, channelCount, string.Concat("ChannelCount not supported: ", channelCount.ToString(CultureInfo.InvariantCulture)));

            int sampleRate = BitConverter.ToInt32(fileData, 24);
            Assert.AreEqual(44100, sampleRate, string.Concat("SampleRate not supported: ", sampleRate.ToString(CultureInfo.InvariantCulture)));

            short sampleSize = BitConverter.ToInt16(fileData, 34);
            Assert.AreEqual(16, sampleSize, string.Concat("SampleSize not supported: ", sampleSize.ToString(CultureInfo.InvariantCulture)));

            // Create session options.
            SessionOptions options = new SessionOptions()
            {
                ChannelCount = (ushort)channelCount,
                SampleRate = (ushort)sampleRate,
                SampleSize = (ushort)sampleSize,
            };

            // Create the session.
            try
            {
                Logger.LogMessage("Create session...");
                session = await factory.CreateSessionAsync(options);
                Assert.IsNotNull(session, "session");
            }
            catch (Exception ex)
            {
                Logger.LogMessage(string.Concat("Unable to create session: ", ex.Message));
                Assert.AreEqual(null, ex?.ToString(), "session: ex?.Message");
                throw;
            }

            // Feed samples to the session.
            try
            {
                int dataIndex = 44;
                int blockSize = 1764; // 176400 bytes per second @ 44.1k, 2 channels, 16 bits per sample, or 1764 bytes per 10 ms.
                do
                {
                    byte[] block = fileData.Skip(dataIndex).Take(blockSize).ToArray();
                    dataIndex += block.Length;

                    Logger.LogMessage(string.Concat("Processing ", block.Length.ToString(CultureInfo.InvariantCulture), "bytes..."));
                    session.AddAudioSample(block);
                    if (realtime)
                    {
                        // Simulate real-time deleivery of samples.
                        Thread.Sleep(10);
                    }
                }
                while (session.IdentificationStatus != IdentifyStatus.Complete &&
                    session.IdentificationStatus != IdentifyStatus.Error &&
                    dataIndex < fileData.Length);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(string.Concat("Unable to create fingerprint: ", ex.Message));
                Assert.AreEqual(null, ex?.ToString(), "sample: ex?.Message");
                throw;
            }

            // Print results and return;
            await PrintResultsAsync(session).ConfigureAwait(false);
            return session;
        }
    }
}
