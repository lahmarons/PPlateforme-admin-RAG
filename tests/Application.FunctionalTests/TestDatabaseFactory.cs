namespace IdaraTech.Application.FunctionalTests;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync()
    {
        try
        {
            var containerDatabase = new SqlTestcontainersTestDatabase();
            await containerDatabase.InitialiseAsync();
            return containerDatabase;
        }
        catch
        {
            var localDatabase = new SqlTestDatabase();
            await localDatabase.InitialiseAsync();
            return localDatabase;
        }
    }
}
