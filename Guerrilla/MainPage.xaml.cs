using NotificationsExtensions.TileContent;
using System;
using Windows.Media;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Guerrilla
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SystemMediaTransportControls systemControls;
        private bool panelOpened = false;
        private SocketClient client;
        private SongMeta meta;

        public MainPage()
        {
            this.InitializeComponent();

            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;

            InitializeTransportControls();

            client = new SocketClient();
            client.SocketMessageReceived += client_SocketMessageReceived;
        }

        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyStatement = new SettingsCommand("privacy", "Privacy Policy", x => Launcher.LaunchUriAsync(
                new Uri("http://guerrilla.azurewebsites.net/privacy-policy/")));

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyStatement);
        }

        void client_SocketMessageReceived(object sender, SocketMessageEventArgs e)
        {
            meta = e.Meta;
            if (meta == null)
            {
                VisualStateManager.GoToState(this, "No_Artist", true);
                artistTextBlock.Text = "";
                songTextBlock.Text = "";
            }
            else
            {
                artistTextBlock.Text = meta.Artist;
                songTextBlock.Text = meta.Song != null ? meta.Song : "";

                if (meta.ArtistImage != null && meta.ArtistImage != "")
                {
                    backgroundImage.Source = new BitmapImage(new Uri(meta.ArtistImage));
                    VisualStateManager.GoToState(this, "Artist_Image", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Artist_No_Image", true);
                }
                if (meta.AlbumArtwork != null && meta.AlbumArtwork != "")
                {
                    albumImage.Source = new BitmapImage(new Uri(meta.AlbumArtwork));
                }
                else
                {
                    albumImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/NoCover.png"));
                }
            }
            UpdateTile();
        }

        private void UpdateTile()
        {
            if (meta == null)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                return;
            }

            IWideTileNotificationContent tile = null;

            if (meta != null && meta.ArtistImage != null && meta.ArtistImage != "")
            {
                tile = TileContentFactory.CreateTileWideImageAndText02();
                ((ITileWideImageAndText02)tile).Image.Src = meta.ArtistImage;
                ((ITileWideImageAndText02)tile).TextCaption1.Text = meta.Song;
                ((ITileWideImageAndText02)tile).TextCaption2.Text = meta.Artist;
            }
            else
            {
                tile = TileContentFactory.CreateTileWideSmallImageAndText02();
                ((ITileWideSmallImageAndText02)tile).Image.Src = meta.AlbumArtwork != null ? meta.AlbumArtwork : "ms-appx:///Assets/Logo.scale-100.png";
                ((ITileWideSmallImageAndText02)tile).TextHeading.Text = meta.Song;
                ((ITileWideSmallImageAndText02)tile).TextBody1.Text = meta.Artist;
            }

            ITileSquarePeekImageAndText01 squareTile = TileContentFactory.CreateTileSquarePeekImageAndText01();
            squareTile.Image.Src = (meta.ArtistImage != null && meta.ArtistImage != "") ? meta.ArtistImage : "ms-appx:///Assets/Logo.scale-100.png";
            squareTile.TextBody1.Text = meta.Song;
            squareTile.TextBody2.Text = meta.Artist;

            tile.SquareContent = squareTile;
            TileNotification notification = tile.CreateNotification();
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(5 * 60);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        private void panelOpenButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, panelOpened ? "Closed" : "Opened", true);
            panelOpened = !panelOpened;
        }

        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            playButtonImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Play-Button.png", UriKind.Absolute));
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Playing:
                    client.Connect();
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    playButtonImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Stop-Button.png", UriKind.Absolute));
                    break;
                case MediaElementState.Paused:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaElementState.Stopped:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaElementState.Closed:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    break;
            }
            if (mediaElement.CurrentState != MediaElementState.Playing)
            {
                client.Disconnect();

                VisualStateManager.GoToState(this, "No_Artist", true);
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            playButtonImage2.Source = playButtonImage.Source;
        }

        void InitializeTransportControls()
        {
            // Hook up app to system transport controls.
            systemControls = SystemMediaTransportControls.GetForCurrentView();
            systemControls.ButtonPressed += systemControls_ButtonPressed;

            // Register to handle the following system transpot control buttons.
            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
        }

        void systemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    PlayMedia();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    PauseMedia();
                    break;
                default:
                    break;
            }
        }

        async void PlayMedia()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaElement.Source = new Uri("http://live.eliberadio.ro:8010/eliberadio-32.aac");
                mediaElement.Play();
            });
        }

        async void PauseMedia()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaElement.Pause();
            });
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.CurrentState != MediaElementState.Playing)
            {
                mediaElement.Source = new Uri("http://live.eliberadio.ro:8010/eliberadio-32.aac");
                mediaElement.AutoPlay = true;
                mediaElement.Play();
            }
            else
            {
                mediaElement.Stop();
            }
        }

        public void StopMediaPlayer()
        {
            mediaElement.Stop();
        }
    }
}
