namespace LibraryManagement.LM.Service.Services
{
    /// <summary>
    /// OSS文件上传
    /// </summary>
    public interface IOssService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "covers");
    }
}
