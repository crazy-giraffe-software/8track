//-----------------------------------------------------------------------
// <copyright file="GracenoteClientIdData.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "GracenoteClientIdData.h"

using namespace Platform;
using namespace CrazyGiraffe::AudioIdentification::Gracenote;

GracenoteClientIdData::GracenoteClientIdData()
    : m_clientId(L"")
    , m_clientTag(L"")
    , m_appVersion(L"")
    , m_license(L"")
{
}

String^ GracenoteClientIdData::ClientId::get()
{
    return m_clientId;
}

void GracenoteClientIdData::ClientId::set(String^ value)
{
    m_clientId = value;
}

String^ GracenoteClientIdData::ClientTag::get()
{
    return m_clientTag;
}

void GracenoteClientIdData::ClientTag::set(String^ value)
{
    m_clientTag = value;
}

String^ GracenoteClientIdData::AppVersion::get()
{
    return m_appVersion;
}

void GracenoteClientIdData::AppVersion::set(String^ value)
{
    m_appVersion = value;
}

String^ GracenoteClientIdData::License::get()
{
    return m_license;
}

void GracenoteClientIdData::License::set(String^ value)
{
    m_license = value;
}
