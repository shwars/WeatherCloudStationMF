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
        public static void Main()
        {
            Debug.Print("Execution begins...");
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
                Debug.Print("\n");

                int tempcode = (int)(temperature * 100);

                var url = "http://weathermon.cloudapp.net/api/Temperature/" + tempcode.ToString();
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
                        Debug.Print(new string(chars));
                    }
                }
                Thread.Sleep(1000 * 60);
            }
        }

    }
}
