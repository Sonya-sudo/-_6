using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Клуб_6.Models;

public partial class Criterion
{
    public int CriterionID { get; set; }

    public string CriterionName { get; set; } = null!;

    public int? CompositionID { get; set; }

    public bool? IsImportant { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    [NotMapped] 
    public IEnumerable<Option> RadioOptions
            => Options?.Where(o => o.OptionType == "radio") ?? Enumerable.Empty<Option>();

    [NotMapped]
    public IEnumerable<Option> TextOptions
        => Options?.Where(o => o.OptionType == "text") ?? Enumerable.Empty<Option>();
}
