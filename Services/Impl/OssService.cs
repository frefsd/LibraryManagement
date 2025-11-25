using Aliyun.OSS;

namespace LibraryManagement.Services.Impl
{
    /// <summary>
    /// 文件上传
    /// </summary>
    public class OssService : IOssService
    {
        private readonly string _accessKeyId;
        private readonly string _accessKeySecret;
        private readonly string _endpoint;
        private readonly string _bucketName;

        //依赖注入
        public OssService(IConfiguration configuration)
        {
            var ossSection = configuration.GetSection("AliyunOss");
            _accessKeyId = ossSection["AccessKeyId"]!;
            _accessKeySecret = ossSection["AccessKeySecret"]!;
            _endpoint = ossSection["Endpoint"]!;
            _bucketName = ossSection["BucketName"]!;
        }
        public async Task<string> UploadFileAsync(IFormFile file, string folder = "covers")
        {
            //验证文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif"};
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("仅支持JPG、PNG、GIF格式的图片");

            //生成唯一文件名
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var objectName = $"{folder}/{fileName}";

            //读取文件到内存流
            await using var originalStream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await originalStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            //防止线程阻塞
            await Task.Run(() =>
            {
                var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
                client.PutObject(_bucketName, objectName, memoryStream);
            });
           
            //返回可公开访问的URL
            return $"https://{_bucketName}.{_endpoint}/{objectName}";
        }
    }
}
