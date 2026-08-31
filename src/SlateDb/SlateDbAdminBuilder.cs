using SlateDb.Configuration;

namespace SlateDb;

/// <summary>
/// Builder for an administrative <see cref="SlateDbAdmin"/> handle, created via
/// <see cref="SlateDb.CreateAdmin(string)"/>.
/// </summary>
public sealed class SlateDbAdminBuilder
{
    private readonly string _path;
    private AbstractSlateDbConfig _configuration = new MemoryStoreConfig();
    private AbstractSlateDbConfig? _walConfiguration;
    private ulong? _seed;

    internal SlateDbAdminBuilder(string path)
    {
        _path = path;
    }

    /// <summary>Sets the object store the administered database is opened against.</summary>
    public SlateDbAdminBuilder WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        _configuration = configuration;
        return this;
    }

    /// <summary>Uses a separate object store for WAL-backed administrative operations.</summary>
    public SlateDbAdminBuilder WithWalObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        _walConfiguration = configuration;
        return this;
    }

    /// <summary>Sets the seed used for SlateDB's internal random number generation.</summary>
    public SlateDbAdminBuilder WithSeed(ulong seed)
    {
        _seed = seed;
        return this;
    }

    /// <summary>Builds the admin handle.</summary>
    public SlateDbAdmin Build()
    {
        if (string.IsNullOrWhiteSpace(_path))
            throw new SlateDbException("Path is empty");

        try
        {
            using var objectStore = Interop.UniffiHelpers.CreateObjectStore(_configuration);
            var builder = new Interop.AdminBuilder(_path, objectStore);

            if (_walConfiguration != null)
            {
                using var walObjectStore = Interop.UniffiHelpers.CreateObjectStore(_walConfiguration);
                builder.WithWalObjectStore(walObjectStore);
            }

            if (_seed != null)
                builder.WithSeed(_seed.Value);

            return new SlateDbAdmin(builder.Build());
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create Admin: {ex.Message}", ex);
        }
    }
}
