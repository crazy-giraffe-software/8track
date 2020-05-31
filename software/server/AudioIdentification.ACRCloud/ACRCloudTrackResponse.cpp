//-----------------------------------------------------------------------
// <copyright file="ACRCloudTrackResponse.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "ACRCloudTrackResponse.h"

using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace CrazyGiraffe::AudioIdentification;
using namespace CrazyGiraffe::AudioIdentification::ACRCloud;

ACRCloudTrackResponse::ACRCloudTrackResponse(
    String^ message,
    String^ version,
    int16 code,
    IVectorView<IReadOnlyTrack^>^ tracks)
{
    this->m_message = message;
    this->m_version = version;
    this->m_code = code;
    this->m_tracks = tracks;
}

String^ ACRCloudTrackResponse::Message::get()
{
    return m_message;
}

String^ ACRCloudTrackResponse::Version::get()
{
    return m_version;
}

int16 ACRCloudTrackResponse::Code::get()
{
    return m_code;
}

IVectorView<IReadOnlyTrack^>^ ACRCloudTrackResponse::Tracks::get()
{
    return m_tracks;
}
