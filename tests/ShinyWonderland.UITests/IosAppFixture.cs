namespace ShinyWonderland.UITests;

public class IosAppFixture : PlatformFixture
{
    public override Platform Platform => Platform.iOS;
}

[CollectionDefinition("iOS")]
public class IosAppCollection : ICollectionFixture<IosAppFixture>;
