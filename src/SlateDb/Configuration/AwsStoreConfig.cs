using System.Text.Json.Serialization;
using SlateDb.Configuration.Converter;

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

/// <summary>
/// Checksum algorithm used to validate S3 requests.
/// </summary>
public enum ChecksumAlgorithm
{
    /// <summary>SHA-256 checksum.</summary>
    [PropertyConverter("sha256")]
    Sha256
}

/// <summary>
/// Server-side encryption scheme applied to objects written to S3.
/// </summary>
public enum S3EncryptionType
{
    /// <summary>SSE-S3: server-side encryption with Amazon S3-managed keys.</summary>
    [PropertyConverter("AES256")]
    S3,

    /// <summary>SSE-KMS: server-side encryption with AWS KMS-managed keys.</summary>
    [PropertyConverter("aws:kms")]
    SseKms,

    /// <summary>DSSE-KMS: dual-layer server-side encryption with AWS KMS-managed keys.</summary>
    [PropertyConverter("aws:kms:dsse")]
    DsseKms,

    /// <summary>SSE-C: server-side encryption with a customer-supplied key.</summary>
    [PropertyConverter("sse-c")]
    SseC
}

/// <summary>
/// Configuration for an object store backed by Amazon S3, or an S3-compatible store.
/// </summary>
public class AwsStoreConfig : AbstractSlateDbConfig
{
    /// <summary>AWS access key ID.</summary>
    [JsonPropertyName("access_key_id")]
    [SlateDbConfiguration("access_key_id")]
    public string AccessKeyId { get; set; }

    /// <summary>AWS secret access key.</summary>
    [JsonPropertyName("secret_access_key")]
    [SlateDbConfiguration("secret_access_key")]
    public string SecretAccessKey { get; set; }

    /// <summary>AWS region the bucket lives in.</summary>
    [JsonPropertyName("region")]
    [SlateDbConfiguration("region")]
    public string Region { get; set; }

    /// <summary>Default AWS region used when <see cref="Region"/> is not set.</summary>
    [JsonPropertyName("default_region")]
    [SlateDbConfiguration("default_region")]
    public string DefaultRegion { get; set; }

    /// <summary>Name of the S3 bucket.</summary>
    [JsonPropertyName("bucket_name")]
    [SlateDbConfiguration("bucket_name")]
    public string BucketName { get; set; }

    /// <summary>Custom S3 endpoint URL, for S3-compatible stores.</summary>
    [JsonPropertyName("endpoint")]
    [SlateDbConfiguration("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>AWS session token, for temporary credentials.</summary>
    [JsonPropertyName("aws_token")]
    [SlateDbConfiguration("aws_token")]
    public string Token { get; set; }

    /// <summary>Whether to fall back to IMDSv1 when IMDSv2 is unavailable.</summary>
    [JsonPropertyName("imdsv1_fallback")]
    [SlateDbConfiguration("imdsv1_fallback")]
    public bool? Imdsv1Fallback { get; set; }

    /// <summary>Whether to use virtual-hosted-style requests instead of path-style.</summary>
    [JsonPropertyName("aws_virtual_hosted_style_request")]
    [SlateDbConfiguration("aws_virtual_hosted_style_request")]
    public bool? VirtualHostedStyleRequest { get; set; }

    /// <summary>Whether the bucket is an S3 Express One Zone bucket.</summary>
    [JsonPropertyName("s3_express")]
    [SlateDbConfiguration("s3_express")]
    public bool? S3ExpressEnabled { get; set; }

    /// <summary>Custom EC2 instance metadata service endpoint.</summary>
    [JsonPropertyName("metadata_endpoint")]
    [SlateDbConfiguration("metadata_endpoint")]
    public string MetadataEndpoint { get; set; }

    /// <summary>Whether to skip signing the request payload (used with some S3-compatible stores).</summary>
    [JsonPropertyName("unsigned_payload")]
    [SlateDbConfiguration("unsigned_payload")]
    public bool? UnsignedPayload { get; set; }

    /// <summary>Checksum algorithm used to validate requests.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("checksum_algorithm")]
    [SlateDbConfiguration("checksum_algorithm", typeof(EnumConverter))]
    public ChecksumAlgorithm? Checksum { get; set; }

    /// <summary>Relative URI for ECS container credentials.</summary>
    [JsonPropertyName("aws_container_credentials_relative_uri")]
    [SlateDbConfiguration("aws_container_credentials_relative_uri")]
    public string ContainerCredentialsRelativeUri { get; set; }

    /// <summary>Full URI for ECS/EKS container credentials.</summary>
    [JsonPropertyName("aws_container_credentials_full_uri")]
    [SlateDbConfiguration("aws_container_credentials_full_uri")]
    public string ContainerCredentialsFullUri { get; set; }

    /// <summary>Path to a file containing the container credentials authorization token.</summary>
    [JsonPropertyName("aws_container_authorization_token_file")]
    [SlateDbConfiguration("aws_container_authorization_token_file")]
    public string ContainerAuthorizationTokenFile { get; set; }

    /// <summary>Path to a file containing a web identity token, for AssumeRoleWithWebIdentity.</summary>
    [JsonPropertyName("web_identity_token_file")]
    [SlateDbConfiguration("web_identity_token_file")]
    public string WebIdentityTokenFile { get; set; }

    /// <summary>ARN of the role to assume when using a web identity token.</summary>
    [JsonPropertyName("role_arn")]
    [SlateDbConfiguration("role_arn")]
    public string RoleARN { get; set; }

    /// <summary>Session name used when assuming a role via a web identity token.</summary>
    [JsonPropertyName("role_session_name")]
    [SlateDbConfiguration("role_session_name")]
    public string RoleSessionName { get; set; }

    /// <summary>Custom STS endpoint used for web identity token exchange.</summary>
    [JsonPropertyName("endpoint_url_sts")]
    [SlateDbConfiguration("endpoint_url_sts")]
    public string STSEndpoint { get; set; }

    /// <summary>Whether to skip request signing entirely (for public buckets).</summary>
    [JsonPropertyName("aws_skip_signature")]
    [SlateDbConfiguration("aws_skip_signature")]
    public bool? SkipSignature { get; set; }

    /// <summary>Whether to disable tagging requests (some S3-compatible stores reject them).</summary>
    [JsonPropertyName("aws_disable_tagging")]
    [SlateDbConfiguration("aws_disable_tagging")]
    public bool? DisableTagging { get; set; }

    /// <summary>Whether to permit operations on requester-pays buckets.</summary>
    [JsonPropertyName("aws_request_payer")]
    [SlateDbConfiguration("aws_request_payer")]
    public bool? RequestPayer { get; set; }

    /// <summary>How conditional puts (compare-and-swap) are performed against S3.</summary>
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

    /// <summary>Whether to allow plain HTTP connections without TLS.</summary>
    [JsonPropertyName("aws_allow_http")]
    [SlateDbConfiguration("aws_allow_http")]
    public bool? AllowHttp { get; set; }

    /// <summary>Whether to accept invalid (e.g. self-signed) TLS certificates.</summary>
    [JsonPropertyName("aws_allow_invalid_certificates")]
    [SlateDbConfiguration("aws_allow_invalid_certificates")]
    public bool? AllowInvalidCertificates { get; set; }

    /// <summary>Timeout for establishing a connection.</summary>
    [JsonPropertyName("aws_connect_timeout")]
    [SlateDbConfiguration("aws_connect_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? ConnectTimeout { get; set; }

    /// <summary>Default content type applied to uploaded objects when none is specified.</summary>
    [JsonPropertyName("aws_default_content_type")]
    [SlateDbConfiguration("aws_default_content_type")]
    public string DefaultContentType { get; set; }

    /// <summary>Whether to restrict requests to HTTP/1.1 only.</summary>
    [JsonPropertyName("aws_http1_only")]
    [SlateDbConfiguration("aws_http1_only")]
    public bool? HttpOnly1 { get; set; }

    /// <summary>Whether to restrict requests to HTTP/2 only.</summary>
    [JsonPropertyName("aws_http2_only")]
    [SlateDbConfiguration("aws_http2_only")]
    public bool? HttpOnly2 { get; set; }

    /// <summary>Interval between HTTP/2 keep-alive pings.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_http2_keep_alive_interval")]
    [SlateDbConfiguration("aws_http2_keep_alive_interval", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveInterval { get; set; }

    /// <summary>Timeout waiting for an HTTP/2 keep-alive ping response.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_http2_keep_alive_timeout")]
    [SlateDbConfiguration("aws_http2_keep_alive_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveTimeout { get; set; }

    /// <summary>Whether HTTP/2 keep-alive pings are sent even while the connection is idle.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_http2_keep_alive_while_idle")]
    [SlateDbConfiguration("aws_http2_keep_alive_while_idle", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveWhileIdle { get; set; }

    /// <summary>Maximum HTTP/2 frame size accepted from the server.</summary>
    [JsonPropertyName("aws_http2_max_frame_size")]
    [SlateDbConfiguration("aws_http2_max_frame_size")]
    public Int32? Http2MaxFrameSize { get; set; }

    /// <summary>How long an idle pooled connection is kept before being closed.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_pool_idle_timeout")]
    [SlateDbConfiguration("aws_pool_idle_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? PoolIdleTimeout { get; set; }

    /// <summary>Maximum number of idle connections kept per host in the connection pool.</summary>
    [JsonPropertyName("aws_pool_max_idle_per_host")]
    [SlateDbConfiguration("aws_pool_max_idle_per_host")]
    public Int16? PoolMaxIdlePerHost { get; set; }

    /// <summary>URL of an HTTP proxy to route requests through.</summary>
    [JsonPropertyName("aws_proxy_url")]
    [SlateDbConfiguration("aws_proxy_url")]
    public string ProxyUrl  { get; set; }

    /// <summary>CA certificate used to validate the proxy's TLS certificate.</summary>
    [JsonPropertyName("aws_proxy_ca_certificate")]
    [SlateDbConfiguration("aws_proxy_ca_certificate")]
    public string ProxyCaCertificate  { get; set; }

    /// <summary>Comma-separated list of hosts excluded from proxying.</summary>
    [JsonPropertyName("aws_proxy_excludes")]
    [SlateDbConfiguration("aws_proxy_excludes")]
    public string ProxyExcludes { get; set; }

    /// <summary>Whether to randomize the order of resolved addresses when connecting.</summary>
    [JsonPropertyName("aws_randomize_addresses")]
    [SlateDbConfiguration("aws_randomize_addresses")]
    public bool? RandomizeAddresses  { get; set; }

    /// <summary>Overall timeout for a request.</summary>
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("aws_timeout")]
    [SlateDbConfiguration("aws_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Timeout { get; set; }

    /// <summary>User agent string sent with requests.</summary>
    [JsonPropertyName("aws_user_agent")]
    [SlateDbConfiguration("aws_user_agent")]
    public String UserAgent { get; set; }

    /// <summary>Server-side encryption scheme applied to uploaded objects.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("aws_server_side_encryption")]
    [SlateDbConfiguration("aws_server_side_encryption", typeof(EnumConverter))]
    public S3EncryptionType? EncryptionType { get; set; }

    /// <summary>KMS key ID used when <see cref="EncryptionType"/> is SSE-KMS or DSSE-KMS.</summary>
    [JsonPropertyName("aws_sse_kms_key_id")]
    [SlateDbConfiguration("aws_sse_kms_key_id")]
    public string EncryptionKmsKeyId { get; set; }

    /// <summary>Whether S3 bucket keys are used to reduce KMS request volume.</summary>
    [JsonPropertyName("aws_sse_bucket_key_enabled")]
    [SlateDbConfiguration("aws_sse_bucket_key_enabled")]
    public bool? EncryptionBucketKeyEnabled { get; set; }

    /// <summary>Base64-encoded customer-supplied encryption key, when <see cref="EncryptionType"/> is SSE-C.</summary>
    [JsonPropertyName("aws_sse_customer_key_base64")]
    [SlateDbConfiguration("aws_sse_customer_key_base64")]
    public string EncryptionCustomerKeyBase64 { get; set; }
}
