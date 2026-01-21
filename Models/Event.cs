using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class Event
{
    public int EventId { get; set; }

    public DateOnly EventDate { get; set; }

    public string? EventVenue { get; set; }

    public int StatusId { get; set; }

    public int? CompositionId { get; set; }

    public string? Judge1 { get; set; }

    public string? Judge2 { get; set; }

    public string? Host { get; set; } 

    public string? CommitteeChairman { get; set; }

    public string? Organization { get; set; }

    public string? TestOrganizer { get; set; }

    public virtual EventComposition? Composition { get; set; }

    public virtual ICollection<DogList> DogLists { get; set; } = new List<DogList>();

    public virtual EventStatus Status { get; set; } = null!;
}