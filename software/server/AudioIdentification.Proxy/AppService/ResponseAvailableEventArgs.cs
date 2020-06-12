//-----------------------------------------------------------------------
// <copyright file="ResponseAvailableEventArgs.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Proxy.AppService
{
    using System;
    using Windows.Foundation.Collections;

    /// <summary>
    /// Event arguments for ResponseAvailable.
    /// </summary>
    public class ResponseAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseAvailableEventArgs" /> class.
        /// </summary>
        /// <param name="response">The response value set.</param>
        public ResponseAvailableEventArgs(ValueSet response)
        {
            this.Response = response;
        }

        /// <summary>
        /// Gets the response from the service.
        /// </summary>
        public ValueSet Response { get; private set; }
    }
}
