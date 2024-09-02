using Devart.Data.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace SQLiteTests
{
  public class Tests : IDisposable
  {
    //TODO 5PARE: add license key.
    private const string LicenseKey = "";

    public Tests()
    {
      using var dbContext = new TestDbContext(
        this.GetDbContextOptionsForTestDb<TestDbContext>());
      dbContext.Database.EnsureCreated();
    }
    
    [Fact]
    public async Task MultipleCommitsOnMultipleThreadsTest()
    {
      var iterations = 50;
      List<TestEntity> entitiesToCommit = new List<TestEntity>();
      for (int i = 0; i < iterations; i++)
      {
        entitiesToCommit.Add(new TestEntity{ Information = "info" + i });
      }

      await Parallel.ForEachAsync(
        entitiesToCommit,
        new ParallelOptions { MaxDegreeOfParallelism = iterations },
        async (entity, ct) =>
        {
          using var dbContext = new TestDbContext(
            this.GetDbContextOptionsForTestDb<TestDbContext>());
          dbContext.Add(entity);
          await dbContext.SaveChangesAsync(ct);
        });
    }

    public DbContextOptions<TContext> GetDbContextOptionsForTestDb<TContext>()
      where TContext : DbContext
    {
      var dbContextOptionsBuilder = new DbContextOptionsBuilder<TContext>();
      var dbLocation = Directory.GetCurrentDirectory();
      var connectionBuilder = new SQLiteConnectionStringBuilder();
      //TODO 5PARE: creating a directory should be done only once when migrating. Clear up once it is known about the database location's part in the settings.
      Directory.CreateDirectory(dbLocation);
      var fullDbLocation = Path.Combine(dbLocation, $"Test.db");
      connectionBuilder.DataSource = fullDbLocation;
      connectionBuilder.LicenseKey = LicenseKey;
      connectionBuilder.DateTimeFormat = SQLiteDateFormats.ISO8601;
      connectionBuilder.Pooling = true;
      connectionBuilder.JournalMode = JournalMode.WAL;
      var connectionString = connectionBuilder.ToString();
      dbContextOptionsBuilder.UseSQLite(connectionString);
      //used in order to avoid https://github.com/dotnet/efcore/issues/16369
      dbContextOptionsBuilder.ConfigureWarnings(
        warnings => warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
      var options = dbContextOptionsBuilder.Options;
      options.Freeze();

      return options;
    }

    public void Dispose()
    {
      using var dbContext = new TestDbContext(
         this.GetDbContextOptionsForTestDb<TestDbContext>());
      dbContext.Database.EnsureDeleted();
    }
  }
}