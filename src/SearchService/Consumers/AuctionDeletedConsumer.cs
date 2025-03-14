using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"AuctionDeletedConsumer");

        var result = await DB.DeleteAsync<Item>(s => s.ID == context.Message.Id);

        if (!result.IsAcknowledged)
        {
            Console.WriteLine($"AuctionDeletedConsumer: failed to delete item {context.Message.Id}");
        }
        else
        {
            Console.WriteLine($"AuctionDeletedConsumer: item deleted  {context.Message.Id}");
        }
    }
}