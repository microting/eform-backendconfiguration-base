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

namespace Microting.EformBackendConfigurationBase.Infrastructure.Const;

/// <summary>
/// Values of TaskChangeFeedEntry.Kind. Changed, Removed and Property are TaskChanged wire names;
/// Assignments maps to AssignmentsChanged. Add values, never change them; readers treat unknown
/// values as Changed.
/// </summary>
public static class TaskChangeKinds
{
    /// <summary>An occurrence was created or changed.</summary>
    public const string Changed = "changed";

    /// <summary>An occurrence, or the whole task when only the event id is set, is gone.</summary>
    public const string Removed = "removed";

    /// <summary>Anything in the property may have changed; no key.</summary>
    public const string Property = "property";

    /// <summary>The worker's property assignments or worker tags changed.</summary>
    public const string Assignments = "assignments";
}
