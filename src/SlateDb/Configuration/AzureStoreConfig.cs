using System.Text.Json.Serialization;
using SlateDb.Configuration.Converter;
using SlateDb.Interop;

namespace SlateDb.Configuration;

public class AzureStoreConfig : AbstractSlateDbConfig
{
    internal override ObjectStoreType StoreType => ObjectStoreType.Azure;
    
    [JsonPropertyName("access_key")]
    [SlateDbConfiguration("access_key")]
    public string AccessKey { get; set; }
    [JsonPropertyName("account_name")]
    [SlateDbConfiguration("account_name")]
    public string AccountName { get; set; }
    [JsonPropertyName("client_id")]
    [SlateDbConfiguration("client_id")]
    public string ClientId { get; set; }
    [JsonPropertyName("client_secret")]
    [SlateDbConfiguration("client_secret")]
    public string ClientSecret { get; set; }
    [JsonPropertyName("tenant_id")]
    [SlateDbConfiguration("tenant_id")]
    public string TenantId { get; set; }
    [JsonPropertyName("authority_host")]
    [SlateDbConfiguration("authority_host")]
    public string AuthorityHost { get; set; }
    [JsonPropertyName("sas_key")]
    [SlateDbConfiguration("sas_key")]
    public string SasKey { get; set; }
    [JsonPropertyName("token")]
    [SlateDbConfiguration("token")]
    public string Token { get; set; }
    [JsonPropertyName("use_emulator")]
    [SlateDbConfiguration("use_emulator")]
    public bool? UseEmulator { get; set; }
    [JsonPropertyName("endpoint")]
    [SlateDbConfiguration("endpoint")]
    public string Endpoint { get; set; }
    [JsonPropertyName("msi_endpoint")]
    [SlateDbConfiguration("msi_endpoint")]
    public string MsiEndpoint  { get; set; }
    [JsonPropertyName("object_id")]
    [SlateDbConfiguration("object_id")]
    public string ObjectId{ get; set; }
    [JsonPropertyName("msi_resource_id")]
    [SlateDbConfiguration("msi_resource_id")]
    public string MsiResourceId{ get; set; }
    [JsonPropertyName("federated_token_file")]
    [SlateDbConfiguration("federated_token_file")]
    public string FederatedTokenFile { get; set; }
    [JsonPropertyName("use_fabric_endpoint")]
    [SlateDbConfiguration("use_fabric_endpoint")]
    public bool? UseFabricEndpoint { get; set; }
    [JsonPropertyName("use_azure_cli")]
    [SlateDbConfiguration("use_azure_cli")]
    public bool? UseAzureCLI { get; set; }
    [JsonPropertyName("azure_skip_signature")]
    [SlateDbConfiguration("azure_skip_signature")]
    public bool? SkipSignature {get; set;}
    [JsonPropertyName("container_name")]
    [SlateDbConfiguration("container_name")]
    public string ContainerName { get; set; }
    [JsonPropertyName("disable_tagging")]
    [SlateDbConfiguration("disable_tagging")]
    public bool? DisableTagging { get; set; }
    [JsonPropertyName("fabric_token_service_url")]
    [SlateDbConfiguration("fabric_token_service_url")]
    public string FabricTokenServiceUrl { get; set; }
    [JsonPropertyName("fabric_workload_host")]
    [SlateDbConfiguration("fabric_workload_host")]
    public string FabricWorkloadHost { get; set; }
    [JsonPropertyName("fabric_session_token")]
    [SlateDbConfiguration("fabric_session_token")]
    public string FabricSessionToken { get; set; }
    [JsonPropertyName("fabric_cluster_identifier")]
    [SlateDbConfiguration("fabric_cluster_identifier")]
    public string FabricClusterIdentifier { get; set; }
    [JsonPropertyName("azure_allow_http")]
    [SlateDbConfiguration("azure_allow_http")]
    public bool? AllowHttp { get; set; }
    [JsonPropertyName("azure_allow_invalid_certificates")]
    [SlateDbConfiguration("azure_allow_invalid_certificates")]
    public bool? AllowInvalidCertificates { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_connect_timeout")]
    [SlateDbConfiguration("azure_connect_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? ConnectTimeout { get; set; }
    [JsonPropertyName("azure_default_content_type")]
    [SlateDbConfiguration("azure_default_content_type")]
    public string DefaultContentType { get; set; }
    [JsonPropertyName("azure_http1_only")]
    [SlateDbConfiguration("azure_http1_only")]
    public bool? HttpOnly1 { get; set; }
    [JsonPropertyName("azure_http2_only")]
    [SlateDbConfiguration("azure_http2_only")]
    public bool? HttpOnly2 { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_http2_keep_alive_interval")]
    [SlateDbConfiguration("azure_http2_keep_alive_interval", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveInterval { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_http2_keep_alive_timeout")]
    [SlateDbConfiguration("azure_http2_keep_alive_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveTimeout { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_http2_keep_alive_while_idle")]
    [SlateDbConfiguration("azure_http2_keep_alive_while_idle", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveWhileIdle { get; set; }
    [JsonPropertyName("azure_http2_max_frame_size")]
    [SlateDbConfiguration("azure_http2_max_frame_size")]
    public Int32? Http2MaxFrameSize { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_pool_idle_timeout")]
    [SlateDbConfiguration("azure_pool_idle_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? PoolIdleTimeout { get; set; }
    [JsonPropertyName("azure_pool_max_idle_per_host")]
    [SlateDbConfiguration("azure_pool_max_idle_per_host")]
    public Int16? PoolMaxIdlePerHost { get; set; }
    [JsonPropertyName("azure_proxy_url")]
    [SlateDbConfiguration("azure_proxy_url")]
    public string ProxyUrl  { get; set; }
    [JsonPropertyName("azure_proxy_ca_certificate")]
    [SlateDbConfiguration("azure_proxy_ca_certificate")]
    public string ProxyCaCertificate  { get; set; }
    [JsonPropertyName("azure_proxy_excludes")]
    [SlateDbConfiguration("azure_proxy_excludes")]
    public string ProxyExcludes { get; set; }
    [JsonPropertyName("azure_randomize_addresses")]
    [SlateDbConfiguration("azure_randomize_addresses")]
    public bool? RandomizeAddresses  { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("azure_timeout")]
    [SlateDbConfiguration("azure_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Timeout { get; set; }
    [JsonPropertyName("azure_user_agent")]
    [SlateDbConfiguration("azure_user_agent")]
    public String UserAgent { get; set; }
}