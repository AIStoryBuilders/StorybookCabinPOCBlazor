﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StorybookCabinPOCBlazor.Models;

public partial class ServiceSetting
{
    public int Id { get; set; }

    public string ServiceName { get; set; }

    public bool Active { get; set; }

    public DateTime? LastStarted { get; set; }

    public DateTime? LastCompleted { get; set; }
}