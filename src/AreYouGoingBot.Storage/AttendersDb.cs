using AreYouGoingBot.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace AreYouGoingBot.Storage;

public class AttendersDb : DbContext
{
    public DbSet<ChatEvent> ChatEvents { get; set; }
    
    public AttendersDb(DbContextOptions opts) : base(opts)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatEventParticipant>()
            .HasOne(eventParticipant => eventParticipant.ChatEvent)
            .WithMany(@event => @event.Participants);
     
        base.OnModelCreating(modelBuilder);
    }
}