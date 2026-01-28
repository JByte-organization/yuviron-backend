using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ComplaintCounterConfiguration : IEntityTypeConfiguration<ComplaintCounter>
{
    public void Configure(EntityTypeBuilder<ComplaintCounter> builder)
    {
        builder.ToTable("complaint_counters");
        builder.HasKey(x => new { x.TargetType, x.TargetId });
    }
}
