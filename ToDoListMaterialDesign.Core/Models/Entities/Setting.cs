using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListMaterialDesign.Models.Entities
{
    [Serializable]
    [Table("m_Setting")]
    public class Setting
    {
        [Key]
        [Required]
        public string SettingId { set; get; }

        public string Value { set; get; }

        [Required]
        public DateTime CreateDateTime { set; get; }

        [Required]
        public DateTime UpdateDateTime { set; get; }

        public struct SettingIdKeys
        {
            public const string Dummy = "Dummy";
        }
    }
}
