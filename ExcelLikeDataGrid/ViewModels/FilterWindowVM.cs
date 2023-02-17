using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using ExcelLikeDataGrid.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Data;
using ExcelLikeDataGrid.Model;
using System;
using System.Data;
using static ExcelLikeDataGrid.Utilities.Enums;

namespace ExcelLikeDataGrid.ViewModel
{
    public partial class FilterWindowVM : ObservableObject
    {

        #region ObservableProperties

        [ObservableProperty]
        internal List<string> _filterModes;

        [ObservableProperty]
        public List<string> _conjunctions = new(Enum.GetValues(typeof(ConjunctionsEnum)).Cast<ConjunctionsEnum>().Select(e => e.ToString()).ToList());

        ObservableCollection<FilterCondition> FilterConditions;

        public ObservableCollection<ObservableCollection<FilterCondition>> ColumnFilterConditions;

        #endregion

        #region Properties & Constructors

        private DataGrid DataGridToFilter;
        private int ColumnIndex;

        public FilterWindowVM()
        {
        }

        public FilterWindowVM(DataGrid dataGrid, int columnIndex, string startFilterMode, ref ObservableCollection<ObservableCollection<FilterCondition>> columnFilterConditions, bool isNumericalColumn)
        {
            ColumnFilterConditions = columnFilterConditions;
            //necessary to assign variable this way to get the value, rather than the reference
            FilterConditions = new ObservableCollection<FilterCondition>(ColumnFilterConditions[columnIndex]);

            DataGridToFilter = dataGrid;
            ColumnIndex = columnIndex;

            if (isNumericalColumn)
                FilterModes = new(Enum.GetValues(typeof(FilterModesEnum)).Cast<FilterModesEnum>().Select(e => e.ToString()).ToList());
            else
                FilterModes = new(
                    Enum.GetValues(typeof(FilterModesEnum))
                    .Cast<FilterModesEnum>()
                    .Where(x =>
                           x != FilterModesEnum.GreaterThan &&
                           x != FilterModesEnum.GreaterThanOrEqual &&
                           x != FilterModesEnum.LessThan &&
                           x != FilterModesEnum.LessThanOrEqual)
                    .Select(e => e.ToString())
                    .ToList()
                    );

            if (FilterModes.Contains(startFilterMode))
            {
                AddFilter();
                FilterConditions.Last().FilterMode = startFilterMode;
            }
        }
        #endregion

        #region RelayCommands

        [RelayCommand]
        public void AddFilter()
        {
            if (FilterConditions.Count == 0)
            {
                FilterConditions.Add(new FilterCondition { HasConjunction = false });
                return;
            }
            FilterConditions.Add(new FilterCondition { Conjunction = Conjunctions[0] });
        }

        [RelayCommand]
        public void RemoveFilter(ListView filterListView)
        {
            var selected = filterListView.SelectedItems.Cast<FilterCondition>();
            foreach (var item in selected.ToList())
            {
                FilterConditions.Remove(item);
            }

            //make first item have no conjunction
            if (FilterConditions.Count > 0)
            {
                FilterConditions[0].Conjunction = null;
                FilterConditions[0].HasConjunction = false;
            }
        }

        [RelayCommand]
        public void CancelFilter(ICloseable window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        [RelayCommand]
        public void ApplyFilter(ICloseable window)
        {
            //Check if filters are valid
            if (!FiltersValid())
            {
                //TODO Error message
                return;
            }

            //Save temporary filter conditions to datagrids filter conditions
            ColumnFilterConditions[ColumnIndex] = FilterConditions;

            //actually filter data
            CollectionView view = CollectionViewSource.GetDefaultView(DataGridToFilter.Items.SourceCollection) as CollectionView;
            FilterCollectionView(view, ColumnFilterConditions);

            if (window != null)
            {
                window.Close();
            }
        }
        #endregion

        #region FilterFunctionality
        internal static void FilterCollectionView(CollectionView view, ObservableCollection<ObservableCollection<FilterCondition>> columnFilterConditions)
        {
            var ignoreCase = StringComparison.OrdinalIgnoreCase; ;
            view.Filter = delegate (object o)
            {
                int i = 0;
                bool result = true;

                //foreach column
                foreach (ObservableCollection<FilterCondition> filterConditions in columnFilterConditions)
                {
                    // Skip this column if there are no filter conditions set
                    if (filterConditions.Count == 0)
                    {
                        i++;
                        continue;
                    }

                    bool columnResult = true;
                    //foreach condition in column
                    foreach (FilterCondition condition in filterConditions)
                    {
                        bool conditionResult = false;
                        var item = o.GetType().GetProperties()[i].GetValue(o);

                        switch (condition.FilterMode)
                        {
                            case "Equals":
                                conditionResult = item.ToString().Equals(condition.FilterValue, ignoreCase);
                                break;
                            case "NotEquals":
                                conditionResult = !item.ToString().Equals(condition.FilterValue, ignoreCase);
                                break;
                            case "BeginsWith":
                                conditionResult = item.ToString().StartsWith(condition.FilterValue, ignoreCase);
                                break;
                            case "EndsWith":
                                conditionResult = item.ToString().EndsWith(condition.FilterValue, ignoreCase);
                                break;
                            case "Contains":
                                conditionResult = item.ToString().IndexOf(condition.FilterValue, ignoreCase) >= 0;
                                break;
                            case "NotContains":
                                conditionResult = item.ToString().IndexOf(condition.FilterValue, ignoreCase) < 0;
                                break;
                            case "GreaterThan":
                                conditionResult = Convert.ToDouble(item) > Convert.ToDouble(condition.FilterValue);
                                break;
                            case "LessThan":
                                conditionResult = Convert.ToDouble(item) < Convert.ToDouble(condition.FilterValue);
                                break;
                            case "GreaterThanOrEqual":
                                conditionResult = Convert.ToDouble(item) >= Convert.ToDouble(condition.FilterValue);
                                break;
                            case "LessThanOrEqual":
                                conditionResult = Convert.ToDouble(item) <= Convert.ToDouble(condition.FilterValue);
                                break;
                            case "IsNull":
                                conditionResult = item == null;
                                break;
                            case "NotIsNull":
                                conditionResult = item != null;
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        // Check if there's a conjunction
                        if (condition.HasConjunction)
                        {
                            // If conjunction is "And", logical AND between columnResult and conditionResult
                            if (condition.Conjunction == ConjunctionsEnum.And.ToString())
                            {
                                columnResult &= conditionResult;
                            }
                            // If conjunction is "Or", logical OR between columnResult and conditionResult
                            else if (condition.Conjunction == ConjunctionsEnum.Or.ToString())
                            {
                                columnResult |= conditionResult;
                            }
                        }
                        else
                        {
                            // If there's no conjunction, set columnResult to the result of the first condition
                            columnResult = conditionResult;
                        }
                    }
                    // If there's no conjunction, set column
                    // Accumulate the result of all columns
                    result &= columnResult;

                    // If result is false for any column, break the loop
                    if (!result)
                    {
                        break;
                    }

                    // Move to the next column
                    i++;
                }
                return result;
            };
        }

        private bool FiltersValid()
        {
            foreach (FilterCondition condition in FilterConditions)
            {
                if (condition.HasConjunction)
                {
                    if (condition.Conjunction == null)
                        return false;
                }

                if (condition.FilterMode != "IsNull" && condition.FilterMode != "NotIsNull")
                {
                    if (condition.FilterValue == null)
                        return false;
                }

                if (condition.FilterMode == null)
                    return false;
            }
            return true;
        }
        #endregion

        //TODO: implement regex
        public void FilterValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox caller = sender as TextBox;
        }
    }
}