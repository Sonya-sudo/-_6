using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class Option
{
    public int OptionId { get; set; }

    public string OptionType { get; set; } = null!;

    public string? OptionValue { get; set; }

    public string? TextInfo { get; set; }

    public int? CompositionID { get; set; }

    public int? CriterionID { get; set; }

    public virtual Criterion? Criterion { get; set; }
}
