using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiSlateParser.Models;
using VoiSlateParser.Helper;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace VoiSlateParser.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
        [ObservableProperty]
        ObservableCollection<SlateLogItem> logItemList = new();
        [ObservableProperty]
        SlateLogItem? selectedItem;
        string jsonPath = @"C:\TechnicalProjects\VoiSlateParser\data.json";
        string? recordPath;
        FileLoadingHelper fhelper = FileLoadingHelper.Instance;

        [RelayCommand]
        void LoadItems()
        {
            try
            {
                fhelper.GetLogs(jsonPath);
                LogItemList = fhelper.LogList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON data: {ex.Message}");
            }
        }

        [RelayCommand]
        void AddItem(SlateLogItem newItem)
        {
            LogItemList.Add(newItem);
        }

        void DeleteItem()
        {
            if (SelectedItem != null)
            {
                LogItemList.Remove((SlateLogItem)SelectedItem);
            }
        }

        [RelayCommand]
        public void GetJsonPath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                jsonPath = dialog.FileName;
            }
        }
        
        [RelayCommand]
        public void GetRecordPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();        //这个方法可以显示文件夹选择对话框
            string selectedPath = folderBrowserDialog.SelectedPath;
            fhelper.GetBwf(selectedPath);
        }

        void SaveButton_Click()
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(logItemList, Formatting.Indented);
                File.WriteAllText("data.json", jsonData);
                MessageBox.Show("Data saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving JSON data: {ex.Message}");
            }
        }
}