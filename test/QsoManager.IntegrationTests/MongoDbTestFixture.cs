namespace QsoManager.IntegrationTests;

public class MongoDbTestFixture
{
    public int Port { get; private set; }
    public string ConnectionString => $"mongodb://admin:password@localhost:{Port}";

    public MongoDbTestFixture()
    {
        // Génération d'un port aléatoire une seule fois pour toute la classe de tests
        var random = new Random();
        Port = random.Next(10000, 30001);
    }
}
