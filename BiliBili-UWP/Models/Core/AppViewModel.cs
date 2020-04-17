﻿using BiliBili_Lib.Enums;
using BiliBili_Lib.Tools;
using BiliBili_UWP.Components.Controls;
using BiliBili_UWP.Components.Layout;
using BiliBili_UWP.Models.UI;
using BiliBili_UWP.Models.UI.Others;
using BiliBili_UWP.Pages.Main;
using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace BiliBili_UWP.Models.Core
{
    public class AppViewModel
    {
        public double StateChangeWidth = 1000;
        public SideMenuItem SelectedSideMenuItem { get; set; }
        public Type CurrentPageType { get; set; }
        public SidePanel CurrentSidePanel;
        public PagePanel CurrentPagePanel;
        public VideoPlayer CurrentVideoPlayer;

        public List<Tuple<Guid, Action<Size>>> WindowsSizeChangedNotify { get; set; } = new List<Tuple<Guid, Action<Size>>>();
        public AppViewModel()
        {
            Window.Current.SizeChanged += WindowSizeChangedHandle;
        }
        public void WindowSizeChangedHandle(object sender, WindowSizeChangedEventArgs e)
        {
            if (WindowsSizeChangedNotify.Count > 0)
            {
                WindowsSizeChangedNotify.ForEach(p => p.Item2?.Invoke(e.Size));
            }
        }
        public void AppInitByActivated(string argument)
        {
            QueryString args = QueryString.Parse(argument);
            args.TryGetValue("action", out string action);
            
        }
        public async void CheckAppUpdate()
        {
            string localVersion = AppTool.GetLocalSetting(Settings.LocalVersion, "");
            string nowVersion = string.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
            if (localVersion != nowVersion)
            {
                var updateFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Others/Update.txt"));
                string updateInfo = await FileIO.ReadTextAsync(updateFile);
                //if (_updatePopup == null)
                //    _updatePopup = new UpdatePanel(updateInfo);
                //_updatePopup.ShowPopup();
                AppTool.WriteLocalSetting(Settings.LocalVersion, nowVersion);
            }
        }
        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="aid">AV号</param>
        /// <param name="sender">触发控件（用于查找封面以实现连接动画）</param>
        public void PlayVideo(int aid,object sender=null)
        {
            if (CurrentPagePanel.IsSubPageOpen)
                CurrentPagePanel.IsSubPageOpen = false;
            SelectedSideMenuItem = null;
            if (sender != null)
            {
                var image = VisualTreeExtension.VisualTreeFindName<FrameworkElement>((FrameworkElement)sender, "VideoCover");
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("VideoConnectedAnimation", image);
            }
            CurrentSidePanel.SetSelectedItem(Enums.SideMenuItemType.Line);
            CurrentPagePanel.NavigateToPage(Enums.SideMenuItemType.Player, aid);
        }

        public void PlayVideoFullScreen(bool isFull)
        {
            if (isFull)
            {
                PlayerPage.Current.RemovePlayer();
                MainPage.Current.InsertPlayer();
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
            else
            {
                MainPage.Current.RemovePlayer();
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                PlayerPage.Current.InsertPlayer();
            }
        }
    }
}