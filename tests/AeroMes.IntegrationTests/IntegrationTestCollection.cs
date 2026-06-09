using Xunit;

namespace AeroMes.IntegrationTests;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<AeroMesWebFactory>;
