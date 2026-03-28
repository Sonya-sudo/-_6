using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class Dog
{
    public int ChipNumber { get; set; }

    public string DogName { get; set; } = null!;

    public string Breed { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public string Gender { get; set; } = null!;

    public string? MotherName { get; set; }

    public string? FatherName { get; set; }

    public string? Color { get; set; }

    public int? HeightCm { get; set; }

    public int? WeightKg { get; set; }

    public bool? IsAlive { get; set; }

    public int? KennelId { get; set; }

    public string? Pedigree { get; set; }

    public virtual ICollection<DogList> DogLists { get; set; } = new List<DogList>();

    public virtual ICollection<DogOwner> DogOwners { get; set; } = new List<DogOwner>();

    public virtual Kennel? Kennel { get; set; }
}
