namespace backend.Configurations
{
    public class MinIOSettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool Secure { get; set; } = false;
        public string BucketName { get; set; } = "hrm-documents";
        public string Region { get; set; } = "us-east-1";
    }
}
