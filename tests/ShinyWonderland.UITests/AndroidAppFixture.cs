namespace ShinyWonderland.UITests;

public class AndroidAppFixture : PlatformFixture
{
    public override Platform Platform => Platform.Android;
}

[CollectionDefinition("Android")]
public class AndroidAppCollection : ICollectionFixture<AndroidAppFixture>;
