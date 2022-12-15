// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.WinUI.UI.Controls;
using JYAutoTester.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JYAutoTester.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExecutePage : Page
    {


        public MATSysViewModel ViewModel { get; set; }
        public ExecutePage()
        {
            this.InitializeComponent();
            this.ViewModel = new MATSysViewModel();

            var view = new CollectionViewSource()
            {
                IsSourceGrouped = true,
                Source = ViewModel.TestItems.GroupBy(x => x.Category),
            };
            datagrid.ItemsSource = view.View;

            TypeDescriptor.GetProperties(ViewModel)["CurrentItem"].AddValueChanged(ViewModel, CurrentItemChangedEvent);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OpeningPage();
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ClosingPage();
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            btn_start.IsEnabled = false;
            btn_stop.IsEnabled = true;
            this.ViewModel.StartTest();
        }

        private void tBox_msg_TextChanged(object sender, TextChangedEventArgs e)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(tBox_msg, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f, true);
                break;
            }

        }

        private void CurrentItemChangedEvent(object sender, EventArgs e)
        {
            var currentRow = (sender as MATSysViewModel).CurrentItem;
            var idx = (sender as MATSysViewModel).TestItems.IndexOf(currentRow);
            DispatcherQueue.TryEnqueue(() =>
            {
                datagrid.SelectedIndex = idx;

                datagrid.UpdateLayout();

                datagrid.ScrollIntoView(currentRow, null);

                datagrid.UpdateLayout();

            });

        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StopTest();
            btn_start.IsEnabled = true;
            btn_stop.IsEnabled = false;
        }

        private void datagrid_LoadingRowGroup(object sender, DataGridRowGroupHeaderEventArgs e)
        {
            var group = e.RowGroupHeader.CollectionViewGroup;
            var item = group.GroupItems[0] as TestItemData;
            e.RowGroupHeader.PropertyValue = item.Category;
        }

        private void logPanel_unchecked(object sender, RoutedEventArgs e)
        {
            log_panel.Height = new GridLength(30, GridUnitType.Pixel);
            tBox_msg.Visibility = Visibility.Collapsed;
        }

        private void logPanel_checked(object sender, RoutedEventArgs e)
        {
            log_panel.Height = new GridLength(130, GridUnitType.Pixel);
            tBox_msg.Visibility = Visibility.Visible;
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExportReport();
        }
    }

}

