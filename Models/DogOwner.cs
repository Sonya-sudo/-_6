using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class DogOwner
{
    public int DogOwnerId { get; set; }

    public int OwnerId { get; set; }

    public int ChipNumber { get; set; }

    public virtual Dog ChipNumberNavigation { get; set; } = null!;

    public virtual Owner Owner { get; set; } = null!;
}
