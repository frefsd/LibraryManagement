namespace LibraryManagement.Exceptions
{
    /// <summary>
    /// 业务异常处理类
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
