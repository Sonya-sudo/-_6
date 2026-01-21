using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class Discipline
{
    public int DisciplineId { get; set; }

    public string DisciplineName { get; set; } = null!;

    public int? WorkingScore { get; set; }
    public int? CompositionID { get; set; }
    public int? Coefficient { get; set; }

    public virtual ICollection<DogDiscipline> DogDisciplines { get; set; } = new List<DogDiscipline>();
}
