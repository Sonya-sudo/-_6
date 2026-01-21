using System;
using System.Collections.Generic;

namespace Клуб_6.Models
{
    public partial class DogCriteriaResults_DogList
    {
        public int ResultIdRecordIdId { get; set; }
        public int RecordId { get; set; }
        public int CriterionId { get; set; }
        public int OptionId { get; set; }
        public string? UserInput { get; set; }

        public virtual Criterion? Criterion { get; set; }
        public virtual DogList Record { get; set; } = null!;
    }
}