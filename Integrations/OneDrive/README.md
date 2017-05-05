---
---
# OneDrive picture slideshow

## Introduction

We will show a slideshow of pictures from the user's OneDrive folder using the `Microsoft.OneDrive.Sdk` package. The slideshow will have controls to set properties (e.g. stretch type, background color...).

## The layout

The slideshow will slowly transition between images with a fade effect. To achieve the transition, two images will be overlapped (a background image and a foreground image) and the foreground image's alpha value will be slowly changed. Furthermore, an opaque `Grid` is placed between the images to separate them (we don't want the background image to be visible in case the front image has regions with an alpha value). The layout also has a `Storyboard` describing the fade effect.

A `Grid` of controls is placed on the bottom left of the page. These controls can play/pause the slideshow, go backward/forward, set the transition time, set the stretch type and toggle between white and black backgrounds. All of the settings are synchronized to the Reported Properties in the Device Twin.

[The full layout can be found here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/Views/SlideShow.xaml)

The callbacks for the controls simply set properties and send them through the app service. The background color toggle, for example, sets the color of the background and foreground grids and sends the update:

```cs
public sealed partial class SlideShow : Page
{
    private readonly Color BLACK = Color.FromArgb(255, 0, 0, 0);
    private readonly Color WHITE = Color.FromArgb(255, 255, 255, 255);

    private bool _whiteBackground;

    private async void ToggleBackground_Click(object sender, RoutedEventArgs e)
    {
        _whiteBackground = !_whiteBackground;
        SetBackground(_whiteBackground ? WHITE : BLACK);
        await AppServiceBridge.SendMessageAsync(new ValueSet
        {
            ["ConfigSlideShowBackgroundColor"] = _whiteBackground ? "white" : "black"
        });
    }

    private void SetBackground(Color color)
    {
        ForegroundImageGrid.Background = ImageGrid.Background = new SolidColorBrush(color);
    }
}
```

The controls are hidden after some time of inactivity:

```cs
public sealed partial class SlideShow : Page
{
    private DispatcherTimer _hideControlsTimer;

    public SlideShow()
    {
        this.InitializeComponent();

        // ...

        _hideControlsTimer = new DispatcherTimer();
        _hideControlsTimer.Tick += (object timer, object args) =>
        {
            SlideShowControls.Visibility = Visibility.Collapsed;
        };
        _hideControlsTimer.Interval = TimeSpan.FromSeconds(10);
    }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        // ...
        _hideControlsTimer.Start();
        // ...
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // ...
        _hideControlsTimer.Stop();
        // ...
    }

    private void OnPointerMoved(object sender, RoutedEventArgs e)
    {
        if (_images != null && _images.Count != 0)
        {
            // Restart timer.
            _hideControlsTimer.Stop();
            _hideControlsTimer.Start();
            SlideShowControls.Visibility = Visibility.Visible;
        }
    }
}
```

The fade effect is achieved by moving the old image to the foreground, the new image to the background and starting the `Storyboard`, which will fade the foreground image until it's invisible:

```cs
private async void StartFadeAnimation(ThreadPoolTimer timer = null)
{
    if (_images != null)
    {
        await _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        {
            ForegroundImage.Source = BackgroundImage.Source;
            ForegroundImage.Opacity = 1;
            currentImage = (currentImage + 1) % _images.Count;
            BackgroundImage.Source = await LoadImage(_images[currentImage].Id);  // The LoadImage function will be described later.
            SlideShowFade.Begin();
        });
    }
}
```

## Registering your app to OneDrive

[Follow the registration instructions here](https://dev.onedrive.com/app-registration.htm#register-your-app-for-onedrive) to register your app. A password won't be needed; we will identify our app and then log in securely with OAuth2. Next, [open your application list](https://apps.dev.microsoft.com/#/appList). Click your application and copy the OneDrive client ID.

## Fetching images from OneDrive

[Make sure the app is associated with the store before using OneDrive.](../../StoreDeployment/README.md)

We will have a simple `OneDriveItemModel` class to model an item on OneDrive. It holds a `Microsoft.OneDrive.Sdk.Item` and has public field to access commonly used information, such as name and ID. [The model class is available here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/OneDriveItemModel.cs)

We will also use a controller to access the OneDrive folder. The `MsaAuthenticationProvider` class can authenticate a Microsoft account to OneDrive through OAuth2. Calling `RestoreMostRecentFromCacheOrAuthenticateUserAsync`, the login dialog is shown only once to give the app access permission and not on subsequent runs:

```cs
class OneDriveItemController
{
    private readonly string clientId = Keys.ONE_DRIVE_CLIENT_ID;
    private readonly string RETURN_URL = "https://login.live.com/oauth20_desktop.srf";
    private readonly string BASE_URL = "https://api.onedrive.com/v1.0";
    private readonly string[] SCOPES = new string[] { "onedrive.readonly", "wl.signin", "offline_access" };

    private IOneDriveClient _oneDriveClient;
    public IOneDriveClient Client { get { return _oneDriveClient; } }

    public async Task InitAsync()
    {
        var authProvider = new MsaAuthenticationProvider(this.clientId, RETURN_URL, SCOPES, new CredentialVault(this.clientId));
        try
        {
            await authProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
        }
        catch (ServiceException e)
        {
            Debug.WriteLine("OneDrive auth error: " + e);
            throw new Exception($"OneDrive login failed: {e.Message}", e);
        }
        _oneDriveClient = new OneDriveClient(BASE_URL, authProvider);
    }
}
```

A `GetImagesAsync` function will return a list of `OneDriveItemModel`s for a given folder ID. Only files that can be displayed as an image are included:

```cs
/// <summary>
/// Get photos for a directory ID.
/// </summary>
/// <param name="id">ID of the parent item or null for the root.</param>
/// <returns>Photos in the specified item ID.</returns>
public async Task<List<OneDriveItemModel>> GetImagesAsync(string id)
{
    List<OneDriveItemModel> results = new List<OneDriveItemModel>();
    if (_oneDriveClient == null)
    {
        return results;
    }

    IItemRequestBuilder folder;
    Item item;
    try
    {
        folder = string.IsNullOrEmpty(id) ? _oneDriveClient.Drive.Root : _oneDriveClient.Drive.Items[id];
        item = await folder.Request().Expand("children").GetAsync();
    }
    catch (Exception e)
    {
        throw new Exception($"Failed to get OneDrive folder: {e.Message}", e);
    }

    if (item.Children == null)
    {
        return results;
    }

    try
    {
        var items = item.Children.CurrentPage.Where(child => child.Image != null);
        foreach (var child in items)
        {
            results.Add(new OneDriveItemModel(child));
        }
    }
    catch (Exception e)
    {
        throw new Exception($"Failed to enumerate images: {e.Message}", e);
    }

    return results;
}
```

[The full code for the `OneDriveItemController` class can be found here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/OneDriveItemController.cs)

These functions are called at the `Loaded` callback of the `SlideShow.xaml.cs` file. If any error is found when authenticating/listing images, it is printed to a `TextBlock`:

```cs
private async void OnLoaded(object sender, RoutedEventArgs args)
{
    // ...
    try
    {
        await _oneDrive.InitAsync();
        _images = await _oneDrive.GetImagesAsync(null);
    }
    catch (Exception e)
    {
        ShowError(e.Message);
        return;
    }
    // ...
}

private void ShowError(string message)
{
    ErrorTextBlock.Text = message;
    SlideShowControls.Visibility = Visibility.Collapsed;
}
```

Then, to display the image, the content is downloaded as a `MemoryStream`:

```cs
private async Task<BitmapImage> LoadImage(string id)
{
    var bitmap = new BitmapImage();
    using (var response = await _oneDrive.Client.Drive.Items[id].Content.Request().GetAsync())
    {
        if (response is MemoryStream)
        {
            await bitmap.SetSourceAsync(((MemoryStream)response).AsRandomAccessStream());
        }
        else
        {
            using (var memoryStream = new MemoryStream())
            {
                await response.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                await bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
            }
        }
    }
    return bitmap;
}
```

[The full code for the `SlideShow.xaml.cs` file can be found here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/Views/SlideShow.xaml.cs)
