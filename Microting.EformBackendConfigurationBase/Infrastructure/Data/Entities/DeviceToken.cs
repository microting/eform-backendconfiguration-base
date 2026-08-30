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

using System.ComponentModel.DataAnnotations;

// Identity is the app install, not the token: (AppId, InstallationId) is
// unique. FcmToken is mutable (it rotates); SdkSiteId is a mutable owner
// reassigned when a different user logs in on the same device.
//
// The unique index has no WorkflowState filter and PnBase.Delete() only
// soft-deletes, so consumers MUST upsert on (AppId, InstallationId)
// including soft-deleted rows and flip WorkflowState back to Created.
// Create()ing over a soft-deleted install throws DbUpdateException instead -
// which is exactly the re-register-after-logout path.
public class DeviceToken : PnBase
{
    // [Required] is load-bearing: this project does not enable nullable
    // reference types, so EF would otherwise map these as NULL-able - and a
    // MariaDB unique index permits unlimited rows whose indexed columns are
    // NULL, silently defeating the (AppId, InstallationId) identity.
    [Required]
    [StringLength(32)]
    public string AppId { get; set; }

    [Required]
    [StringLength(128)]
    public string InstallationId { get; set; }

    [StringLength(512)]
    public string FcmToken { get; set; }

    // SDK Site.Id of the worker owning the device.
    public int SdkSiteId { get; set; }

    // e.g. "android" or "ios"
    [StringLength(50)]
    public string Platform { get; set; }
}
