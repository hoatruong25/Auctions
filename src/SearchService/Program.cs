using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.RequestHelpers;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>();
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

/* Add service mass transit for connection to message broker */
builder.Services.AddMassTransit(
    s =>
    {
        s.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
        
        /* Add prefix  to queue name => avoid conflict with other services */
        s.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
        
        s.UsingRabbitMq(
            (context, cfg) =>
            {
                /* Config actions for search-auction-created queue */
                cfg.ReceiveEndpoint("search-auction-created", e =>
                {
                    e.UseMessageRetry(r => r.Interval(5,5));
                   
                    /* Specify the consumer who handle retry event */
                    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
                });
                
                cfg.ConfigureEndpoints(context);
            }
        );
    }
);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

/* Make process initailize db fucntion async , If it fails system will run next process and retry later */
app.Lifetime.ApplicationStarted.Register(
    async () =>
    {
        try
        {
            await DbInitializer.InitializeAsync(app);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    });

app.Run();

/* Add policy for catch exception and retry http request */
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));