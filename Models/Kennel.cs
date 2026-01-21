using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class Kennel
{
    public int KennelId { get; set; }

    public string KennelName { get; set; } = null!;

    public DateOnly? FoundationDate { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Dog> Dogs { get; set; } = new List<Dog>();
}
