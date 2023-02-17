using CommunityToolkit.Mvvm.ComponentModel;

namespace ExcelLikeDataGrid.Model
{
    public partial class FilterCondition : ObservableObject
    {
        [ObservableProperty]
        public bool _hasConjunction = true;
        [ObservableProperty]
        public string? _conjunction;
        [ObservableProperty]
        public string _filterMode = string.Empty;
        [ObservableProperty]
        public string _filterValue = string.Empty;
    }


}
