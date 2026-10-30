/*
The MIT License (MIT)

Copyright (c) 2007 - 2022 Microting A/S

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

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

// The Leder flag of one worker, keyed by the SDK Sites.Id of its Medarbejdere row (D-L15); live while
// WorkflowState != Removed. Both unique indexes (SiteId, EformUserId) also cover removed rows, so a grant must find
// the row, set WorkflowState back to Created and call Update(); Create() over a revoked site throws
// DbUpdateException. New rows carry SiteId only. EformUserId is the legacy per-login key (D-L13): rows written before
// D-L15 keep it until the plugin converts them at startup (D-L19). No foreign keys: Sites and AspNetUsers live in
// other databases.
public class LederAssignment : PnBase
{
    public int? SiteId { get; set; }

    public int? EformUserId { get; set; }
}
