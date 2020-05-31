//-----------------------------------------------------------------------
// <copyright file="Track.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "Track.h"

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace CrazyGiraffe::AudioIdentification;

Track::Track()
    : m_identifier(L"")
    , m_title(L"")
    , m_artist(L"")
    , m_album(L"")
    , m_genre()
    , m_covertArtImage()
    , m_matchConfidence(L"")
    , m_duration(-1)
    , m_matchPosition(-1)
    , m_currentPosition(-1)
{
}

String^ Track::Identifier::get()
{
    return m_identifier;
}

void Track::Identifier::set(String^ value)
{
    m_identifier = value;
}

String^ Track::Title::get()
{
    return m_title;
}

void Track::Title::set(String^ value)
{
    m_title = value;
}

String^ Track::Artist::get()
{
    return m_artist;
}

void Track::Artist::set(String^ value)
{
    m_artist = value;
}

String^ Track::Album::get()
{
    return m_album;
}

void Track::Album::set(String^ value)
{
    m_album = value;
}

String^ Track::Genre::get()
{
    return m_genre;
}

void Track::Genre::set(String^ value)
{
    m_genre = value;
}

Uri^ Track::CovertArtImage::get()
{
    return m_covertArtImage;
}

void Track::CovertArtImage::set(Uri^ value)
{
    m_covertArtImage = value;
}

String^ Track::MatchConfidence::get()
{
    return m_matchConfidence;
}

void Track::MatchConfidence::set(String^ value)
{
    m_matchConfidence = value;
}

int32 Track::Duration::get()
{
    return m_duration;
}

void Track::Duration::set(int32 value)
{
    m_duration = value;
}

int32 Track::MatchPosition::get()
{
    return m_matchPosition;
}

void Track::MatchPosition::set(int32 value)
{
    m_matchPosition = value;
}

int32 Track::CurrentPosition::get()
{
    return m_currentPosition;
}

void Track::CurrentPosition::set(int32 value)
{
    m_currentPosition = value;
}
