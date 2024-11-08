﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StorybookCabinPOCBlazor.Models;

public partial class StorybookCabinPOCBlazorContext : DbContext
{
    public StorybookCabinPOCBlazorContext(DbContextOptions<StorybookCabinPOCBlazorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Credit> Credits { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<ServiceSetting> ServiceSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credit>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Credits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Credits_Users");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasIndex(e => e.LogType, "IX_Logs");

            entity.Property(e => e.LogDate).HasColumnType("datetime");
            entity.Property(e => e.LogDetail)
                .IsRequired()
                .HasMaxLength(4000);
            entity.Property(e => e.LogIpaddress)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("LogIPAddress");
            entity.Property(e => e.LogType)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ServiceInstanceName).HasMaxLength(50);

            entity.HasOne(d => d.Credit).WithMany(p => p.Logs)
                .HasForeignKey(d => d.CreditId)
                .HasConstraintName("FK_Logs_Credits");

            entity.HasOne(d => d.User).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Logs_Users");
        });

        modelBuilder.Entity<ServiceSetting>(entity =>
        {
            entity.HasIndex(e => e.ServiceName, "IX_ServiceSettings").IsUnique();

            entity.Property(e => e.LastCompleted).HasColumnType("datetime");
            entity.Property(e => e.LastStarted).HasColumnType("datetime");
            entity.Property(e => e.ServiceName)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.AuthenticationType)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(1000);
            entity.Property(e => e.Email).HasMaxLength(1000);
            entity.Property(e => e.FirstName).HasMaxLength(500);
            entity.Property(e => e.IdentityProvider)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.LastAuthTime).HasColumnName("LastAuth_time");
            entity.Property(e => e.LastIpaddress)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("LastIPAddress");
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.LastidpAccessToken)
                .HasMaxLength(4000)
                .HasColumnName("Lastidp_access_token");
            entity.Property(e => e.Objectidentifier)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.SigninMethod).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}