using SQLite;
using System;

namespace SmartTales.Model
{
    [Table("ParentChild")]
    public class ParentChildModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ParentId")]
        public int ParentId { get; set; }

        [Column("ChildId")]
        public int ChildId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;
    }
}
