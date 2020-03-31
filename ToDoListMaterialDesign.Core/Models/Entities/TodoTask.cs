using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListMaterialDesign.Models.Entities
{
    [Serializable]
    [Table("t_TodoTask")]
    public class TodoTask
    {
        [Key]
        [Required]
        public string TodoTaskId { set; get; }

        public string Subject { set; get; }

        public DateTime? DueDate { set; get; }

        public string StatusCode { set; get; }

        [Required]
        public DateTime CreateDateTime { set; get; }

        [Required]
        public DateTime UpdateDateTime { set; get; }
    }
}
