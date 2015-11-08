using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO;
using System.Text;

namespace WeatherCloudStation
{
    public class WeatherCloudStation
    {

        protected const bool send_data = true; // Send data to the cloud
        protected const int interval = 120; // Measurement interval in seconds
        public static void Main()
        {
            Debug.Print("Execution begins...");
            SecretLabs.NETMF.Hardware.AnalogInput photo_in = new SecretLabs.NETMF.Hardware.AnalogInput(Pins.GPIO_PIN_A0);
            OutputPort pin = new OutputPort(Pins.GPIO_PIN_SDA, true);
            Thread.Sleep(200);
            pin.Write(false);
            pin.Dispose();

            Debug.Print("Initializing sensor library...");
            SensorLibrary.BMP180 sensor = new SensorLibrary.BMP180();
            sensor.Init(SensorLibrary.BMP180.Mode.BMP085_MODE_STANDARD);

            while (true)
            {
                Debug.Print("Getting readings...");
                double pressure = sensor.GetPressure() / 100.0F;
                double pressuremm = pressure * 0.75006375541921;
                Debug.Print("Pressure:    " + pressuremm.ToString() + " mmHg");
                double temperature = sensor.GetTemperature();
                Debug.Print("Temperature: " + temperature.ToString() + " C");
                int lum = photo_in.Read();
                Debug.Print("Luminocity: " + lum.ToString());
                Debug.Print("\n");

                if (send_data)
                {
                    try
                    {
                        record("Temperature", (int)(temperature * 100));
                        record("Pressure", (int)(pressuremm * 10));
                        record("Luminocity", lum);
                    }
                    catch { }
                }
                Thread.Sleep(1000 * interval);
            }
        }

        private static void record(string measurement, int value)
        {
            try
            {
                var s = GetUrl("http://weathermon.cloudapp.net/api/" + measurement + "/" + value.ToString());
            }
            catch { }
        }

        private static string GetUrl(string url)
        {
            Debug.Print("Sending data via url " + url);
            using (var req = (HttpWebRequest)WebRequest.Create(url))
            {
                req.Method = "GET";
                using (var resp = req.GetResponse())
                {
                    var buffer = new byte[(int)resp.ContentLength];
                    Stream stream = resp.GetResponseStream();
                    int toRead = buffer.Length;
                    while (toRead > 0)
                    {
                        // already read: buffer.Length - toRead
                        int read = stream.Read(buffer, buffer.Length - toRead, toRead);
                        toRead = toRead - read;
                    }
                    char[] chars = Encoding.UTF8.GetChars(buffer);
                    return new string(chars);
                }
            }
        }


    }
}
