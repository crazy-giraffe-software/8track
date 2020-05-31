//-----------------------------------------------------------------------
// <copyright file="ResourceHelper.cs" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
namespace CrazyGiraffe.AudioIdentification.Gracenote.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Helper class for embedded resources.
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Reads the contents of an resource file.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>byte array.</returns>
        public static byte[] ReadResourceAsByteArray(string name)
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
        public static string ReadResourceAsString(string name)
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
        public static XmlDocument ReadResourceAsXml(string name)
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
    }
}
