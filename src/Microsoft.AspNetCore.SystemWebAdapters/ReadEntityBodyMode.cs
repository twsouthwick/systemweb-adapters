﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web
{
    public enum ReadEntityBodyMode
    {
        None,
        Classic, // BinaryRead, Form, Files, InputStream
        Bufferless, // GetBufferlessInputStream
        Buffered // GetBufferedInputStream
    }
}
