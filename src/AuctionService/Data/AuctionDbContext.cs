using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /* Default when using EF */
        base.OnModelCreating(modelBuilder);
        
        /* Config outbox mass transit (fault tolerance in msg broker) */
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        
    }
    
    public DbSet<Auction> Auctions { get; set; }
    
}