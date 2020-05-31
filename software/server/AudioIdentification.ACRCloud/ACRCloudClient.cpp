//-----------------------------------------------------------------------
// <copyright file="ACRCloudClient.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "ACRCloudClient.h"
#include "ACRCloudHelpers.h"
#include <sstream>
#include <chrono>

using namespace std;
using namespace Concurrency;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Data::Json;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::Security::Cryptography;
using namespace Windows::Security::Cryptography::Core;
using namespace Windows::Storage::Streams;
using namespace Windows::Web;
using namespace Windows::Web::Http;
using namespace Windows::Web::Http::Filters;
using namespace Windows::Web::Http::Headers;
using namespace CrazyGiraffe::AudioIdentification;
using namespace CrazyGiraffe::AudioIdentification::ACRCloud;

ACRCloudClient::ACRCloudClient(ACRCloudClientIdData^ clientdata)
    : m_clientdata(clientdata)
    , m_httpFilter(nullptr)
{
}

ACRCloudClient::ACRCloudClient(ACRCloudClientIdData^ clientdata, IHttpFilter^ httpFilter)
    : m_clientdata(clientdata)
    , m_httpFilter(httpFilter)
{
}

String^ ACRCloudClient::CreateSignature(String^ input, String^ key)
{
    String^ hashValue = nullptr;
    try
    {
        MacAlgorithmProvider^ provider = MacAlgorithmProvider::OpenAlgorithm(MacAlgorithmNames::HmacSha1);

        IBuffer^ keyBuffer = CryptographicBuffer::ConvertStringToBinary(key, BinaryStringEncoding::Utf8);
        CryptographicHash^ hash = provider->CreateHash(keyBuffer);

        IBuffer^ inputBuffer = CryptographicBuffer::ConvertStringToBinary(input, BinaryStringEncoding::Utf8);
        hash->Append(inputBuffer);
        IBuffer^ hashBuffer = hash->GetValueAndReset();

        hashValue = CryptographicBuffer::EncodeToBase64String(hashBuffer);
    }
    catch (Exception ^ ex)
    {
        LogMessage("CreateSignature exception: %s", ex->Message);
    }

    return hashValue;
}

IAsyncOperation<HttpRequestResult^>^ ACRCloudClient::QueryTrackInfoAsync(IBuffer^ fingerprintBuffer)
{
    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    return create_async([this, &fingerprintBuffer]() -> task<HttpRequestResult^>
        {
            int rc = 0;
            try
            {
                if (fingerprintBuffer == nullptr)
                {
                    return task_from_result(static_cast<HttpRequestResult^>(nullptr));
                };

                wstring method = L"POST";
                wstring path = L"/v1/identify";
                wstring dataType = L"fingerprint";
                wstring signatureVersion = L"1";
                wstring boundryHeader = L"acrcloud___copyright___2015___";

                // Create request URL.
                wstringstream requestUrlStream;
                requestUrlStream << L"http://" << this->m_clientdata->Host->Data() << path;
                const wstring requestUrlWstr = requestUrlStream.str();
                Uri^ resourceUrl = ref new Uri(ref new String(requestUrlWstr.c_str()));

                // Create timestamp.
                time_t ltime;
                wstringstream timestamp;

                time(&ltime);
                timestamp << ltime;

                // Create signature.
                wstringstream signatureInput;
                signatureInput << method << L"\n" << path << L"\n" << this->m_clientdata->AccessKey->Data() << L"\n";
                signatureInput << dataType << L"\n" << signatureVersion << L"\n" << ltime;

                String^ signatureInputString = ref new String(signatureInput.str().c_str());
                String^ signature = CreateSignature(signatureInputString, this->m_clientdata->AccessSecret);

                // Create mime boundry.
                FILETIME filetime;
                ::GetSystemTimeAsFileTime(&filetime);
                unsigned __int64 ticks = (__int64(filetime.dwHighDateTime) << 32LL) + __int64(filetime.dwLowDateTime);

                const unsigned __int64 TicksPerDay = 864000000000;
                unsigned __int64 ticksPerYear = TicksPerDay * 365;
                unsigned __int64 ticksSince1601 = (ticksPerYear * 1601) + (TicksPerDay * 23);
                ticksSince1601 += (TicksPerDay * 23); // the is calculated emperically.

                wstringstream boundry;
                boundry << boundryHeader << hex << ticks + ticksSince1601;

                // Setup multi-part form data.
                const wstring boundryWstr = boundry.str();
                String^ boundryStr = ref new String(boundryWstr.c_str());
                HttpMultipartFormDataContent^ formContent = ref new HttpMultipartFormDataContent(boundryStr);

                HttpStringContent^ accessKeyContent = ref new HttpStringContent(this->m_clientdata->AccessKey);
                formContent->Add(accessKeyContent, L"access_key");

                HttpStringContent^ timestampContent = ref new HttpStringContent(ref new String(timestamp.str().c_str()));
                formContent->Add(timestampContent, L"timestamp");

                HttpStringContent^ signatureContent = ref new HttpStringContent(signature);
                formContent->Add(signatureContent, L"signature");

                HttpStringContent^ dataTypeContent = ref new HttpStringContent(ref new String(dataType.c_str()));
                formContent->Add(dataTypeContent, L"data_type");

                HttpStringContent^ signatureVersionContent = ref new HttpStringContent(ref new String(signatureVersion.c_str()));
                formContent->Add(signatureVersionContent, L"signature_version");

                wstringstream sampleLength;
                sampleLength << fingerprintBuffer->Length;
                HttpStringContent^ sampleLengthContent = ref new HttpStringContent(ref new String(sampleLength.str().c_str()));
                formContent->Add(sampleLengthContent, L"sample_bytes");

                HttpBufferContent^ sampleContent = ref new HttpBufferContent(fingerprintBuffer);
                sampleContent->Headers->ContentDisposition = ref new HttpContentDispositionHeaderValue(L"form-data");
                sampleContent->Headers->ContentDisposition->Name = ref new String(L"sample");
                sampleContent->Headers->ContentDisposition->FileName = ref new String(L"sample");
                sampleContent->Headers->Append(L"Content-Type", L"application/octet-stream");
                formContent->Add(sampleContent);

                // Send request.
                ACRCloudClient^ _this = this;
                HttpClient^ httpClient = GetHttpClient();
                cancellation_token_source cancellationTokenSource = cancellation_token_source();

                return create_task(httpClient->TryPostAsync(resourceUrl, formContent), cancellationTokenSource.get_token());
            }
            catch (Exception ^ ex)
            {
                LogMessage("QueryTrackInfoAsync exception: %s", ex->Message);
                throw;
            }
        });
}

IAsyncOperation<ACRCloudTrackResponse^>^ ACRCloudClient::ParseTrackResponseAync(Platform::String^ responseBody)
{
    return create_async([&responseBody]() -> task<ACRCloudTrackResponse^>
        {
            String^ message = L"";
            String^ version = L"";
            double code = -1;
            Vector<IReadOnlyTrack^>^ tracks = ref new Vector<IReadOnlyTrack^>();

            do
            {
                JsonObject^ root = JsonObject::Parse(responseBody);

                //"status":{
                //    "msg":"Success",
                //        "version" : "1.0",
                //        "code" : 0
                //},
                if (!root->HasKey("status"))
                {
                    LogMessage("ParseTrackResponseAync: status missing");
                    continue;
                }

                JsonObject^ status = root->GetNamedObject(L"status");
                message = status->GetNamedString(L"msg");
                version = status->GetNamedString(L"version");
                code = status->GetNamedNumber(L"code");
                if (code != 0)
                {
                    LogMessage("ParseTrackResponseAync: code = %d", (int)code);
                    continue;
                }

                //"metadata":{
                //    "timestamp_utc":"2020-01-19 02:58:28",
                //     "music" : [
                //          ...
                //     ]
                if (!root->HasKey("metadata"))
                {
                    LogMessage("ParseTrackResponseAync: metadata missing");
                    continue;
                }

                JsonObject^ metadata = root->GetNamedObject(L"metadata");
                if (!metadata->HasKey("music"))
                {
                    LogMessage("ParseTrackResponseAync: music missing");
                    continue;
                }

                JsonArray^ musicArray = metadata->GetNamedArray(L"music");
                for (unsigned int i = 0; i < musicArray->Size; i++)
                {
                    try
                    {
                        Track^ track = ref new Track();
                        JsonObject^ music = musicArray->GetObjectAt(i);

                        // Get basic metadata. Per the docs, this stuff is required.
                        // Please note: Only ACRID, Track Title, Artists Name and Album Name fields are required, other fields are optional.

                        // "acrid":"6049f11da7095e8bb8266871d4a70873",
                        track->Identifier = music->GetNamedString(L"acrid");

                        // "title":"Hello",
                        track->Title = music->GetNamedString(L"title");

                        // Get album.
                        //"album":{
                        //    "name":"Hello"
                        //},
                        JsonObject^ album = music->GetNamedObject(L"album");
                        track->Album = album->GetNamedString(L"name");

                        // Get artist (first one)
                        //"artists":[
                        //    {
                        //        "name":"Adele"
                        //    },
                        //    ...
                        // ],
                        JsonArray^ artists = music->GetNamedArray(L"artists");
                        if (artists->Size > 0)
                        {
                            JsonObject^ artist = artists->GetObjectAt(0);
                            track->Artist = artist->GetNamedString(L"name");
                        }

                        // Get extended metadata. Per the docs, this stuff is optional.
                        // "genres": [
                        //    {
                        //        "name": "Alternative"
                        //    },
                        //    ...
                        // ],
                        // "duration_ms":295000,
                        if (music->HasKey("genres"))
                        {
                            JsonArray^ genres = music->GetNamedArray(L"genres");
                            if (genres->Size > 0)
                            {
                                JsonObject^ genre = genres->GetObjectAt(0);
                                track->Genre = genre->GetNamedString(L"name");
                            }
                        }

                        // "duration_ms":295000,
                        if (music->HasKey("duration_ms"))
                        {
                            track->Duration = static_cast<int>(music->GetNamedNumber(L"duration_ms"));
                        }

                        // "play_offset_ms":9040,
                        if (music->HasKey("play_offset_ms"))
                        {
                            track->CurrentPosition = static_cast<int>(music->GetNamedNumber(L"play_offset_ms"));
                            track->MatchPosition = track->CurrentPosition;
                        }

                        // Get score.
                        // "score":100,
                        if (music->HasKey("score"))
                        {
                            double score = music->GetNamedNumber(L"score");
                            wstringstream scoreStream;
                            scoreStream << score;
                            track->MatchConfidence = ref new String(scoreStream.str().c_str());
                        }

                        // Get external info to retrieve cover art.
                        //"external_metadata":{
                        //    "musicbrainz": [
                        //    {
                        //        "track":{
                        //            "id":"0a8e8d55-4b83-4f8a-9732-fbb5ded9f344"
                        //        }
                        //    }
                        //] ,
                        if (music->HasKey("external_metadata"))
                        {
                            JsonObject^ externalLinks = music->GetNamedObject(L"external_metadata");
                            if (externalLinks->HasKey("musicbrainz"))
                            {
                                JsonArray^ musicbrainz = externalLinks->GetNamedArray(L"musicbrainz");
                                if (musicbrainz->Size > 0)
                                {
                                    // Just grab the first one.
                                    JsonObject^ musicbrainzTrack = musicbrainz->GetObjectAt(0);
                                    if (musicbrainzTrack != nullptr)
                                    {
                                        if (musicbrainzTrack->HasKey("track"))
                                        {
                                            JsonObject^ musicbrainzTrack0 = musicbrainzTrack->GetNamedObject(L"track");
                                            String^ musicbrainzTrackId = musicbrainzTrack0->GetNamedString(L"id");

                                            wstringstream imageUrl;
                                            imageUrl << L"http://coverartarchive.org/release/" << musicbrainzTrackId->Data() << L"/front";
                                            const wstring wstr = imageUrl.str();
                                            track->CovertArtImage = ref new Uri(ref new String(wstr.c_str()));
                                        }
                                    }
                                }
                            }
                        }

                        // Save track to list.
                        tracks->Append(track);
                    }
                    catch (Exception ^ ex)
                    {
                        LogMessage("ParseTrackResponseAync track exception: %s", ex->Message);
                    }
                }
            } while (false);

            return task_from_result(ref new ACRCloudTrackResponse(
                message,
                version,
                static_cast<int16>(code),
                tracks->GetView()));
        });
}

HttpClient^ ACRCloudClient::GetHttpClient()
{
    if (m_httpFilter != nullptr)
    {
        return ref new HttpClient(m_httpFilter);
    }

    return ref new HttpClient();
}
