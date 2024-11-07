using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;


class WeatherApp
{
    private static readonly HttpClient client = new HttpClient();
    private static List<Grad> cities;

    static async Task Main(string[] args)
    {
        // Load cities from file
        cities = LoadCitiesFromFile("city.txt");

        Console.WriteLine("Список городов:");
        for (int i = 0; i < cities.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {cities[i].Name}");
        }

        Console.WriteLine("\nВведите номер города для получения прогноза погоды:");
        if (int.TryParse(Console.ReadLine(), out int cityIndex) && cityIndex >= 1 && cityIndex <= cities.Count)
        {
            Grad selectedCity = cities[cityIndex - 1];

            Weather? weather = await GetWeatherAsync(selectedCity.Lat, selectedCity.Lon);
            if (weather != null)
            {
                Console.WriteLine(weather.Value.ToString());
            }
            else
            {
                Console.WriteLine("Не удалось получить данные о погоде.");
            }
        }
        else
        {
            Console.WriteLine("Неверный выбор города.");
        }
    }

    private static List<Grad> LoadCitiesFromFile(string filename)
    {
        List<Grad> cities = new List<Grad>();
        foreach (string line in File.ReadAllLines(filename))
        {
            string[] parts = line.Replace(", ", "\t").Replace(".", ",").Split('\t');
            if (parts.Length == 3)
            {
                Grad city = new Grad(
                    parts[0].Trim(),
                    Convert.ToDouble(parts[1]),
                    Convert.ToDouble(parts[2])
                );
                cities.Add(city);
            }
        }
        return cities;
    }

    private static async Task<Weather?> GetWeatherAsync(double lat, double lon)
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<WeatherApp>();
            IConfiguration configuration = builder.Build();
            string apiKey = configuration["OpenWeatherApiKey"];
            var response = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&appid={apiKey}");
            dynamic json = JsonConvert.DeserializeObject(response);
            if (json.sys.country != null)
            {
                return new Weather
                {
                    Country = json.sys.country,
                    Name = json.name,
                    Temp = json.main.temp,
                    Description = json.weather[0].description
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении данных о погоде: {ex.Message}");
            return null;
        }
    }
}

public struct Weather
{
    public string Country { get; set; }
    public string Name { get; set; }
    public float Temp { get; set; }
    public string Description { get; set; }

    public Weather(string country, string name, float temp, string description)
    {
        Country = country;
        Name = name;
        Temp = temp;
        Description = description;
    }

    public override string ToString()
    {
        return $"Страна: {Country}\nНазвание города: {Name}\nТемпература воздуха: {Temp}°C\nОписание погоды: {Description}\n";
    }
}

public class Grad
{
    public string Name { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }

    public Grad(string name, double lat, double lon)
    {
        Name = name;
        Lat = lat;
        Lon = lon;
    }
}