//-----------------------------------------------------------------------
// <copyright file="IAppServiceClient.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.AppService
{
    using System.Threading.Tasks;

    /// <summary>
    /// The client for the application service.
    /// </summary>
    public interface IAppServiceClient
    {
        /// <summary>
        /// Gets a value indicating whether the client connection is open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Open a connection to the application service.
        /// </summary>
        /// <returns>True if the service was opened; false otherwise.</returns>
        Task<bool> OpenAsync();

        /// <summary>
        /// Open a connection to the application service.
        /// </summary>
        void Close();

        /// <summary>
        /// Start a  new session.
        /// </summary>
        /// <param name="session">The session proxy.</param>
        /// <param name="options">The session options.</param>
        /// <returns>The session id.</returns>
        Task<string> StartSessionAsync(ISessionProxy session, SessionOptions options);

        /// <summary>
        /// Send a sample to the application service.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="sample">The sample to send.</param>
        /// <returns>A task which can be awaited.</returns>
        Task SendAudioSampleAsync(string sessionId, byte[] sample);

        /// <summary>
        /// End a session.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <returns>A task which can be awaited.</returns>
        Task EndSessionAsync(string sessionId);
    }
}