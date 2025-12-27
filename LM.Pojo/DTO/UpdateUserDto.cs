namespace LibraryManagement.LM.Pojo.DTO
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }     
        public string? Phone { get; set; }    
        public string? Email { get; set; }       
        public string? CardNumber { get; set; }
        public int Status { get; set; } // 1.正常 0.禁用
    }
}
