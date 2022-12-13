// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using JYAutoTester.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using CommunityToolkit.WinUI.UI.Controls;
using System.Text.Json;
using System.Text;
using Microsoft.UI;
using static System.Net.Mime.MediaTypeNames;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JYAutoTester.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private int idx = -1;
        public SettingsViewModel ViewModel { get; set; }
        public SettingPage()
        {
            this.InitializeComponent();
            ViewModel = new SettingsViewModel();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            
            var cat = (this.category.SelectedItem as AssemblieInfo).Name;
            var path = this.path.Text.Trim();
            ViewModel.AddNewExternalReference(cat, path);

            if (this.btn_newExtRef.Flyout is Flyout f)
            {
                this.category.SelectedIndex = -1;
                this.path.Text = "";
                f.Hide();
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.btn_newExtRef.Flyout is Flyout f)
            {
                this.category.SelectedIndex = -1;
                this.path.Text = "";
                f.Hide();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            idx = (sender as DataGrid).SelectedIndex;
            var str= ViewModel.ModuleInfos[idx].RawString;
            module_editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, str);
        }

        private void mod_Update_Click(object sender, RoutedEventArgs e)
        {
            if (idx!=-1)
            {
                string output;
                module_editor.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out output);
                ViewModel.UpdateModuleInfo(idx, output);

            }
        }

        private static string BeautifyJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private void RichEditBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key==  Windows.System.VirtualKey.Enter)
            {
                try
                {
                    var cursorPos=module_editor.Document.Selection.StartPosition;
                    string output = "";
                    module_editor.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out output);
                    var beauty = BeautifyJson(output);
                    module_editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, beauty);
                    module_editor.Document.Selection.StartPosition = cursorPos;
                }
                catch (Exception)
                {

                }

            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            nlog_editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, ViewModel.NLogSetting);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            nlog_editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");
        }

        private void nlog_Update(object sender, RoutedEventArgs e)
        {
            string output= "";
            if ((bool)chkBox_nlog.IsChecked)
            {                
                nlog_editor.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out output);                
            }
            ViewModel.NLogSetting = output;
        }

        private async void Configuration_commit(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new Dialogs.FileSavingDialog();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.Commit();
            }
        }

        private void btn_newModule_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateNewModule();

        }

        private void nlog_LoadDefault(object sender, RoutedEventArgs e)
        {
            nlog_editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, ViewModel.LoadDefaultSetting());
        }
    }

    public class AssemblieInfo
    {
        public string Name { get; set; }
        public ObservableCollection<AssemblieInfo> Files { get; set; } = new ObservableCollection<AssemblieInfo>();
    }


}
