// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpApplicationOptions
{
    public bool SetCurrentNotification { get; set; }

    public bool UseAuthentication { get; set; }

    public bool UseAuthorization { get; set; }

    public bool EnableHttpApplication { get; set; }

    public bool EnableHandlers { get; set; }
}
