using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class EventComposition
{
    public int CompositionId { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
