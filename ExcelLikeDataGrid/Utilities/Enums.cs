using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelLikeDataGrid.Utilities
{
    internal class Enums
    {
        internal enum FilterModesEnum
        {
            Equals, NotEquals, BeginsWith, EndsWith, Contains, NotContains, GreaterThan,
            LessThan, GreaterThanOrEqual, LessThanOrEqual, IsNull, NotIsNull
        }

        internal enum ConjunctionsEnum
        {
            And, Or
        }
    }
}
