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
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using Microsoft.Extensions.Hosting;
using MATSys.Hosting;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Windows.System.RemoteSystems;
using JYAutoTester.ViewModels;
using System.Threading;
using System.Text.Json;
using MATSys.Hosting.Scripting;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using CommunityToolkit.WinUI.UI.Controls;
using System.ComponentModel;
using System.Drawing;
using Microsoft.UI;
using WinRT;

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
            TypeDescriptor.GetProperties(ViewModel)["CurrentItem"].AddValueChanged(ViewModel, CurrentItemChangedEvent);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            

        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
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

            datagrid.ScrollIntoView(currentRow, null);
            datagrid.SelectedIndex = idx;
            
            
        }

        private void datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0] as TestItemData;
            var col = datagrid.Columns.FirstOrDefault(x => x.Header.ToString() == "Result") as DataGridTextColumn;
            var tb=col.GetCellContent(item) as TextBlock;
            tb.Foreground = item.ResultBrush;
            
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StopTest();
            btn_start.IsEnabled = true;
            btn_stop.IsEnabled = false;
        }

    }

}

