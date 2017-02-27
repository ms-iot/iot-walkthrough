using Windows.ApplicationModel.AppService;

namespace ShowcaseBridgeService
{
    public sealed class AppServiceConnectionFactory
    {
        public static AppServiceConnection GetConnection()
        {
            AppServiceConnection connection = new AppServiceConnection()
            {
                AppServiceName = "com.microsoft.showcase.bridge",
                PackageFamilyName = "19434TiagoShibata.devex-showcase_gr440wvt0bh62"
            };
            return connection;
        }
    }
}
