/*
* LICENSE: https://raw.github.com/apimash/StarterKits/master/LicenseTerms-SampleApps%20.txt
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using XPlatformCloudKit.Resources;
using XPlatformCloudKit.ViewModels;
using XPlatformCloudKit.Models;
using XPlatformCloudKit.Services;
using XPlatformCloudKit.Helpers;
using Cirrious.MvvmCross.WindowsPhone.Views;
using Cirrious.MvvmCross.Plugins.File;
using Cirrious.CrossCore;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using AppPromo;
using Microsoft.Advertising.Mobile.UI;

namespace XPlatformCloudKit
{
    public partial class ItemsShowcaseView : MvxPhonePage
    {
        private static LicenseInformation licenseInfo = new LicenseInformation();

        // Constructor
        public ItemsShowcaseView()
        {
            InitializeComponent();
            
            DataContext = new ItemsShowcaseViewModel();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            SearchBox.TextChanged += SearchBox_TextChanged;
            Loaded += ItemsShowcaseView_Loaded;

            if (AppSettings.EnablePhoneBackground8X == true)
            {
                LayoutRoot.Background = Application.Current.Resources["WallPaperBrush"] as ImageBrush;
                LayoutRoot.Background.Opacity = .5;
            }

            if (AppSettings.EnableAppPromoRatingReminder)
            {
                RateReminder rateReminder = new RateReminder();
                rateReminder.RunsBeforeReminder = AppSettings.NumberOfRunsBeforeRateReminder;
                LayoutRoot.Children.Add(rateReminder);
            }

            if (AppSettings.EnablePubcenterAdsPhone8)
            {
                if (AppSettings.HideAdsIfPurchasedPhone8)
                    if (!licenseInfo.IsTrial())
                        return;

                var advertisingControlPlaceholder = new RowDefinition();
                advertisingControlPlaceholder.Height = new GridLength(80);
                LayoutRoot.RowDefinitions.Add(advertisingControlPlaceholder);
                var appbarSpacer = new RowDefinition();
                appbarSpacer.Height = new GridLength(30);
                LayoutRoot.RowDefinitions.Add(appbarSpacer);
                AdControl adControl = new AdControl(AppSettings.PubcenterApplicationIdPhone8, AppSettings.PubcenterAdUnitIdPhone8, true);
                adControl.Width = 480;
                adControl.Height = 80;
                Grid.SetRow(adControl, 2);
                LayoutRoot.Children.Add(adControl);
            }
        }

        void ItemsShowcaseView_Loaded(object sender, RoutedEventArgs e)
        {
            if(AppSettings.TrialModeEnabled)
            CheckTrial();
        }

        private void CheckTrial()
        {
            //Set up trial mode logic based on first run of app
            var fileStore = Mvx.Resolve<IMvxFileStore>();

            if (licenseInfo.IsTrial() || AppSettings.SimulateTrialMode)
            {
                if (fileStore.Exists("FirstLaunch"))
                {
                    string firstLaunch;
                    if (fileStore.TryReadTextFile("FirstLaunch", out firstLaunch))
                    {
                        var dateTimeOfFirstLaunch = DateTime.Parse(firstLaunch);
                        if ((DateTime.Now - dateTimeOfFirstLaunch).Days >= AppSettings.TrialPeriodInDays)
                        {
                            TrialBlocker.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    fileStore.WriteFile("FirstLaunch", DateTime.Now.ToString());
                }
            }

        }

        private void TrialBlocker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           var button = MessageBox.Show("Press \"ok\" to view this app in the marketplace","Trial Expired", MessageBoxButton.OKCancel);

           if (button == MessageBoxResult.OK)
           {
               MarketplaceDetailTask marketPlaceDetailTask = new MarketplaceDetailTask();
               marketPlaceDetailTask.Show();
           }
        }

        void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ItemsShowcaseViewModel)DataContext).SearchCommand.Execute(SearchBox.Text.ToString());
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (SearchBox.Visibility == Visibility.Visible)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchBox.Text = "";
                ((ItemsShowcaseViewModel)DataContext).ClearSearch.Execute(null);
                e.Cancel = true;
            }
            else
                base.OnBackKeyPress(e);
        }

        private void longListSelector_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var selectedItem = ((FrameworkElement)e.OriginalSource).DataContext as Item;

            if (selectedItem != null)
            {
                ((ItemsShowcaseViewModel)DataContext).SelectedItem = selectedItem;
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ((ItemsShowcaseViewModel)DataContext).RefreshCommand.Execute(new object());
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            SearchBox.Visibility = Visibility.Visible;
            SearchBox.Focus();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}