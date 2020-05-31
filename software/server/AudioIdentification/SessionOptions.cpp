//-----------------------------------------------------------------------
// <copyright file="SessionOptions.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "SessionOptions.h"

using namespace CrazyGiraffe::AudioIdentification;

SessionOptions::SessionOptions()
    : m_sampleRate(44100)
    , m_sampleSize(16)
    , m_channelCount(2)
{
}

SessionOptions::SessionOptions(SessionOptions^ options)
{
    this->SampleRate = options->SampleRate;
    this->SampleSize = options->SampleSize;
    this->ChannelCount = options->ChannelCount;
}

SessionOptions::SessionOptions(
    uint16 SampleRate,
    uint16 SampleSize,
    uint16 ChannelCount)
{
    this->SampleRate = SampleRate;
    this->SampleSize = SampleSize;
    this->ChannelCount = ChannelCount;
}

uint16 SessionOptions::SampleRate::get()
{
    return m_sampleRate;
}

void SessionOptions::SampleRate::set(uint16 value)
{
    m_sampleRate = value;
}

uint16 SessionOptions::SampleSize::get()
{
    return m_sampleSize;
}

void SessionOptions::SampleSize::set(uint16 value)
{
    m_sampleSize = value;
}

uint16 SessionOptions::ChannelCount::get()
{
    return m_channelCount;
}

void SessionOptions::ChannelCount::set(uint16 value)
{
    m_channelCount = value;
}
