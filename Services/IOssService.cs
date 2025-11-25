namespace LibraryManagement.Services
{
    /// <summary>
    /// 文件上传
    /// </summary>
    public interface IOssService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "covers");
    }
}
