using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Planet.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        [Display(Name = "Middle Name")]
        public string middle_Name { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Birthday")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime birth_day { get; set; }
        public byte[] Avatar { get; set; }
        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public Role Role { get; set; }
    }

    public enum SortState
    {
        IdAsc,
        IdDesc,
        EmailAsc, 
        EmailDesc,
        LoginAsc,
        LoginDesc
    }
}
