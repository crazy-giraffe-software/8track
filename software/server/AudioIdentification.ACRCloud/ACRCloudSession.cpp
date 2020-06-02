//-----------------------------------------------------------------------
// <copyright file="ACRCloudSession.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "ACRCloudSession.h"
#include "ACRCloudHelpers.h"

using namespace Concurrency;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Security::Cryptography;
using namespace Windows::Storage::Streams;
using namespace Windows::Web::Http;
using namespace Windows::Web::Http::Filters;
using namespace CrazyGiraffe::AudioIdentification;
using namespace CrazyGiraffe::AudioIdentification::ACRCloud;

ACRCloudSession::ACRCloudSession()
    : m_clientdata()
    , m_options()
    , m_bytesPerSecond(0)
    , m_sessionId(Session::CreateSessionIdentifier())
    , m_status(IdentifyStatus::Invalid)
    , m_tracks((ref new Vector<IReadOnlyTrack^>())->GetView())
    , m_audioQueue()
    , m_audioQueueSize(0)
    , m_audioData()
    , m_audioDataTargetSize(0)\
    , m_recognitionTask(create_task([] { task_from_result(); }))
    , m_recognitionAttempts(0)
{
}

void ACRCloudSession::Initialize(ACRCloudClientIdData^ clientdata, IHttpFilter^ httpFilter, SessionOptions^ options)
{
    // Cache the options.
    m_client = ref new ACRCloudClient(clientdata, httpFilter);
    m_clientdata = clientdata;
    m_options = options;

    m_bytesPerSecond = options->ChannelCount * options->SampleRate * options->SampleSize / 8;
}

String^ ACRCloudSession::SessionIdentifier::get()
{
    return m_sessionId;
}

IdentifyStatus ACRCloudSession::IdentificationStatus::get()
{
    return m_status;
}

void ACRCloudSession::AddAudioSample(const Array<byte>^ audioData)
{
    if (m_status != IdentifyStatus::Complete && m_status != IdentifyStatus::Error && audioData != nullptr)
    {
        // Save the audio data.
        std::vector<byte> audioVector(begin(audioData), end(audioData));
        m_audioQueue.push_back(audioVector);
        m_audioQueueSize += audioData->Length;

        // Every three seconds, try recognition on the audio buffer.
        if ((m_audioDataTargetSize + (3 * m_bytesPerSecond)) < m_audioQueueSize)
        {
            m_audioDataTargetSize.store(m_audioQueueSize);
            ProcessAudioSamples(m_audioDataTargetSize);
        }
    }
}

IAsyncOperation<IVectorView<IReadOnlyTrack^>^>^ ACRCloudSession::GetTracksAsync()
{
    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    return create_async([this]() -> task<IVectorView<IReadOnlyTrack^>^>
        {
            return task_from_result(m_tracks);
        });
}

void ACRCloudSession::UpdateStatus(IdentifyStatus newStatus)
{
    // Update.
    bool changed = (m_status != newStatus);
    m_status = newStatus;

    if (changed)
    {
        StatusChangedEventArgs^ eventArgs = ref new StatusChangedEventArgs(newStatus);
        StatusChanged(this, eventArgs);
    }
}

void ACRCloudSession::ProcessAudioSamples(unsigned long audioQueueTargetSize)
{
    // Exit if a task is in progress.
    if (!m_recognitionTask.is_done())
    {
        return;
    }

    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    m_recognitionTask = create_task([this, audioQueueTargetSize]
        {
            // Only allow 3 attempts.
            if (m_recognitionAttempts > 2)
            {
                UpdateStatus(IdentifyStatus::Error);
                cancel_current_task();
            }
        }, task_continuation_context::use_arbitrary())
    .then([this, audioQueueTargetSize](void)
        {
            while (m_audioData.size() < audioQueueTargetSize)
            {
                if (m_audioQueue.empty())
                {
                    break;
                }

                std::vector<byte> audioVector = m_audioQueue.front();
                    if (audioVector.empty())
                {
                    break;
                }

                for (std::vector<byte>::iterator it = audioVector.begin(); it != audioVector.end(); ++it)
                {
                    m_audioData.push_back(*it);
                }

                m_audioQueue.pop_front();
            }

            return task_from_result(m_audioData);
        }, task_continuation_context::use_arbitrary())
    .then([this](std::vector<byte> audioData)
        {
            size_t audioSecondsAvailable = audioData.size() / m_bytesPerSecond;
            IBuffer^ fingerprintBuffer = GetFingerprint(audioData, audioSecondsAvailable);
            return task_from_result(fingerprintBuffer);
        }, task_continuation_context::use_arbitrary())
    .then([this](IBuffer^ fingerprintBuffer)
        {
            if (fingerprintBuffer == nullptr)
            {
                cancel_current_task();
            }

            return m_client->QueryTrackInfoAsync(fingerprintBuffer);
    }, task_continuation_context::use_arbitrary())
    .then([this](HttpRequestResult^ result)
        {
            if (!result->Succeeded)
            {
                cancel_current_task();
            }

            HttpResponseMessage^ response = result->ResponseMessage;
            return response->Content->ReadAsStringAsync();
        }, task_continuation_context::use_arbitrary())
    .then([this](String^ responseBody)
        {
            if (responseBody->IsEmpty())
            {
                cancel_current_task();
            }

            return m_client->ParseTrackResponseAync(responseBody);
        }, task_continuation_context::use_arbitrary())
    .then([this](task<ACRCloudTrackResponse^> previousTask)
        {
            try
            {
                ACRCloudTrackResponse^ trackRepsonse = previousTask.get();
                m_recognitionAttempts++;

                if (trackRepsonse->Code == 0)
                {
                    m_tracks = trackRepsonse->Tracks;
                    UpdateStatus(IdentifyStatus::Complete);
                }
            }
            catch (const task_canceled&)
            {
            }
            catch (Exception^ ex)
            {
            }
        }, task_continuation_context::use_arbitrary());
}

IBuffer^ ACRCloudSession::GetFingerprint(std::vector<byte>& audioContent, size_t audioContentSize)
{
    IBuffer^ buffer = nullptr;
    int start_time_seconds = 0;
    char* fingerprint = NULL;
    char is_db_fingerprint = 0;
    int audio_len_seconds = 0;
    Array<byte>^ fingerprintBytes;
    int rc = 0;

    // Create the fingerprint. create_fingerprint expects a 8000 hz stream but we don't have that, Instead,
    // wrap our stream in a wav header and let create_fingerprint_by_filebuffer handle the conversion.
    std::vector<byte> fileContent;
    rc = PrependFileHeader(audioContent, audioContentSize, fileContent);
    ACR_CHECK(rc);

    audio_len_seconds = static_cast<int>(audioContentSize / m_bytesPerSecond);
    rc = create_fingerprint_by_filebuffer(
        reinterpret_cast<char*>(fileContent.data()),
        static_cast<int>(fileContent.size()),
        start_time_seconds,
        audio_len_seconds,
        is_db_fingerprint,
        &fingerprint);
    ACR_CHECK(rc);

    // If the fingerprint is valid, copy it to a buffer.
    fingerprintBytes = ref new Array<byte>(rc);
    for (int i = 0; i < rc; i++)
    {
        fingerprintBytes[i] = reinterpret_cast<byte&>(fingerprint[i]);
    }

    buffer = CryptographicBuffer::CreateFromByteArray(fingerprintBytes);
    rc = buffer != nullptr ? 0 : -1;

error:
    ACR_CLEANUP_STRING(fingerprint);
    return buffer;
}

int ACRCloudSession::PrependFileHeader(const std::vector<byte>& audioContent, size_t audioContentSize, std::vector<byte>& fileContent)
{
    union byte_converter
    {
        int intValue;
        short shortValue;
        unsigned char bytes[sizeof(float)];
    } byte_converter;

    // Get the size of the content.
    int subChunk2Size = static_cast<int>(audioContent.size());

    //
    // From: http://soundfile.sapp.org/doc/WaveFormat/
    //
    // Offset  Size  Name             Description
    // 0       4     ChunkID          Contains the letters "RIFF" in ASCII form (0x52494646 big - endian form).
    fileContent.push_back(0x52);
    fileContent.push_back(0x49);
    fileContent.push_back(0x46);
    fileContent.push_back(0x46);

    // 4       4     ChunkSize        36 + SubChunk2Size, or more precisely : 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
    byte_converter.intValue = 36 + subChunk2Size;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);
    fileContent.push_back(byte_converter.bytes[2]);
    fileContent.push_back(byte_converter.bytes[3]);

    // 8       4     Format           Contains the letters "WAVE" (0x57415645 big - endian form).
    fileContent.push_back(0x57);
    fileContent.push_back(0x41);
    fileContent.push_back(0x56);
    fileContent.push_back(0x45);

    // 12      4     Subchunk1ID      Contains the letters "fmt " (0x666d7420 big - endian form).
    fileContent.push_back(0x66);
    fileContent.push_back(0x6d);
    fileContent.push_back(0x74);
    fileContent.push_back(0x20);

    // 16      4     Subchunk1Size    16 for PCM. This is the size of the rest of the Subchunk which follows this number.
    byte_converter.intValue = 16;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);
    fileContent.push_back(byte_converter.bytes[2]);
    fileContent.push_back(byte_converter.bytes[3]);

    // 20      2     AudioFormat      PCM = 1 (i.e.Linear quantization) Values other than 1 indicate some form of compression.
    byte_converter.shortValue = 1;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);

    // 22      2     NumChannels      Mono = 1, Stereo = 2, etc.
    byte_converter.shortValue = m_options->ChannelCount;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);

    // 24      4     SampleRate       8000, 44100, etc.
    byte_converter.intValue = m_options->SampleRate;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);
    fileContent.push_back(byte_converter.bytes[2]);
    fileContent.push_back(byte_converter.bytes[3]);

    // 28      4     ByteRate         SampleRate * NumChannels * BitsPerSample / 8
    byte_converter.intValue = m_options->SampleRate * m_options->ChannelCount * m_options->SampleSize / 8;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);
    fileContent.push_back(byte_converter.bytes[2]);
    fileContent.push_back(byte_converter.bytes[3]);

    // 32      2     BlockAlign       NumChannels * BitsPerSample / 8. The number of bytes for one sample including all channels.
    byte_converter.shortValue = m_options->ChannelCount * m_options->SampleSize / 8;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);

    // 34      2     BitsPerSample    8 bits = 8, 16 bits = 16, etc.
    byte_converter.shortValue = m_options->SampleSize;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);

    // 36      4     Subchunk2ID      Contains the letters "data" (0x64617461 big - endian form).
    fileContent.push_back(0x64);
    fileContent.push_back(0x61);
    fileContent.push_back(0x74);
    fileContent.push_back(0x61);

    // 40      4     Subchunk2Size    NumSamples * NumChannels * BitsPerSample / 8. This is the number of bytes in the data.
    byte_converter.intValue = subChunk2Size;
    fileContent.push_back(byte_converter.bytes[0]);
    fileContent.push_back(byte_converter.bytes[1]);
    fileContent.push_back(byte_converter.bytes[2]);
    fileContent.push_back(byte_converter.bytes[3]);

    // 44      *     Data             The actual sound data.
    // Iterate the audioContent vector and deep-copy to the member vector.
    for (std::vector<byte>::const_iterator it = audioContent.cbegin(); it != audioContent.cend(); it++)
    {
        fileContent.push_back(*it);
    }

    return 0;
}
