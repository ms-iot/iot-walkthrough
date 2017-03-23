using Windows.ApplicationModel.AppService;

namespace ShowcaseBridgeService
{
    public sealed class AppServiceConnectionFactory
    {
        public static AppServiceConnection GetConnection()
        {
            AppServiceConnection connection = new AppServiceConnection()
            {
                AppServiceName = "com.microsoft.showcase.appservice",
                PackageFamilyName = "BackgroundWeatherStation-uwp_ph1m9x8skttmg"
            };
            return connection;
        }
    }
}
