﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StorybookCabinPOCBlazor.Models;

public partial class Log
{
    public int LogId { get; set; }

    public bool IsError { get; set; }

    public DateTime LogDate { get; set; }

    public string LogType { get; set; }

    public string LogDetail { get; set; }

    public string LogIpaddress { get; set; }

    public string ServiceInstanceName { get; set; }

    public int? UserId { get; set; }

    public int? ArticleId { get; set; }

    public int? VideoRequestId { get; set; }

    public int? CreditId { get; set; }

    public virtual Credit Credit { get; set; }

    public virtual User User { get; set; }
}