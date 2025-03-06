using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>();

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