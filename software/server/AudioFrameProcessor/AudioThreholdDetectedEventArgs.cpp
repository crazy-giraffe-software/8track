//-----------------------------------------------------------------------
// <copyright file="AudioThreholdDetectedEventArgs.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "AudioThreholdDetectedEventArgs.h"

using namespace CrazyGiraffe::AudioFrameProcessor;

AudioThreholdDetectedEventArgs::AudioThreholdDetectedEventArgs(ThresholdStatus status)
    : m_status(status)
{
}

ThresholdStatus AudioThreholdDetectedEventArgs::Status::get()
{
    return m_status;
}
