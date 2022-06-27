using AreYouGoingBot.Storage.Migrations;
using Microsoft.EntityFrameworkCore;

var dbContext = new DesignTimeDbContextFactory().CreateDbContext(new [] { "" });
// migrate any database changes on startup (includes initial db creation)
dbContext.Database.Migrate();