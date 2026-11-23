/*
The MIT License (MIT)

Copyright (c) 2007 - 2026 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Microting.EformBackendConfigurationBase.Infrastructure.Enum;

/// <summary>
/// State of a TaskNotification row, stored as tinyint unsigned. The numbers are persisted: never
/// renumber or reuse a value; add new states at the end. 0 is deliberately unused, so a row written
/// without a state is visible as an error. See TaskNotificationOutbox for the transitions.
/// </summary>
public enum TaskNotificationState : byte
{
    /// <summary>Planned; the deliverer sends it once NextAttemptAtUtc has passed.</summary>
    Pending = 1,

    /// <summary>Claimed by a deliverer for sending, until LeaseUntilUtc.</summary>
    Sending = 2,

    /// <summary>Sent (FCM accepted it, or the site had no token).</summary>
    Sent = 3,

    /// <summary>Sent, then no longer wanted; the deliverer sends a withdraw.</summary>
    WithdrawPending = 4,

    /// <summary>Claimed by a deliverer for withdrawing, until LeaseUntilUtc.</summary>
    Withdrawing = 5,

    /// <summary>The withdraw was sent.</summary>
    Withdrawn = 6,

    /// <summary>No longer wanted before it was sent; nothing was sent.</summary>
    Cancelled = 7,

    /// <summary>Not sent before the planned start; a reminder is never sent late.</summary>
    Expired = 8,

    /// <summary>Gave up (permanent error, or a withdraw that could not be delivered in time).</summary>
    Failed = 9
}
