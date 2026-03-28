using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class DogList
{
    public int RecordId { get; set; }

    public int EventId { get; set; }

    public int DogId { get; set; }

    public string? DogName { get; set; }

    public string? Owner { get; set; }

    public int? TrialPassed { get; set; }

    public string? TrialFailed { get; set; }

    public virtual Dog Dog { get; set; } = null!;

    public virtual ICollection<DogCriteriaResults_DogList> DogCriteriaResultsDogLists { get; set; } = new List<DogCriteriaResults_DogList>();

    public virtual ICollection<DogDiscipline> DogDisciplines { get; set; } = new List<DogDiscipline>();

    public virtual Event Event { get; set; } = null!;
}