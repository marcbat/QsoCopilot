using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using Xunit;

namespace QsoManager.IntegrationTests;

/// <summary>
/// Collection de tests d'intégration qui force l'exécution séquentielle des classes de tests.
/// Cela évite les conflits entre les containers MongoDB de différentes classes de tests.
/// </summary>
[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<WebApplicationFactory<Program>>, ICollectionFixture<MongoDbTestFixture>
{
    // Cette classe existe uniquement pour définir la collection de tests
    // Elle force l'exécution séquentielle grâce à DisableParallelization = true
}
