using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using System;
using Newtonsoft.Json.Linq;
using ClimateDataAnalyticsApi.Models;
using Microsoft.VisualBasic.FileIO;


namespace ClimateDataAnalyticsApi.Services
{
    public class WeatherService
    {
        private readonly IMongoCollection<Weather> _Weather;

        public WeatherService(IDatabaseSettings Weathersettings)
        {

            var Weather_client = new MongoClient(Weathersettings.ConnectionString);
            var Weather_database = Weather_client.GetDatabase(Weathersettings.DatabaseName);

            _Weather = Weather_database.GetCollection<Weather>(Weathersettings.WeatherCollectionName);
        }

        public Weather getjson(Weather weather, string number, int day)
        {


            string address = "https://worldweather.wmo.int/en/json/" + number + "_en.json";

            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers.Add("Content-Type", "application/json");
            dynamic data = JObject.Parse(client.DownloadString(address));
            int flag=0;
            weather = JsonByDateDay(data, weather, day,flag);
            if(Int32.Parse(weather.WeatherIcon)<3)Create(weather);
            else{weather.WeatherIcon="error";return weather;}
            string IdForGets = weather.Country + "-" + weather.City + "-" + ((weather.IssueDate.DayOfYear) - 1) + "-" + (weather.ForecastDate.DayOfYear);
            try
            {
                RemoveByDay(IdForGets);
            }
            catch (InvalidCastException e)
            {

            }

            return weather;
        }

        public Weather JsonByDateDay(dynamic data, Weather weather, int day,int flag)
        {

            weather.Id = null;//"546c776b3e23f5f2ebdd3b03";
            try { weather.City = (data.city.cityName); }
            catch { weather.City = "N/A";flag++; }
            try { weather.Country = (data.city.member.memName); }
            catch { weather.Country = "N/A";flag++; }

            try { weather.IssueDate = Convert.ToDateTime(data.city.forecast.issueDate); }
            catch { weather.IssueDate = new DateTime(0001, 1, 1, 0, 0, 0);flag++; }
            try { weather.ForecastDate = Convert.ToDateTime(data.city.forecast.forecastDay[day].forecastDate); }
            catch { weather.ForecastDate = new DateTime(0001, 1, 1, 0, 0, 0);flag++; }
            try { weather.weather = (data.city.forecast.forecastDay[day].weather); }
            catch { weather.weather = "N/A";flag++; }

            try { weather.MinTemp = (data.city.forecast.forecastDay[day].minTemp); }
            catch { weather.MinTemp = "N/A";flag++; }

            try { weather.MaxTemp = (data.city.forecast.forecastDay[day].maxTemp); }
            catch { weather.MaxTemp = "N/A";flag++; }

            try { weather.WeatherIcon = (data.city.forecast.forecastDay[day].weatherIcon); }
            catch { weather.WeatherIcon = (flag++).ToString(); }

            try { weather.IdForGets = weather.Country + "-" + weather.City + "-" + weather.IssueDate.DayOfYear + "-" + weather.ForecastDate.DayOfYear; }
            catch { weather.IdForGets = "N/A";}


            return weather;
        }

        public string CityToNumber(string number)
        {
            string[] words = number.Split('-');
            System.Console.WriteLine($"<{words[1]}>");

            var path = @"cities.txt"; // Habeeb, "Dubai Media City, Dubai"
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { ";" });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string Country = fields[0];
                    string City = fields[1];
                    String CityId = fields[2];
                    if (words[1] == fields[1])
                    {
                        return fields[2];
                    }
                }



            }
            return number;

        }

        public List<Weather> Get() => _Weather.Find(Weather => true).ToList();

        public Weather Get(string Id) => _Weather.Find<Weather>(Weather => Weather.Id == Id).FirstOrDefault();


        public Weather Get_ByCity(string IdForGets)
        {
            var results = _Weather.Find(x => x.IdForGets == IdForGets).FirstOrDefault();
            return results;
            // var filter = Builders<Weather>.Filter.Eq(x => x.IdForGets, IdForGets);
            // var results = collection.Find(filter).ToList();

        }

        public Weather Create(Weather Weather)
        {
            _Weather.InsertOne(Weather);

            return Weather;
        }

        public void Update(string Id, Weather WeatherIn) => _Weather.ReplaceOne(Weather => Weather.Id == Id, WeatherIn);

        public void Remove(Weather WeatherIn) => _Weather.DeleteOne(Weather => Weather.Id == WeatherIn.Id);

        public void Remove(string Id) => _Weather.DeleteOne(Weather => Weather.Id == Id);

        public void RemoveByDay(string IdForGets) => _Weather.DeleteOne(Weather => Weather.IdForGets == IdForGets);
    }

}