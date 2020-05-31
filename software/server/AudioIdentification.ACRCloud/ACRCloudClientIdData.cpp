//-----------------------------------------------------------------------
// <copyright file="ACRCloudClientIdData.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "ACRCloudClientIdData.h"

using namespace Platform;
using namespace CrazyGiraffe::AudioIdentification::ACRCloud;

ACRCloudClientIdData::ACRCloudClientIdData()
    : m_host(L"")
    , m_accessKey(L"")
    , m_accessSecret(L"")
{
}

String^ ACRCloudClientIdData::Host::get()
{
    return m_host;
}

void ACRCloudClientIdData::Host::set(String^ value)
{
    m_host = value;
}

String^ ACRCloudClientIdData::AccessKey::get()
{
    return m_accessKey;
}

void ACRCloudClientIdData::AccessKey::set(String^ value)
{
    m_accessKey = value;
}

String^ ACRCloudClientIdData::AccessSecret::get()
{
    return m_accessSecret;
}

void ACRCloudClientIdData::AccessSecret::set(String^ value)
{
    m_accessSecret = value;
}
