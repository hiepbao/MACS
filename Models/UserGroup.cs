namespace MACS.Models
{
    public class UserGroup
    {
        public int? GroupId { get; set; }             
        public string GroupName { get; set; }     
        public string Description { get; set; }    
        public List<UserAccount> Members { get; set; } = new(); 
    }
}
