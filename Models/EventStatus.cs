using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class EventStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
