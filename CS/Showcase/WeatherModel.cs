namespace Showcase
{
    class WeatherModel
    {
        public double Temperature { get { return _temperature; } }
        public double Humidity { get { return _humidity; } }
        public double Pressure { get { return _pressure; } }
        public string Condition { get { return _condition; } }
        public string Icon { get { return _icon; } }

        private double _temperature;
        private double _humidity;
        private double _pressure;
        private string _condition;
        private string _icon;

        public WeatherModel(double temperature, double humidity, double pressure, string condition, string icon)
        {
            _temperature = temperature;
            _humidity = humidity;
            _pressure = pressure;
            _condition = condition;
            _icon = icon;
        }
    }
}
