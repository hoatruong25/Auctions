using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("AuctionUpdatedConsumer: " + context.Message.Id);

        var item = await DB.Find<Item, Item>()
            .Match(s => s.ID == context.Message.Id)
            .ExecuteFirstAsync();
        
        // Need more process if item is null
        if (item is null) throw new Exception();
        
        item.Make = context.Message.Make;
        item.Model = context.Message.Model;
        item.Year = context.Message.Year;
        item.Color = context.Message.Color;
        item.Mileage = context.Message.Mileage;
        
        var result = await DB.Update<Item>()
            .MatchID(context.Message.Id)
            .ModifyOnly(s => new { s.Make, s.Model, s.Year, s.Color, s.Mileage }, item)
            .ExecuteAsync();
        
        if (!result.IsAcknowledged)
        {
            Console.WriteLine($"AuctionUpdatedConsumer: failed to delete item {context.Message.Id}");
        }
        else
        {
            Console.WriteLine($"AuctionUpdatedConsumer: item deleted  {context.Message.Id}");
        }
     }
}