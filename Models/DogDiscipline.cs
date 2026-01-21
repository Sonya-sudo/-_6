using System;
using System.Collections.Generic;

namespace Клуб_6.Models;

public partial class DogDiscipline
{
    public int Id { get; set; }

    public int RecordId { get; set; }

    public int DisciplineId { get; set; }

    public int? Score { get; set; }

    public virtual Discipline Discipline { get; set; } = null!;

    public virtual DogList Record { get; set; } = null!;
}
