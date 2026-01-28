using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class ComplaintCounter
{
    public ComplaintTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    public int CountOpen { get; set; }
    public int CountTotal { get; set; }
    public DateTime UpdatedAt { get; set; }
}