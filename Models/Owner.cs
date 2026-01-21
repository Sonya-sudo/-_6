using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class Owner
{
    public int OwnerId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public DateOnly? BirthDate { get; set; }

    public virtual ICollection<DogOwner> DogOwners { get; set; } = new List<DogOwner>();
}
