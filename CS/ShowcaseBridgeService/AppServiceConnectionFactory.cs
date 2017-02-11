using Windows.ApplicationModel.AppService;

namespace ShowcaseBridgeService
{
    public sealed class AppServiceConnectionFactory
    {
        public static AppServiceConnection GetConnection()
        {
            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = "com.microsoft.showcase.bridge";
            connection.PackageFamilyName = "05479d55-aff3-469f-a7d5-bf38a8aeb3f5_b8fas1k6xztg8";
            return connection;
        }
    }
}
