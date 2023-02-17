using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelLikeDataGrid.Model
{
    /// <summary>
    /// <see cref="QuickFilterModel"/> is a class that can store simple information about each unique item in the column of the datagrid, to show it in the contextmenu quickFilter listView.
    /// </summary>
    public partial class QuickFilterModel : ObservableObject
    {
        /// <summary>
        /// The name of the quick filter item.
        /// </summary>
        [ObservableProperty]
        public string _name = string.Empty;

        /// <summary>
        /// A boolean value indicating if the quick filter item is checked or not.
        /// </summary>
        [ObservableProperty]
        public bool _isChecked = false;

        /// <summary>
        /// A boolean value indicating if the quick filter item is enabled or not.
        /// </summary>
        [ObservableProperty]
        public bool _isEnabled = true;
    }
}
