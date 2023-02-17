using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelLikeDataGrid.Model
{
    public class FilterCondition
    {
        public bool HasConjunction { get; set; } = true;
        public string Conjunction { get; set; }
        public string FilterMode { get; set; }
        public string FilterValue { get; set; }
    }
}