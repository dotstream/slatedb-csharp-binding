using System.Text.Json.Serialization;
using SlateDb.Configuration.Converter;

namespace SlateDb.Configuration;

/// <summary>
/// Configuration for an object store backed by Google Cloud Storage.
/// </summary>
public class GoogleStoreConfig : AbstractSlateDbConfig
{
    /// <summary>Path to a service account key file.</summary>
    [JsonPropertyName("service_account_path")]
    [SlateDbConfiguration("service_account_path")]
    public string ServiceAccountPath { get; set; }

    /// <summary>Service account key contents, as an alternative to <see cref="ServiceAccountPath"/>.</summary>
    [JsonPropertyName("service_account_key")]
    [SlateDbConfiguration("service_account_key")]
    public string ServiceAccountKey { get; set; }

    /// <summary>Name of the GCS bucket.</summary>
    [JsonPropertyName("bucket_name")]
    [SlateDbConfiguration("bucket_name")]
    public string BucketName { get; set; }

    /// <summary>Path to an application default credentials file.</summary>
    [JsonPropertyName("application_credentials")]
    [SlateDbConfiguration("application_credentials")]
    public string ApplicationCredentialsPath { get; set; }

    /// <summary>Whether to skip request signing entirely (for public buckets).</summary>
    [JsonPropertyName("google_skip_signature")]
    [SlateDbConfiguration("google_skip_signature")]
    public bool? SkipSignature { get; set; }

    /// <summary>Whether to allow plain HTTP connections without TLS.</summary>
    [JsonPropertyName("google_allow_http")]
    [SlateDbConfiguration("google_allow_http")]
    public bool? AllowHttp { get; set; }

    /// <summary>Whether to accept invalid (e.g. self-signed) TLS certificates.</summary>
    [JsonPropertyName("google_allow_invalid_certificates")]
    [SlateDbConfiguration("google_allow_invalid_certificates")]
    public bool? AllowInvalidCertificates { get; set; }

    /// <summary>Timeout for establishing a connection.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_connect_timeout")]
    [SlateDbConfiguration("google_connect_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? ConnectTimeout { get; set; }

    /// <summary>Default content type applied to uploaded objects when none is specified.</summary>
    [JsonPropertyName("google_default_content_type")]
    [SlateDbConfiguration("google_default_content_type")]
    public string DefaultContentType { get; set; }

    /// <summary>Whether to restrict requests to HTTP/1.1 only.</summary>
    [JsonPropertyName("google_http1_only")]
    [SlateDbConfiguration("google_http1_only")]
    public bool? HttpOnly1 { get; set; }

    /// <summary>Whether to restrict requests to HTTP/2 only.</summary>
    [JsonPropertyName("google_http2_only")]
    [SlateDbConfiguration("google_http2_only")]
    public bool? HttpOnly2 { get; set; }

    /// <summary>Interval between HTTP/2 keep-alive pings.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_http2_keep_alive_interval")]
    [SlateDbConfiguration("google_http2_keep_alive_interval", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveInterval { get; set; }

    /// <summary>Timeout waiting for an HTTP/2 keep-alive ping response.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_http2_keep_alive_timeout")]
    [SlateDbConfiguration("google_http2_keep_alive_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveTimeout { get; set; }

    /// <summary>Whether HTTP/2 keep-alive pings are sent even while the connection is idle.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_http2_keep_alive_while_idle")]
    [SlateDbConfiguration("google_http2_keep_alive_while_idle", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveWhileIdle { get; set; }

    /// <summary>Maximum HTTP/2 frame size accepted from the server.</summary>
    [JsonPropertyName("google_http2_max_frame_size")]
    [SlateDbConfiguration("google_http2_max_frame_size")]
    public Int32? Http2MaxFrameSize { get; set; }

    /// <summary>How long an idle pooled connection is kept before being closed.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_pool_idle_timeout")]
    [SlateDbConfiguration("google_pool_idle_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? PoolIdleTimeout { get; set; }

    /// <summary>Maximum number of idle connections kept per host in the connection pool.</summary>
    [JsonPropertyName("google_pool_max_idle_per_host")]
    [SlateDbConfiguration("google_pool_max_idle_per_host")]
    public Int16? PoolMaxIdlePerHost { get; set; }

    /// <summary>URL of an HTTP proxy to route requests through.</summary>
    [JsonPropertyName("google_proxy_url")]
    [SlateDbConfiguration("google_proxy_url")]
    public string ProxyUrl  { get; set; }

    /// <summary>CA certificate used to validate the proxy's TLS certificate.</summary>
    [JsonPropertyName("google_proxy_ca_certificate")]
    [SlateDbConfiguration("google_proxy_ca_certificate")]
    public string ProxyCaCertificate  { get; set; }

    /// <summary>Comma-separated list of hosts excluded from proxying.</summary>
    [JsonPropertyName("google_proxy_excludes")]
    [SlateDbConfiguration("google_proxy_excludes")]
    public string ProxyExcludes { get; set; }

    /// <summary>Whether to randomize the order of resolved addresses when connecting.</summary>
    [JsonPropertyName("google_randomize_addresses")]
    [SlateDbConfiguration("google_randomize_addresses")]
    public bool? RandomizeAddresses  { get; set; }

    /// <summary>Overall timeout for a request.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_timeout")]
    [SlateDbConfiguration("google_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Timeout { get; set; }

    /// <summary>User agent string sent with requests.</summary>
    [JsonPropertyName("google_user_agent")]
    [SlateDbConfiguration("google_user_agent")]
    public String UserAgent { get; set; }
}
