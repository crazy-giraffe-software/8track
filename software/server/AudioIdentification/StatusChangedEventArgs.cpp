//-----------------------------------------------------------------------
// <copyright file="StatusChangedEventArgs.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "StatusChangedEventArgs.h"

using namespace CrazyGiraffe::AudioIdentification;

StatusChangedEventArgs::StatusChangedEventArgs()
    : m_status(IdentifyStatus::Invalid)
{
}

StatusChangedEventArgs::StatusChangedEventArgs(IdentifyStatus status)
    : m_status(status)
{
}

IdentifyStatus StatusChangedEventArgs::Status::get()
{
    return m_status;
}
