﻿using BiliBili_Lib.Models.BiliBili.Video;
using BiliBili_Lib.Service;
using BiliBili_UWP.Components.Widgets;
using BiliBili_UWP.Dialogs;
using BiliBili_UWP.Models.Core;
using BiliBili_UWP.Models.UI.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BiliBili_UWP.Pages.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page, IRefreshPage
    {
        private bool _isInit = false;
        public BiliViewModel biliVM = App.BiliViewModel;
        public AccountService _account = App.BiliViewModel._client.Account;
        private ObservableCollection<VideoDetail> HistoryCollection = new ObservableCollection<VideoDetail>();
        private bool _isHistoryRequesting = false;
        private int _page = 1;
        public HistoryPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            biliVM.IsLoginChanged += IsLoginChanged;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.AppViewModel.CurrentPagePanel.ScrollToBottom = ScrollViewerBottomHandle;
            if (_isInit || e.NavigationMode == NavigationMode.Back)
            {
                return;
            }
            await Refresh();
            _isInit = true;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            App.AppViewModel.CurrentPagePanel.ScrollToBottom = null;
            base.OnNavigatingFrom(e);
        }
        public async Task Refresh()
        {
            HistoryCollection.Clear();
            _page = 1;
            if (biliVM.IsLogin)
            {
                await LoadHistory();
            }
        }
        private async Task LoadHistory(bool isIncrease = false)
        {
            if (_isHistoryRequesting)
                return;
            _isHistoryRequesting = true;
            VideoLoadingBar.Visibility = Visibility.Visible;
            if (isIncrease)
                _page += 1;
            var data = await _account.GetVideoHistoryAsync(_page);
            if (data != null && data.Count > 0)
                data.ForEach(p => HistoryCollection.Add(p));
            HolderText.Visibility = HistoryCollection.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            VideoLoadingBar.Visibility = Visibility.Collapsed;
            _isHistoryRequesting = false;
        }
        private async void ScrollViewerBottomHandle()
        {
            await LoadHistory(true);
        }
        private async void IsLoginChanged(object sender, bool e)
        {
            await Refresh();
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!biliVM.CheckAccoutStatus())
                return;
            var dialog = new ConfirmDialog("您确认清空观看的历史记录吗？");
            dialog.PrimaryButtonClick += async (_s, _e) =>
            {
                _e.Cancel = true;
                dialog.IsPrimaryButtonEnabled = false;
                dialog.PrimaryButtonText = "清空中...";
                bool reuslt = await _account.ClearHistoryAsync();
                if (reuslt)
                {
                    new TipPopup("清空成功").ShowMessage();
                    HistoryCollection.Clear();
                    dialog.Hide();
                }
                else
                {
                    new TipPopup("清空失败").ShowError();
                }
                dialog.PrimaryButtonText = "确认";
                dialog.IsPrimaryButtonEnabled = true;
            };
            await dialog.ShowAsync();
        }

        private void HistoryVideoView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as VideoDetail;
            var ele = HistoryVideoView.ContainerFromItem(item);
            App.AppViewModel.PlayVideo(item.aid, ele);
        }
    }
}