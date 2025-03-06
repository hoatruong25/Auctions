using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        await DB.InitAsync(
            "SearchDB", MongoClientSettings
                .FromConnectionString(app.Configuration.GetConnectionString("MongoDBConnection"))
        );

        // Create a text index on the fields we want to search => optimize performance
        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        /* Old Approach using Json for import initialize data*/
        // var count = await DB.CountAsync<Item>();
        // if (count == 0)
        // {
        //     Console.WriteLine("No data - will attempt to seed");
        //     var itemData = await File.ReadAllTextAsync("Data/auctions.json");
        //
        //     var options = new JsonSerializerOptions
        //     {
        //         PropertyNameCaseInsensitive = true
        //     };
        //     
        //     var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
        //     
        //     await DB.InsertAsync(items);
        //
        // }
            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            var items = await httpClient.GetItemsForSearchDb();
            
            Console.WriteLine(items.Count + " items received from AuctionService");
            
            if (items.Count > 0) await DB.SaveAsync(items);
    }
}