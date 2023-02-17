using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using ExcelLikeDataGrid.Utilities;
using static ExcelLikeDataGrid.Utilities.Enums;
using ExcelLikeDataGrid.Model;
using ExcelLikeDataGrid.ViewModel;


// TODO: Seperate presentation logic from business logic and bring into mvvm
namespace ExcelLikeDataGrid.View
{
    /// <summary>
    /// The CustomDataGrid class represents a custom WPF DataGrid control that provides advanced filtering and sorting functionalities.
    /// This class is implemented as a partial class and inherits from the UserControl class.
    /// </summary>
    [ObservableObject]
    public partial class CustomDataGrid : UserControl
    {
        /// <summary>
        /// The index of the column for which to perform filtering.
        /// </summary>
        private int ColumnIndex;

        /// <summary>
        /// Reference to the checked sort menuitem for sorting.
        /// </summary>
        private MenuItem CheckedSortMenuItem;

        /// <summary>
        /// The source of data items. Gets set as datagrid itemsSource in xaml.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Proxy))]
        public IEnumerable _itemsSource;

        /// <summary>
        /// The brush used to determine the background color when hovering over the item.
        /// </summary>
        [ObservableProperty]
        public Brush _hoverBackground;

        /// <summary>
        /// The collection of <see cref="QuickFilterModel"/>s used to perform quick filtering.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Proxy))]
        private ObservableCollection<QuickFilterModel> _quickFilterData = new();

        /// <summary>
        /// Collection that holds a collection of <see cref="FilterCondition"/>s for each column.
        /// </summary>
        public ObservableCollection<ObservableCollection<FilterCondition>> ColumnFilterConditions = new();

        /// <summary>
        /// Indicates whether the column is numerical or not.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Proxy))]
        public bool _isNumericalColumn;

        /// <summary>
        /// Indicates whether the current quickfilter list view is valid.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Proxy))]
        public bool _isQuickFilterValid = true;

        /// <summary>
        /// The binding proxy used to allow the UI elements not part of the visual tree, to access and modify the data.
        /// </summary>
        public BindingProxy Proxy
        {
            get
            {
                var prox = new BindingProxy() { Data = cstmDataGrid };
                this.Resources.Remove("Proxy");
                this.Resources.Add("Proxy", prox);
                this.InvalidateVisual();
                return prox;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDataGrid"/> class, sets datacontext and adds event handlers.
        /// </summary>
        public CustomDataGrid()
        {
            InitializeComponent();
            this.DataContext = this;
            Loaded += CustomDataGrid_Loaded;
        }

        /// <summary>
        /// Event handler for when the <see cref="CustomDataGrid"/> is loaded. Adds an empty <see cref="FilterCondition"/>  to each column of the <see cref="ColumnFilterConditions"/>  list.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">Event arguments</param>
        private void CustomDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize an empty filter condition for each column
            foreach (DataGridColumn col in BaseDataGrid.Columns)
                ColumnFilterConditions.Add(new ObservableCollection<FilterCondition>());

            SizeChanged += FillDataGrid;
        }

        /// <summary>
        /// This method fills the DataGrid with columns to match the width of the DataGrid control.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event data, including the previous and new sizes of the column that triggered the event.</param>
        private void FillDataGrid(object sender, SizeChangedEventArgs e)
        {
            // Calculate the sum of the width of all columns, including the left row resize bar (6 pixels wide)
            double widthSum = 6;
            foreach (var column in BaseDataGrid.Columns)
            {
                widthSum += column.ActualWidth;
            }
            // Check if the sum of the column widths is less than the DataGrid width
            if (e.NewSize.Width > widthSum)
            {
                // Increase the width of the last column to fill the remaining space in the DataGrid
                BaseDataGrid.Columns.Last().Width = BaseDataGrid.Columns.Last().ActualWidth + (BaseDataGrid.ActualWidth - widthSum);
            }
            else
            {
                // Decrease the width of the last column to fill the space in the DataGrid and not more
                BaseDataGrid.Columns.Last().Width = BaseDataGrid.Columns.Last().ActualWidth - (widthSum - e.NewSize.Width);
            }
        }

        /// <summary>
        /// Added event handler for the SizeChanged event of the DataGridCell. 
        /// Resizes all columns in the DataGrid to ensure that the width of the columns and the DataGrid match and implements custom resizing functioniality.
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        /// <param name="e">The event data</param>
        private void DataGridCell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGridCell col = sender as DataGridCell;
            double diff = e.PreviousSize.Width - e.NewSize.Width;

            // Calculate the sum of the width of all columns, including the left row resize bar (6 pixels wide)
            double widthSum = 6;
            foreach (var column in BaseDataGrid.Columns)
            {
                widthSum += column.ActualWidth;
            }

            // If the column size has increased
            if (e.PreviousSize.Width < e.NewSize.Width)
            {
                // Check if the sum of the column widths is wider than the DataGrid width
                if (widthSum > BaseDataGrid.ActualWidth + 8)
                {
                    bool allMin = true;
                    double columnCut = Math.Abs(diff) / ((BaseDataGrid.Columns.Count) - col.Column.DisplayIndex + 1);

                    // Reduce the width of all columns to the right of the current column
                    for (int i = col.Column.DisplayIndex + 1; i < BaseDataGrid.Columns.Count; i++)
                    {
                        if (BaseDataGrid.Columns[i].ActualWidth > BaseDataGrid.Columns[i].MinWidth)
                        {
                            BaseDataGrid.Columns[i].Width = BaseDataGrid.Columns[i].ActualWidth - columnCut;
                            allMin = false;
                        }
                    }
                    // If all columns to the right have reached their minimum width, set the width of the current column to its previous width
                    if (allMin)
                    {
                        col.Column.Width = e.PreviousSize.Width;
                    }
                }
            }
            // If the column size has decreased
            else
            {
                // Check if the sum of the column widths is less than the DataGrid width
                if (widthSum < BaseDataGrid.ActualWidth)
                {
                    bool allMin = true;
                    double columnCut = Math.Abs(diff) / ((BaseDataGrid.Columns.Count) - col.Column.DisplayIndex + 1);

                    // Increase the width of all columns to the right of the current column
                    for (int i = col.Column.DisplayIndex + 1; i < BaseDataGrid.Columns.Count; i++)
                    {
                        if (BaseDataGrid.Columns[i].ActualWidth < BaseDataGrid.Columns[i].MaxWidth)
                        {
                            BaseDataGrid.Columns[i].Width = BaseDataGrid.Columns[i].ActualWidth + columnCut;
                            allMin = false;
                        }
                    }
                    // If all columns to the right have reached their maximum width, set the width of the current column to its previous width
                    if (allMin)
                    {
                        col.Column.Width = e.PreviousSize.Width;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the mouse down event for the <see cref="SplitButtonControl.SplitButton"/> in the header of a column
        /// </summary>
        /// <param name="sender"><see cref="SplitButtonControl.SplitButton"/></param>
        /// <param name="e">The mouse button event arguments</param>
        private void SplitButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Get the column header that the split button belongs to
            DataGridColumnHeader columnHeader = (sender as FrameworkElement).TemplatedParent as DataGridColumnHeader;
            ColumnIndex = BaseDataGrid.Columns.IndexOf(columnHeader.Column);
            // Check if it's an NumericalColumn and set the variable
            IsNumericalColumn = CheckIfIsNumericalColumn();

            // Initilize the QuickFilterData collection to store the items in the quick filter menu
            QuickFilterData = new ObservableCollection<QuickFilterModel> { new QuickFilterModel { Name = "Select All" } };

            // If there are no items in the datagrid, return early
            if (BaseDataGrid.Items.Count == 0)
                return;

            // Keep track of whether all items are selected
            bool all = true;
            Collection<string> alreadyFound = new Collection<string>();

            // Get the property of the first item that corresponds to the column
            var property = BaseDataGrid.Items[0].GetType().GetProperty((string)columnHeader.Column.Header);

            // Iterate over all items in the datagrid's source collection
            foreach (var item in BaseDataGrid.Items.SourceCollection)
            {
                string name = property.GetValue(item).ToString();

                // If this name has not been seen before, add it to the quick filter menu
                if (!alreadyFound.Contains(name))
                {
                    alreadyFound.Add(name);
                    if (BaseDataGrid.Items.Contains(item))
                    {
                        QuickFilterData.Add(new QuickFilterModel { Name = name, IsChecked = true });
                    }
                    else
                    {
                        QuickFilterData.Add(new QuickFilterModel { Name = name, IsEnabled = false });
                        all = false;
                    }
                }
            }
            // Set the "Select All" item's checked state based on whether all items are selected
            QuickFilterData.First().IsChecked = all;
        }

        /// <summary>
        /// Check if the current column is a numerical column or not.
        /// </summary>
        /// <returns>Returns true if the current column is numerical, returns false otherwise</returns>
        private bool CheckIfIsNumericalColumn()
        {
            bool hasValues = false;
            // Loop through all the items in the datagrid
            foreach (var item in BaseDataGrid.Items)
            {
                // Check if the column is of type DataGridCheckBoxColumn
                if (BaseDataGrid.Columns[ColumnIndex].GetCellContent(item).GetType() == typeof(CheckBox))
                {
                    return false;
                }

                // Check if the column is of type DataGridTextColumn
                if (BaseDataGrid.Columns[ColumnIndex].GetCellContent(item).GetType() == typeof(TextBlock))
                {
                    // Get the value of the column
                    TextBlock value = BaseDataGrid.Columns[ColumnIndex].GetCellContent(item) as TextBlock;
                    // If the value is null, we ignore the cell
                    if (value.Text == null)
                    {
                        break;
                    }
                    hasValues = true;

                    // Check if the value is a number or not
                    if (!double.TryParse(value.Text.ToString(), out _))
                    {
                        return false;
                    }
                }
            }
            // If there are no values, we cannot determine if it's numerical or not, thus we return hasValues
            return hasValues;
        }

        /// <summary>
        /// Event handler for the click of the sort menuitem that delegates to the <see cref="Sort(DataGridColumnHeader, ListSortDirection)"/> method.
        /// If the senders name contains "Descending"(ignore case) the sort direction is <see cref="ListSortDirection.Descending"/>, else its <see cref="ListSortDirection.Ascending"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement caller = sender as FrameworkElement;
            DataGridColumnHeader columnHeader = caller.TemplatedParent as DataGridColumnHeader;

            //uncheck previous sort menu item
            CheckedSortMenuItem.IsChecked = false;
            //set new sort menu item to clicked one
            CheckedSortMenuItem = caller as MenuItem;
            //check new sort menu item
            CheckedSortMenuItem.IsChecked = true;

            ListSortDirection sortDirection = ListSortDirection.Ascending;
            if (caller.Name.IndexOf("Descending", StringComparison.OrdinalIgnoreCase) != -1)
            {
                sortDirection = ListSortDirection.Descending;
            }

            Sort(columnHeader, sortDirection);
            (caller.Parent as ContextMenu).IsOpen = false;
        }

        /// <summary>
        /// Sorts the datagrid based on the column header and sort direction.
        /// </summary>
        /// <param name="columnHeader">The header of the column to be sorted</param>
        /// <param name="sortDirection">The direction of the sort</param>
        private void Sort(DataGridColumnHeader columnHeader, ListSortDirection sortDirection)
        {
            // Get the default view of the items in the datagrid
            ICollectionView view = CollectionViewSource.GetDefaultView(BaseDataGrid.ItemsSource);

            // Get the column to be sorted
            DataGridColumn column = columnHeader.Column;

            // Create a SortDescription with the sort member path and sort direction
            SortDescription sortDescription = new SortDescription(column.SortMemberPath, sortDirection);

            // Clear all existing sort descriptions
            view.SortDescriptions.Clear();

            // Add the new sort description
            view.SortDescriptions.Add(sortDescription);

            // Refresh the view to reflect the changes
            view.Refresh();
        }

        /// <summary>
        /// Event handler for the click event of a the menu item. 
        /// Opens the <see cref="FilterWindow"/> and populates it with the selected column information.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FilterModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Get the clicked menu item
            MenuItem menuItem = sender as MenuItem;
            // Create a new instance of FilterWindow, passing in the relevant information such as the base datagrid, the index of the column being filtered, the header of the menu item (used as the filter type), a reference to the column filter conditions, and a flag indicating whether the column is numerical or not.
            FilterWindow window = new FilterWindow(BaseDataGrid, ColumnIndex, menuItem.Header.ToString(), ref ColumnFilterConditions, IsNumericalColumn);
            // Show the FilterWindow as a dialog
            window.ShowDialog();
        }

        /// <summary>
        /// Event handler for when the "Sort Ascending" menu item is initialized. This method sets the sort order for the column to ascending.
        /// </summary>
        /// <param name="sender">The sender of the event, the "Sort Ascending" menu item.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void SortAscendingMenuItem_Initialized(object sender, System.EventArgs e)
        {
            FrameworkElement caller = sender as FrameworkElement;
            DataGridColumnHeader columnHeader = caller.TemplatedParent as DataGridColumnHeader;

            // Check if the content of the column header matches the header of the first column in the DataGrid
            if (columnHeader.Content.ToString() == BaseDataGrid.Columns[0].Header.ToString())
            {
                // Set the "Sort Ascending" menu item as the checked sort menu item
                CheckedSortMenuItem = (sender as MenuItem);
                CheckedSortMenuItem.IsChecked = true;

                // Sort the column in ascending order
                Sort(columnHeader, ListSortDirection.Ascending);
            }
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event for the Context Menu QuickFilterListViewItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void QuickFilterListViewItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Cast the sender to a ListViewItem
            ListViewItem caller = sender as ListViewItem;

            // Get the content of the clicked row
            QuickFilterModel clickedRow = caller.Content as QuickFilterModel;

            // Toggle the IsChecked property of the clicked row
            clickedRow.IsChecked = !clickedRow.IsChecked;

            // Check if the clicked row was unchecked
            if (!clickedRow.IsChecked)
            {
                // If so, disable the "Select All" checkbox
                QuickFilterData.First().IsChecked = false;
            }

            // Check if the "Select All" checkbox is pressed. If so, check all items.
            if (clickedRow.Name == "Select All")
            {
                for (int i = 0; i < QuickFilterData.Count; i++)
                {
                    QuickFilterData[i].IsChecked = clickedRow.IsChecked;
                }
            }

            // Check if any quick filter is selected for context menu "Ok" buttons IsEnabled binding to IsQuickFilterValid.
            IsQuickFilterValid = false;
            foreach (var item in QuickFilterData)
            {
                if (item.IsChecked == true)
                {
                    IsQuickFilterValid = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Converts the <see cref="QuickFilterData"/> to <see cref="FilterCondition"/>s and applies them to the data grid.
        /// </summary>
        private void CvtQuickFilters2FilterConditions()
        {
            // Clear the current filter conditions for the current column
            ColumnFilterConditions[ColumnIndex].Clear();

            // Loop through each quick filter
            foreach (QuickFilterModel quickFilter in QuickFilterData.Skip(1))
            {
                // If the quick filter is checked
                if (quickFilter.IsChecked)
                {
                    // If there are no current filter conditions for the current column
                    if (ColumnFilterConditions[ColumnIndex].Count == 0)
                    {
                        // Add a new filter condition with the "Equals" filter mode and no conjunction
                        ColumnFilterConditions[ColumnIndex].Add(new FilterCondition
                        {
                            FilterMode = FilterModesEnum.Equals.ToString(),
                            HasConjunction = false,
                            FilterValue = quickFilter.Name
                        });
                    }
                    // If there are existing filter conditions for the current column
                    else
                    {
                        // Add a new filter condition with the "Equals" filter mode and an "Or" conjunction
                        ColumnFilterConditions[ColumnIndex].Add(new FilterCondition
                        {
                            FilterMode = FilterModesEnum.Equals.ToString(),
                            Conjunction = ConjunctionsEnum.Or.ToString(),
                            FilterValue = quickFilter.Name
                        });
                    }
                }
            }
            // Get the collection view of the items in the base data grid
            CollectionView view = CollectionViewSource.GetDefaultView(BaseDataGrid.Items.SourceCollection) as CollectionView;

            // Filter the collection view using the current filter conditions
            FilterWindowVM.FilterCollectionView(view, ColumnFilterConditions);
        }

        /// <summary>
        /// Event handler for the "Cancel" button in the context menu. Closes the context menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Btn_ContextMenuCancel(object sender, RoutedEventArgs e)
        {
            ((sender as FrameworkElement).TemplatedParent as ContextMenu).IsOpen = false;
        }

        /// <summary>
        /// Event handler for the "OK" button in the context menu. Closes the context menu and executes the "CvtQuickFilters2FilterConditions" method.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Btn_ContextMenuOk(object sender, RoutedEventArgs e)
        {
            CvtQuickFilters2FilterConditions();

            ((sender as FrameworkElement).TemplatedParent as ContextMenu).IsOpen = false;
        }

    }
}