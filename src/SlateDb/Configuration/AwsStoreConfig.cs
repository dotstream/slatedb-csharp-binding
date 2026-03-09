using System.Text.Json.Serialization;
using SlateDb.Configuration.Converter;
using SlateDb.Interop;

namespace SlateDb.Configuration;

/// <summary>
/// Configure how to provide conditional put support for AmazonS3.
/// </summary>
public enum S3ConditionalPut
{
    /// <summary>
    /// Some S3-compatible stores, such as Cloudflare R2 and minio support conditional
    /// put using the standard [HTTP precondition] headers If-Match and If-None-Match
    /// Encoded as `etag` ignoring whitespace
    /// </summary>
    ETagMatch
}

public enum ChecksumAlgorithm
{
    [PropertyConverter("sha256")]
    SHA256
}

public enum S3EncryptionType
{
    [PropertyConverter("AES256")]
    S3,
    [PropertyConverter("aws:kms")]
    SseKms,
    [PropertyConverter("aws:kms:dsse")]
    DsseKms,
    [PropertyConverter("sse-c")]
    SseC
}

public class AwsStoreConfig : AbstractSlateDbConfig
{
    internal override ObjectStoreType StoreType => ObjectStoreType.S3;
    
    [JsonPropertyName("access_key_id")]
    [SlateDbConfiguration("access_key_id")]
    public string AccessKeyId { get; set; }
    [JsonPropertyName("secret_access_key")]
    [SlateDbConfiguration("secret_access_key")]
    public string SecretAccessKey { get; set; }
    [JsonPropertyName("region")]
    [SlateDbConfiguration("region")]
    public string Region { get; set; }
    [JsonPropertyName("default_region")]
    [SlateDbConfiguration("default_region")]
    public string DefaultRegion { get; set; }
    [JsonPropertyName("bucket_name")]
    [SlateDbConfiguration("bucket_name")]
    public string BucketName { get; set; }
    [JsonPropertyName("endpoint")]
    [SlateDbConfiguration("endpoint")]
    public string Endpoint { get; set; }
    [JsonPropertyName("aws_token")]
    [SlateDbConfiguration("aws_token")]
    public string Token { get; set; }
    [JsonPropertyName("imdsv1_fallback")]
    [SlateDbConfiguration("imdsv1_fallback")]
    public bool? Imdsv1Fallback { get; set; }
    [JsonPropertyName("aws_virtual_hosted_style_request")]
    [SlateDbConfiguration("aws_virtual_hosted_style_request")]
    public bool? VirtualHostedStyleRequest { get; set; }
    [JsonPropertyName("s3_express")]
    [SlateDbConfiguration("s3_express")]
    public bool? S3ExpressEnabled { get; set; }
    [JsonPropertyName("metadata_endpoint")]
    [SlateDbConfiguration("metadata_endpoint")]
    public string MetadataEndpoint { get; set; }
    [JsonPropertyName("unsigned_payload")]
    [SlateDbConfiguration("unsigned_payload")]
    public bool? UnsignedPayload { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("checksum_algorithm")]
    [SlateDbConfiguration("checksum_algorithm", typeof(EnumConverter))]
    public ChecksumAlgorithm? Checksum { get; set; }
    [JsonPropertyName("aws_container_credentials_relative_uri")]
    [SlateDbConfiguration("aws_container_credentials_relative_uri")]
    public string ContainerCredentialsRelativeUri { get; set; }
    [JsonPropertyName("aws_container_credentials_full_uri")]
    [SlateDbConfiguration("aws_container_credentials_full_uri")]
    public string ContainerCredentialsFullUri { get; set; }
    [JsonPropertyName("aws_container_authorization_token_file")]
    [SlateDbConfiguration("aws_container_authorization_token_file")]
    public string ContainerAuthorizationTokenFile { get; set; }
    [JsonPropertyName("web_identity_token_file")]
    [SlateDbConfiguration("web_identity_token_file")]
    public string WebIdentityTokenFile { get; set; }
    [JsonPropertyName("role_arn")]
    [SlateDbConfiguration("role_arn")]
    public string RoleARN { get; set; }
    [JsonPropertyName("role_session_name")]
    [SlateDbConfiguration("role_session_name")]
    public string RoleSessionName { get; set; }
    [JsonPropertyName("endpoint_url_sts")]
    [SlateDbConfiguration("endpoint_url_sts")]
    public string STSEndpoint { get; set; }
    [JsonPropertyName("aws_skip_signature")]
    [SlateDbConfiguration("aws_skip_signature")]
    public bool? SkipSignature { get; set; }
    [JsonPropertyName("aws_disable_tagging")]
    [SlateDbConfiguration("aws_disable_tagging")]
    public bool? DisableTagging { get; set; }
    [JsonPropertyName("aws_request_payer")]
    [SlateDbConfiguration("aws_request_payer")]
    public bool? RequestPayer { get; set; }
    [JsonPropertyName("aws_conditional_put")]
    [SlateDbConfiguration("aws_conditional_put", typeof(EnumConverter))]
    public S3ConditionalPut ConditionalPut => S3ConditionalPut.ETagMatch;
    /// <summary>
    /// <para>
    /// Configure how to provide [`ObjectStore::copy_if_not_exists`] for [`AmazonS3`].
    /// </para>
    /// <para>
    /// Some S3-compatible stores, such as Cloudflare R2, support copy if not exists
    /// semantics through custom headers.
    /// Encoded as `header:HEADER_NAME:HEADER_VALUE` ignoring whitespace
    /// </para>
    /// <para>
    /// The same as `Header` but allows custom status code checking, for object stores that return values
    /// other than 412.
    /// Encoded as `header-with-status:HEADER_NAME:HEADER_VALUE:STATUS` ignoring whitespace
    /// </para>
    /// <para>
    /// Native Amazon S3 supports copy if not exists through a multipart upload
    /// where the upload copies an existing object and is completed only if the
    /// new object does not already exist.
    /// Encoded as `multipart` ignoring whitespace.
    /// </para>
    /// <para>
    /// The name of a DynamoDB table to use for coordination.
    /// Encoded as either `dynamo:TABLE_NAME` or `dynamo:TABLE_NAME:TIMEOUT_MILLIS`
    /// </para>
    /// </summary>
    [JsonPropertyName("aws_copy_if_not_exists")]
    [SlateDbConfiguration("aws_copy_if_not_exists")]
    public string? CopyIfNotExists { get; set; }
    [JsonPropertyName("aws_allow_http")]
    [SlateDbConfiguration("aws_allow_http")]
    public bool? AllowHttp { get; set; }
    [JsonPropertyName("aws_allow_invalid_certificates")]
    [SlateDbConfiguration("aws_allow_invalid_certificates")]
    public bool? AllowInvalidCertificates { get; set; }
    [JsonPropertyName("aws_connect_timeout")]
    [SlateDbConfiguration("aws_connect_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? ConnectTimeout { get; set; }
    [JsonPropertyName("aws_default_content_type")]
    [SlateDbConfiguration("aws_default_content_type")]
    public string DefaultContentType { get; set; }
    [JsonPropertyName("aws_http1_only")]
    [SlateDbConfiguration("aws_http1_only")]
    public bool? HttpOnly1 { get; set; }
    [JsonPropertyName("aws_http2_only")]
    [SlateDbConfiguration("aws_http2_only")]
    public bool? HttpOnly2 { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_http2_keep_alive_interval")]
    [SlateDbConfiguration("aws_http2_keep_alive_interval", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveInterval { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_http2_keep_alive_timeout")]
    [SlateDbConfiguration("aws_http2_keep_alive_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveTimeout { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_http2_keep_alive_while_idle")]
    [SlateDbConfiguration("aws_http2_keep_alive_while_idle", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveWhileIdle { get; set; }
    [JsonPropertyName("aws_http2_max_frame_size")]
    [SlateDbConfiguration("aws_http2_max_frame_size")]
    public Int32? Http2MaxFrameSize { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_pool_idle_timeout")]
    [SlateDbConfiguration("aws_pool_idle_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? PoolIdleTimeout { get; set; }
    [JsonPropertyName("aws_pool_max_idle_per_host")]
    [SlateDbConfiguration("aws_pool_max_idle_per_host")]
    public Int16? PoolMaxIdlePerHost { get; set; }
    [JsonPropertyName("aws_proxy_url")]
    [SlateDbConfiguration("aws_proxy_url")]
    public string ProxyUrl  { get; set; }
    [JsonPropertyName("aws_proxy_ca_certificate")]
    [SlateDbConfiguration("aws_proxy_ca_certificate")]
    public string ProxyCaCertificate  { get; set; }
    [JsonPropertyName("aws_proxy_excludes")]
    [SlateDbConfiguration("aws_proxy_excludes")]
    public string ProxyExcludes { get; set; }
    [JsonPropertyName("aws_randomize_addresses")]
    [SlateDbConfiguration("aws_randomize_addresses")]
    public bool? RandomizeAddresses  { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_timeout")]
    [SlateDbConfiguration("aws_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Timeout { get; set; }
    [JsonPropertyName("aws_user_agent")]
    [SlateDbConfiguration("aws_user_agent")]
    public String UserAgent { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("aws_server_side_encryption")]
    [SlateDbConfiguration("aws_server_side_encryption", typeof(EnumConverter))]
    public S3EncryptionType? EncryptionType { get; set; }
    [JsonPropertyName("aws_sse_kms_key_id")]
    [SlateDbConfiguration("aws_sse_kms_key_id")]
    public string EncryptionKmsKeyId { get; set; }
    [JsonPropertyName("aws_sse_bucket_key_enabled")]
    [SlateDbConfiguration("aws_sse_bucket_key_enabled")]
    public bool? EncryptionBucketKeyEnabled { get; set; }
    [JsonPropertyName("aws_sse_customer_key_base64")]
    [SlateDbConfiguration("aws_sse_customer_key_base64")]
    public string EncryptionCustomerKeyBase64 { get; set; }
}