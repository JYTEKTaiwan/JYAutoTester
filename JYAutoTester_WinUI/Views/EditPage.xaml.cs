// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage;
using Microsoft.UI.Text;
using Windows.UI;
using System.Runtime.InteropServices;
using Windows.UI.Popups;
using WinRT.Interop;
using JYAutoTester.ViewModels;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.ServiceModel.Channels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JYAutoTester.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public EditorViewModel ViewModel { get; private set; }
        public EditPage()
        {
            this.InitializeComponent();
            ViewModel = new EditorViewModel();
        }
        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {

            editor.Document.SetText(TextSetOptions.None, ViewModel.OpenFile());

        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string text;
            editor.Document.GetText(TextGetOptions.None, out text);
            if (!string.IsNullOrEmpty(ViewModel.FilePath))
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Save your work?";
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Don't Save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new Dialogs.FileSavingDialog();

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.SaveFile(text);
                }

            }
            else
            {
                ViewModel.SaveFile(text);
            }



        }

        private void TestItemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is RadioButtons rbs)
            {
                var type = rbs.SelectedItem as string;
                switch (type)
                {
                    case "Executer":
                        panel_executer.Visibility = Visibility.Visible;
                        panel_script.Visibility = Visibility.Collapsed;
                        break;
                    case "Script":
                        panel_executer.Visibility = Visibility.Collapsed;
                        panel_script.Visibility = Visibility.Visible;
                        break;
                    default:
                        panel_executer.Visibility = Visibility.Collapsed;
                        panel_script.Visibility = Visibility.Collapsed;
                        break;
                }

            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.SetText(TextSetOptions.None, ViewModel.NewFile());
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("{");
                JsonObject jObj = new JsonObject();
                var itemType = rbs_TestItem.SelectedItem as string;
                var repeat = (int)nBox_Repeat.Value;
                switch (itemType)
                {
                    case "Executer":

                        if (cBox_ModNames.SelectedItem != null && !string.IsNullOrEmpty(tBox_MethodCmd.Text))
                        {
                            sb.Append($"    \"Executer\":{{\"{cBox_ModNames.SelectedItem.ToString()}\":{tBox_MethodCmd.Text}}}");
                        }

                        if (!string.IsNullOrEmpty(tBox_AnalyzeCmd.Text))
                        {
                            sb.AppendLine(",");
                            sb.Append($"    \"Analyzer\":{tBox_AnalyzeCmd.Text}");
                        }
                        var retest = rbs_ReTest.SelectedItem as string;
                        if (retest != "None" && nBox_iteration.Value > 1)
                        {
                            sb.AppendLine(",");
                            sb.Append($"    \"{retest}\":{(int)nBox_iteration.Value}");
                        }
                        sb.AppendLine();
                        break;
                    case "Script":
                        if (!string.IsNullOrEmpty(tBox_scriptPath.Text))
                        {
                            sb.Append($"    \"Script\":\"{tBox_scriptPath.Text}\"");
                        }
                        if (repeat > 1)
                        {
                            sb.AppendLine(",");
                            sb.AppendLine($"    \"Repeat\":{repeat}");
                        }
                        else
                        {
                            sb.AppendLine();
                        }
                        break;
                    default:
                        break;
                }
                sb.Append("}");
                item_preview.Document.SetText(TextSetOptions.None, sb.ToString());
            }
            catch (Exception ex)
            {
            }
        }

    }
}
