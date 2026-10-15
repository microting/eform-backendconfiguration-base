/*
The MIT License (MIT)

Copyright (c) 2007 - 2023 Microting A/S

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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Enum;

public class File : PnBase
{
	public string FileName { get; set; }

	public virtual List<PropertyFile> PropertyFiles { get; set; }
		= new ();

	public virtual UploadedData UploadedData { get; set; }

	public virtual List<FileTags> FileTags { get; set; }
		= new List<FileTags>();

	/// <summary>Required: every archive document lies in exactly one ArchiveFolder.</summary>
	public int FolderId { get; set; }

	public virtual ArchiveFolder Folder { get; set; }

	[Column(TypeName = "date")]
	public DateTime? DocumentDate { get; set; }

	public DocumentDateState DocumentDateState { get; set; }

	/// <summary>True: ReadTags decide who reads the file, not the folder's.</summary>
	public bool OwnAccess { get; set; }

	public virtual List<FileReadTag> ReadTags { get; set; } = new();
}