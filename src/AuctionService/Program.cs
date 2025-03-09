using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.RequestHelpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* Add services to the container. */
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(
    opt =>
    {
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
);

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

/* Add service mass transit for connection to message broker */
builder.Services.AddMassTransit(
    s =>
    {
        /* Add outbox for retry send msg when msg send fails */
        s.AddEntityFrameworkOutbox<AuctionDbContext>(
            o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(10);

                o.UsePostgres();
                o.UseBusOutbox();
            });
        
        s.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
        s.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction",  false));
        
        /* Config type of message broker is using */
        s.UsingRabbitMq(
            (context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            }
        );
    }
);

var app = builder.Build();
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

app.Run();