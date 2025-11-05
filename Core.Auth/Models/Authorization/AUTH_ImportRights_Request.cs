namespace Core.Auth.Models.Authorization
{
    public class AUTH_ImportRights_Request
    {
        public bool IsMatchRightsBy_GID { get; set; }
        public bool IsMatchRightsBy_Code { get; set; }
        public List<AUTH_ImportRights_RightGroup> RightGroups { get; set; } = null!;
    }

    public class AUTH_ImportRights_RightGroup
    {
        public string RightGroup_Name { get; set; } = null!;

        public AUTH_ImportRights_RightGroup? Parent { get; set; }

        public List<AUTH_ImportRights_Right> Rights { get; set; } = null!;
    }

    public class AUTH_ImportRights_Right
    {
        public string Right_Name { get; set; } = null!;
        public string Right_Code { get; set; } = null!;
        public Guid Right_GID { get; set; }
    }
}