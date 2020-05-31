//-----------------------------------------------------------------------
// <copyright file="Session.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "Session.h"

using namespace concurrency;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace CrazyGiraffe::AudioIdentification;

Session::Session()
    : m_options()
    , m_sessionId(CreateSessionIdentifier())
    , m_status(IdentifyStatus::Invalid)
    , m_tracks(ref new Vector<IReadOnlyTrack^>())
{
}

Session::Session(SessionOptions^ options)
    : m_options(options)
    , m_sessionId(CreateSessionIdentifier())
    , m_status(IdentifyStatus::Invalid)
    , m_tracks(ref new Vector<IReadOnlyTrack^>())
{
}

/* static */
String^ Session::CreateSessionIdentifier()
{
    // GuidHelper::CreateNewGuid().ToString() adds {}.
    std::wstring id(GuidHelper::CreateNewGuid().ToString()->Data());
    id.replace(id.begin(), id.begin() + 1, L"");
    id.replace(id.end() - 1, id.end(), L"");
    return ref new String(id.c_str());
}

String^ Session::SessionIdentifier::get()
{
    return m_sessionId;
}

IdentifyStatus Session::IdentificationStatus::get()
{
    return m_status;
}

void Session::AddAudioSample(const Array<byte>^ audioData)
{
}

IAsyncOperation<IVectorView<IReadOnlyTrack^>^>^ Session::GetTracksAsync()
{
    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    return create_async([this]() -> task<IVectorView<IReadOnlyTrack^>^>
        {
            return task_from_result(m_tracks->GetView());
        });
}
