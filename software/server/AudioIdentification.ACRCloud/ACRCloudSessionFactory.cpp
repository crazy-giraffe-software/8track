//-----------------------------------------------------------------------
// <copyright file="ACRCloudSessionFactory.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "ACRCloudSessionFactory.h"
#include "ACRCloudSession.h"
#include "ACRCloudHelpers.h"
#include <stdlib.h>

using namespace concurrency;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::Web::Http::Filters;
using namespace CrazyGiraffe::AudioIdentification;
using namespace CrazyGiraffe::AudioIdentification::ACRCloud;

ACRCloudSessionFactory::ACRCloudSessionFactory(ACRCloudClientIdData^ clientdata)
    : m_clientdata(clientdata)
    , m_initialized(false)
    , m_httpFilter(nullptr)
{
}

ACRCloudSessionFactory::ACRCloudSessionFactory(ACRCloudClientIdData^ clientdata, IHttpFilter^ httpFilter)
    : m_clientdata(clientdata)
    , m_initialized(false)
    , m_httpFilter(httpFilter)
{
}

IAsyncOperation<ISession^>^ ACRCloudSessionFactory::CreateSessionAsync(SessionOptions^ options)
{
    // E1740 error - [this] seems to be an error but it's a bug in VS2019.
    // It will show as an error in the editor and during a failed compilation
    // but will compile cleanly. Move along, nothing to see here.
    return create_async([this, &options]() -> task<ISession^>
        {
            // Initialize plugin.
            if (!m_initialized)
            {
                acr_init();
            }

            // Create an initialize a new server.
            ACRCloudSession^ session = ref new ACRCloudSession();
            session->Initialize(m_clientdata, m_httpFilter, options);

            return task_from_result<ISession^>(session);
        });
}
