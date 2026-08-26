using System.Text.Json.Serialization;
using SlateDb.Configuration.Converter;

namespace SlateDb.Configuration;

/// <summary>
/// Configuration for an object store backed by Azure Blob Storage.
/// </summary>
public class AzureStoreConfig : AbstractSlateDbConfig
{
    /// <summary>Storage account access key.</summary>
    [JsonPropertyName("access_key")]
    [SlateDbConfiguration("access_key")]
    public string AccessKey { get; set; }

    /// <summary>Storage account name.</summary>
    [JsonPropertyName("account_name")]
    [SlateDbConfiguration("account_name")]
    public string AccountName { get; set; }

    /// <summary>Azure AD application (client) ID, for service principal authentication.</summary>
    [JsonPropertyName("client_id")]
    [SlateDbConfiguration("client_id")]
    public string ClientId { get; set; }

    /// <summary>Azure AD client secret, for service principal authentication.</summary>
    [JsonPropertyName("client_secret")]
    [SlateDbConfiguration("client_secret")]
    public string ClientSecret { get; set; }

    /// <summary>Azure AD tenant ID.</summary>
    [JsonPropertyName("tenant_id")]
    [SlateDbConfiguration("tenant_id")]
    public string TenantId { get; set; }

    /// <summary>Azure AD authority host, for sovereign/government clouds.</summary>
    [JsonPropertyName("authority_host")]
    [SlateDbConfiguration("authority_host")]
    public string AuthorityHost { get; set; }

    /// <summary>Shared access signature (SAS) key used for authentication instead of an account key.</summary>
    [JsonPropertyName("sas_key")]
    [SlateDbConfiguration("sas_key")]
    public string SasKey { get; set; }

    /// <summary>Bearer token used for authentication instead of an account key.</summary>
    [JsonPropertyName("token")]
    [SlateDbConfiguration("token")]
    public string Token { get; set; }

    /// <summary>Whether to target the Azurite/Azure Storage Emulator.</summary>
    [JsonPropertyName("use_emulator")]
    [SlateDbConfiguration("use_emulator")]
    public bool? UseEmulator { get; set; }

    /// <summary>Custom Azure Blob Storage endpoint URL.</summary>
    [JsonPropertyName("endpoint")]
    [SlateDbConfiguration("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>Custom managed identity (MSI) endpoint.</summary>
    [JsonPropertyName("msi_endpoint")]
    [SlateDbConfiguration("msi_endpoint")]
    public string MsiEndpoint  { get; set; }

    /// <summary>Object ID of the managed identity to authenticate as.</summary>
    [JsonPropertyName("object_id")]
    [SlateDbConfiguration("object_id")]
    public string ObjectId{ get; set; }

    /// <summary>Resource ID of the managed identity to authenticate as.</summary>
    [JsonPropertyName("msi_resource_id")]
    [SlateDbConfiguration("msi_resource_id")]
    public string MsiResourceId{ get; set; }

    /// <summary>Path to a file containing a federated identity token (workload identity federation).</summary>
    [JsonPropertyName("federated_token_file")]
    [SlateDbConfiguration("federated_token_file")]
    public string FederatedTokenFile { get; set; }

    /// <summary>Whether to use the Azure Fabric endpoint for authentication.</summary>
    [JsonPropertyName("use_fabric_endpoint")]
    [SlateDbConfiguration("use_fabric_endpoint")]
    public bool? UseFabricEndpoint { get; set; }

    /// <summary>Whether to authenticate using the credentials from the Azure CLI.</summary>
    [JsonPropertyName("use_azure_cli")]
    [SlateDbConfiguration("use_azure_cli")]
    public bool? UseAzureCLI { get; set; }

    /// <summary>Whether to skip request signing entirely (for public containers).</summary>
    [JsonPropertyName("azure_skip_signature")]
    [SlateDbConfiguration("azure_skip_signature")]
    public bool? SkipSignature {get; set;}

    /// <summary>Name of the blob container.</summary>
    [JsonPropertyName("container_name")]
    [SlateDbConfiguration("container_name")]
    public string ContainerName { get; set; }

    /// <summary>Whether to disable tagging requests.</summary>
    [JsonPropertyName("disable_tagging")]
    [SlateDbConfiguration("disable_tagging")]
    public bool? DisableTagging { get; set; }

    /// <summary>Fabric token service URL, when running inside Microsoft Fabric.</summary>
    [JsonPropertyName("fabric_token_service_url")]
    [SlateDbConfiguration("fabric_token_service_url")]
    public string FabricTokenServiceUrl { get; set; }

    /// <summary>Fabric workload host, when running inside Microsoft Fabric.</summary>
    [JsonPropertyName("fabric_workload_host")]
    [SlateDbConfiguration("fabric_workload_host")]
    public string FabricWorkloadHost { get; set; }

    /// <summary>Fabric session token, when running inside Microsoft Fabric.</summary>
    [JsonPropertyName("fabric_session_token")]
    [SlateDbConfiguration("fabric_session_token")]
    public string FabricSessionToken { get; set; }

    /// <summary>Fabric cluster identifier, when running inside Microsoft Fabric.</summary>
    [JsonPropertyName("fabric_cluster_identifier")]
    [SlateDbConfiguration("fabric_cluster_identifier")]
    public string FabricClusterIdentifier { get; set; }

    /// <summary>Whether to allow plain HTTP connections without TLS.</summary>
    [JsonPropertyName("azure_allow_http")]
    [SlateDbConfiguration("azure_allow_http")]
    public bool? AllowHttp { get; set; }

    /// <summary>Whether to accept invalid (e.g. self-signed) TLS certificates.</summary>
    [JsonPropertyName("azure_allow_invalid_certificates")]
    [SlateDbConfiguration("azure_allow_invalid_certificates")]
    public bool? AllowInvalidCertificates { get; set; }

    /// <summary>Timeout for establishing a connection.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_connect_timeout")]
    [SlateDbConfiguration("azure_connect_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? ConnectTimeout { get; set; }

    /// <summary>Default content type applied to uploaded blobs when none is specified.</summary>
    [JsonPropertyName("azure_default_content_type")]
    [SlateDbConfiguration("azure_default_content_type")]
    public string DefaultContentType { get; set; }

    /// <summary>Whether to restrict requests to HTTP/1.1 only.</summary>
    [JsonPropertyName("azure_http1_only")]
    [SlateDbConfiguration("azure_http1_only")]
    public bool? HttpOnly1 { get; set; }

    /// <summary>Whether to restrict requests to HTTP/2 only.</summary>
    [JsonPropertyName("azure_http2_only")]
    [SlateDbConfiguration("azure_http2_only")]
    public bool? HttpOnly2 { get; set; }

    /// <summary>Interval between HTTP/2 keep-alive pings.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_http2_keep_alive_interval")]
    [SlateDbConfiguration("azure_http2_keep_alive_interval", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveInterval { get; set; }

    /// <summary>Timeout waiting for an HTTP/2 keep-alive ping response.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_http2_keep_alive_timeout")]
    [SlateDbConfiguration("azure_http2_keep_alive_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveTimeout { get; set; }

    /// <summary>Whether HTTP/2 keep-alive pings are sent even while the connection is idle.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_http2_keep_alive_while_idle")]
    [SlateDbConfiguration("azure_http2_keep_alive_while_idle", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveWhileIdle { get; set; }

    /// <summary>Maximum HTTP/2 frame size accepted from the server.</summary>
    [JsonPropertyName("azure_http2_max_frame_size")]
    [SlateDbConfiguration("azure_http2_max_frame_size")]
    public Int32? Http2MaxFrameSize { get; set; }

    /// <summary>How long an idle pooled connection is kept before being closed.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_pool_idle_timeout")]
    [SlateDbConfiguration("azure_pool_idle_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? PoolIdleTimeout { get; set; }

    /// <summary>Maximum number of idle connections kept per host in the connection pool.</summary>
    [JsonPropertyName("azure_pool_max_idle_per_host")]
    [SlateDbConfiguration("azure_pool_max_idle_per_host")]
    public Int16? PoolMaxIdlePerHost { get; set; }

    /// <summary>URL of an HTTP proxy to route requests through.</summary>
    [JsonPropertyName("azure_proxy_url")]
    [SlateDbConfiguration("azure_proxy_url")]
    public string ProxyUrl  { get; set; }

    /// <summary>CA certificate used to validate the proxy's TLS certificate.</summary>
    [JsonPropertyName("azure_proxy_ca_certificate")]
    [SlateDbConfiguration("azure_proxy_ca_certificate")]
    public string ProxyCaCertificate  { get; set; }

    /// <summary>Comma-separated list of hosts excluded from proxying.</summary>
    [JsonPropertyName("azure_proxy_excludes")]
    [SlateDbConfiguration("azure_proxy_excludes")]
    public string ProxyExcludes { get; set; }

    /// <summary>Whether to randomize the order of resolved addresses when connecting.</summary>
    [JsonPropertyName("azure_randomize_addresses")]
    [SlateDbConfiguration("azure_randomize_addresses")]
    public bool? RandomizeAddresses  { get; set; }

    /// <summary>Overall timeout for a request.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_timeout")]
    [SlateDbConfiguration("azure_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Timeout { get; set; }

    /// <summary>User agent string sent with requests.</summary>
    [JsonPropertyName("azure_user_agent")]
    [SlateDbConfiguration("azure_user_agent")]
    public String UserAgent { get; set; }
}
