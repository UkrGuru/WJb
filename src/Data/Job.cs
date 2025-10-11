// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace UkrGuru.WJb.Data;

public partial class JobQueue : Job { }

public partial class JobHistory : Job { }

public class Job : Rule
{
    [Display(Name = "Id")]
    public int JobId { get; set; }

    [Display(Name = "Priority")]
    public Priority JobPriority { get; set; } = Priority.Normal;

    [DisplayFormat(DataFormatString = "{0:HH:mm:ss.fff}")]
    public DateTime Created { get; set; }

    [DisplayFormat(DataFormatString = "{0:HH:mm:ss.fff}")]
    public DateTime? Started { get; set; }

    [DisplayFormat(DataFormatString = "{0:HH:mm:ss.fff}")]
    public DateTime? Finished { get; set; }

    [Display(Name = "More")]
    public string? JobMore { get; set; }

    [Display(Name = "Status")]
    public JobStatus JobStatus { get; set; }
}

public enum JobStatus
{
    Unknown = 0,
    Queued = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}