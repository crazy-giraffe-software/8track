//-----------------------------------------------------------------------
// <copyright file="Track.h" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#pragma once

namespace CrazyGiraffe { namespace AudioIdentification
{
    /// <summary>
    /// Track representing an identified song.
    /// </summary>
    public interface class IReadOnlyTrack
    {
    public:
        /// <summary>
        /// Gets the identifier of the track.
        /// </summary>
        property Platform::String^ Identifier
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the title of the track.
        /// </summary>
        property Platform::String^ Title
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the name of the artist for the track.
        /// </summary>
        property Platform::String^ Artist
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the title of the track's album.
        /// </summary>
        property Platform::String^ Album
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the genre of the track.
        /// </summary>
        property Platform::String^ Genre
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the cover art Uri.
        /// </summary>
        property Windows::Foundation::Uri^ CovertArtImage
        {
            Windows::Foundation::Uri^ get();
        }

        /// <summary>
        /// Gets the match confidence of the track.
        /// </summary>
        property Platform::String^ MatchConfidence
        {
            Platform::String^ get();
        }

        /// <summary>
        /// Gets the duration of the track in milliseconds.
        /// </summary>
        property int32 Duration
        {
            int32 get();
        }

        /// <summary>
        /// Gets the match position of the track in milliseconds.
        /// </summary>
        property int32 MatchPosition
        {
            int32 get();
        }

        /// <summary>
        /// Gets the current position of the track in milliseconds.
        /// </summary>
        property int32 CurrentPosition
        {
            int32 get();
        }
    };

    /// <summary>
     /// Track representing an identified song.
     /// </summary>
    public interface class ITrack : IReadOnlyTrack
    {
    public:
        /// <summary>
        /// Gets the identifier of the track.
        /// </summary>
        property Platform::String^ Identifier
        {
            void set(Platform::String^ value);
        }

        /// <summary>
        /// Gets the title of the track.
        /// </summary>
        property Platform::String^ Title
        {
            void set(Platform::String^ value);
        }

        /// <summary>
        /// Gets the name of the artist for the track.
        /// </summary>
        property Platform::String^ Artist
        {
            void set(Platform::String^ value);
        }

        /// <summary>
        /// Gets the title of the track's album.
        /// </summary>
        property Platform::String^ Album
        {
            void set(Platform::String^ value);
        }

        /// <summary>
        /// Gets the genre of the track.
        /// </summary>
        property Platform::String^ Genre
        {
            void set(Platform::String^ value);
        }

        /// <summary>
        /// Gets the cover art Uri.
        /// </summary>
        property Windows::Foundation::Uri^ CovertArtImage
        {
            void set(Windows::Foundation::Uri^ value);
        }

        /// <summary>
        /// Gets the match confidence of the track.
        /// </summary>
        property Platform::String^ MatchConfidence
        {
            void set(Platform::String^ value);
        }

        /// <summary>
        /// Gets the duration of the track in milliseconds.
        /// </summary>
        property int32 Duration
        {
            void set(int32 value);
        }

        /// <summary>
        /// Gets the match position of the track in milliseconds.
        /// </summary>
        property int32 MatchPosition
        {
            void set(int32 value);
        }

        /// <summary>
        /// Gets the current position of the track in milliseconds.
        /// </summary>
        property int32 CurrentPosition
        {
            void set(int32 value);
        }
    };

    /// <summary>
    /// Track representing an identified song.
    /// </summary>
    public ref class Track sealed : public ITrack
    {
    public:
        /// <summary>
        /// Create an instance of the <see cref="Track" /> class.
        /// </summary>
        Track();

        /// <summary>
        /// Gets the identifier of the track.
        /// </summary>
        virtual property Platform::String^ Identifier
        {
            Platform::String^ get();
            void set(Platform::String^);
        }

        /// <summary>
        /// Gets the title of the track.
        /// </summary>
        virtual property Platform::String^ Title
        {
            Platform::String^ get();
            void set(Platform::String^);
        }

        /// <summary>
        /// Gets the name of the artist for the track.
        /// </summary>
        virtual property Platform::String^ Artist
        {
            Platform::String^ get();
            void set(Platform::String^);
        }

        /// <summary>
        /// Gets the title of the track's album.
        /// </summary>
        virtual property Platform::String^ Album
        {
            Platform::String^ get();
            void set(Platform::String^);
        }

        /// <summary>
        /// Gets the genre of the track.
        /// </summary>
        virtual property Platform::String^ Genre
        {
            Platform::String^ get();
            void set(Platform::String^);
        }

        /// <summary>
        /// Gets the cover art Uri.
        /// </summary>
        virtual property Windows::Foundation::Uri^ CovertArtImage
        {
            Windows::Foundation::Uri^ get();
            void set(Windows::Foundation::Uri^);
        }

        /// <summary>
        /// Gets the match confidence of the track.
        /// </summary>
        virtual property Platform::String^ MatchConfidence
        {
            Platform::String^ get();
            void set(Platform::String^);
        }

        /// <summary>
        /// Gets the duration of the track in milliseconds.
        /// </summary>
        virtual property int32 Duration
        {
            int32 get();
            void set(int32);
        }

        /// <summary>
        /// Gets the match position of the track in milliseconds.
        /// </summary>
        virtual property int32 MatchPosition
        {
            int32 get();
            void set(int32);
        }

        /// <summary>
        /// Gets the current position of the track in milliseconds.
        /// </summary>
        virtual property int32 CurrentPosition
        {
            int32 get();
            void set(int32);
        }

    private:
        /// <summary>
        /// The identifier of the track.
        /// </summary>
        Platform::String^ m_identifier;

        /// <summary>
        /// The title of the track.
        /// </summary>
        Platform::String^ m_title;

        /// <summary>
        /// The name of the artist for the track.
        /// </summary>
        Platform::String^ m_artist;

        /// <summary>
        /// The title of the track's album.
        /// </summary>
        Platform::String^ m_album;

        /// <summary>
        /// The genre of the track's album.
        /// </summary>
        Platform::String^ m_genre;

        /// <summary>
        /// The cover art Uri.
        /// </summary>
        Windows::Foundation::Uri^ m_covertArtImage;

        /// <summary>
        /// The duration of the track in milliseconds.
        /// </summary>
        int32 m_duration;

        /// <summary>
        /// The match position of the track in milliseconds.
        /// </summary>
        int32 m_matchPosition;

        /// <summary>
        /// The match confidence of the track.
        /// </summary>
        Platform::String^ m_matchConfidence;

        /// <summary>
        /// The current position of the track in milliseconds.
        /// </summary>
        int32 m_currentPosition;
    };
} }
