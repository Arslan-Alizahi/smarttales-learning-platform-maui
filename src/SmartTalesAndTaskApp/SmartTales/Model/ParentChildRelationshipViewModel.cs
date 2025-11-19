namespace SmartTales.Model
{
    public class ParentChildRelationshipViewModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public string ChildEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
