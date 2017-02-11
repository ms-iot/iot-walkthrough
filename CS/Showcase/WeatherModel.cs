namespace Showcase
{
    class WeatherModel
    {
        public double Temperature { get { return temperature; } }
        public double Humidity { get { return humidity; } }
        public double Pressure { get { return pressure; } }
        public string Condition { get { return condition; } }
        public string Icon { get { return icon; } }

        private double temperature;
        private double humidity;
        private double pressure;
        private string condition;
        private string icon;

        public WeatherModel(double temperature, double humidity, double pressure, string condition, string icon)
        {
            this.temperature = temperature;
            this.humidity = humidity;
            this.pressure = pressure;
            this.condition = condition;
            this.icon = icon;
        }
    }
}
