using Xunit;

namespace TestEasyPicPay.Infrastructure;

[CollectionDefinition("DatabaseTests")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}