using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ExcelLikeDataGrid.Model;
using ExcelLikeDataGrid.Utilities;
using ExcelLikeDataGrid.ViewModel;

namespace ExcelLikeDataGrid.View
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window, ICloseable
    {
        public FilterWindow ()
        {

        }

        public FilterWindow(DataGrid dataGrid, int columnIndex, string startFilterMode, ref ObservableCollection<ObservableCollection<FilterCondition>> filterConditions, bool isNumericalColumn)
        {
            InitializeComponent();
            DataContext = new FilterWindowVM(dataGrid, columnIndex, startFilterMode, ref filterConditions, isNumericalColumn);
            Title = dataGrid.Columns[columnIndex].Header.ToString() + " - Column Filters";
        }

        private void FilterValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Delegate the event to the viewmodel
            (DataContext as FilterWindowVM).FilterValueTextBox_TextChanged(sender, e);
        }
    }
}