using SlateDb;
using SlateDb.Configuration;
using SlateDb.Options;

namespace SlateDbUnitTests;

public class SlateDb_FilterPolicyTest
{
    private sealed class ThreeByteExtractor : IPrefixExtractor
    {
        public string Name() => "fixed3";

        public ulong? PrefixLen(PrefixTarget target)
        {
            var input = target switch
            {
                PrefixTarget.Point p => p.Key,
                PrefixTarget.Prefix p => p.PrefixValue,
                _ => throw new ArgumentOutOfRangeException(nameof(target)),
            };
            return input.Length >= 3 ? 3UL : null;
        }
    }

    [Test]
    public void CreateBloom_HasDefaultBloomName()
    {
        using var policy = SlateDbFilterPolicy.CreateBloom(10);

        Assert.That(policy.Name, Is.EqualTo("_bf"));
    }

    [Test]
    public void CreateBloom_NameIsIndependentOfBitsPerKey()
    {
        using var policy1 = SlateDbFilterPolicy.CreateBloom(4);
        using var policy2 = SlateDbFilterPolicy.CreateBloom(20);

        Assert.That(policy1.Name, Is.EqualTo(policy2.Name));
    }

    [Test]
    public void CreateBloomWithOptions_NoExtractor_UsesDefaultBloomName()
    {
        using var policy = SlateDbFilterPolicy.CreateBloomWithOptions(new BloomFilterOptions());

        Assert.That(policy.Name, Is.EqualTo("_bf"));
    }

    [Test]
    public void CreateBloomWithOptions_WithPrefixExtractor_NameIncludesExtractorName()
    {
        using var policy = SlateDbFilterPolicy.CreateBloomWithOptions(
            new BloomFilterOptions(), new ThreeByteExtractor());

        Assert.That(policy.Name, Is.EqualTo("_bf:p=fixed3"));
    }

    [Test]
    public void WithFilterPolicies_SingleBloomPolicy_DatabaseWorks()
    {
        using var policy = SlateDbFilterPolicy.CreateBloom(10);

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithFilterPolicies([policy])
            .Build();

        db.Put("key1", "value1");

        Assert.That(db.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithFilterPolicies_EmptyCollection_DisablesFiltersButDatabaseStillWorks()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithFilterPolicies(Array.Empty<SlateDbFilterPolicy>())
            .Build();

        db.Put("key1", "value1");

        Assert.That(db.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithFilterPolicies_DisposedAfterBuild_DatabaseStillWorks()
    {
        var policy = SlateDbFilterPolicy.CreateBloom(10);

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithFilterPolicies([policy])
            .Build();

        policy.Dispose();

        db.Put("key1", "value1");

        Assert.That(db.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithFilterPolicies_CustomPrefixExtractor_ScanPrefixStillReturnsMatches()
    {
        using var policy = SlateDbFilterPolicy.CreateBloomWithOptions(
            new BloomFilterOptions(), new ThreeByteExtractor());

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithFilterPolicies([policy])
            .Build();

        db.Put("ca:Vancouver", "1");
        db.Put("ca:Toronto", "2");
        db.Put("us:Austin", "3");

        var results = db.ScanPrefix("ca:").Select(kv => kv.Key).OrderBy(k => k).ToList();

        Assert.That(results, Is.EqualTo(new[] { "ca:Toronto", "ca:Vancouver" }));
    }

    [Test]
    public void Get_With64ByteFilterContext_DoesNotThrow()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();

        db.Put("key1", "value1");

        var options = new ReadOptions { FilterContext = new FilterContext.Bytes(new byte[64]) };

        Assert.That(() => db.Get("key1", options), Throws.Nothing);
        Assert.That(db.Get("key1", options), Is.EqualTo("value1"));
    }

    [Test]
    public void Get_WithArbitraryLengthFilterContext_DoesNotThrow()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();

        db.Put("key1", "value1");

        var options = new ReadOptions { FilterContext = new FilterContext.Bytes(new byte[10]) };

        Assert.That(() => db.Get("key1", options), Throws.Nothing);
        Assert.That(db.Get("key1", options), Is.EqualTo("value1"));
    }

    [Test]
    public void ScanPrefix_WithArbitraryLengthFilterContext_DoesNotThrow()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();

        db.Put("ca:Vancouver", "1");

        var options = new ScanOptions { FilterContext = new FilterContext.Bytes(new byte[10]) };

        Assert.That(() => db.ScanPrefix("ca:", options).ToList(), Throws.Nothing);
    }
}
