using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelLikeDataGrid.Utilities
{
    public interface ICloseable
    {
        void Close();
        void Hide();
    }
}
