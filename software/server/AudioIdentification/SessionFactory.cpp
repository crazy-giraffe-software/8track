//-----------------------------------------------------------------------
// <copyright file="SessionFactory.cpp" company="CrazyGiraffeSoftware.net">
// Copyright (c) CrazyGiraffeSoftware.net. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------
#include "pch.h"
#include "SessionFactory.h"

using namespace concurrency;
using namespace Windows::Foundation;
using namespace CrazyGiraffe::AudioIdentification;

SessionFactory::SessionFactory()
{
}

IAsyncOperation<ISession^>^ SessionFactory::CreateSessionAsync(SessionOptions^ options)
{
    return create_async([&options]() -> task<ISession^>
        {
            // Create an initialize a new session.
            ISession^ session = ref new Session(options);
            return task_from_result(session);
        });
}
