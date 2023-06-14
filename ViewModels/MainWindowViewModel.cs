using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiSlateParser.Models;
using VoiSlateParser.Helper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using MessageBox = System.Windows.MessageBox;

namespace VoiSlateParser.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
        List<SlateLogItem> logItemList = new();
        [ObservableProperty]
        ICollectionView collectionView;
        
        [ObservableProperty]
        SlateLogItem? selectedItem;

        [ObservableProperty] 
        string filterText;
        
        
        string jsonPath = @"C:\TechnicalProjects\VoiSlateParser\data.json";
        string? recordPath;
        FileLoadingHelper fhelper = FileLoadingHelper.Instance;
        public FilterType filterTypes;

        [RelayCommand]
        void LoadItems()
        {
            try
            {
                fhelper.GetLogs(jsonPath);
                logItemList = fhelper.LogList;
                CollectionView = CollectionViewSource.GetDefaultView(logItemList);
                CollectionView.Filter = (item) =>
                {
                    if (string.IsNullOrEmpty(FilterText)) return true;
                    var im = item as SlateLogItem;
                    // find all the fields in the slateLogItem, and find if the filter is 
                    // contained in any of them.
                    return im.Contains(FilterText);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON data: {ex.Message}");
            }
        }

        [RelayCommand]
        void AddItem(SlateLogItem newItem)
        {
            logItemList.Add(newItem);
        }

        void DeleteItem()
        {
            if (SelectedItem != null)
            {
                logItemList.Remove(SelectedItem);
            }
        }

        partial void OnFilterTextChanged(string? oldValue, string newValue) => CollectionView.Refresh();

        public void LoadLogItem(string path) => fhelper.GetLogs(path);
        public void LoadBwf(string path) => fhelper.GetBwf(path);
}

public enum FilterType
{
    name,
    date,
}