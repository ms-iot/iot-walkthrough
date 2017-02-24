using Windows.ApplicationModel.AppService;

namespace ShowcaseBridgeService
{
    public sealed class AppServiceConnectionFactory
    {
        public static AppServiceConnection GetConnection()
        {
            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = "com.microsoft.showcase.bridge";
            connection.PackageFamilyName = "19434TiagoShibata.devex-showcase_gr440wvt0bh62";
            return connection;
        }
    }
}
