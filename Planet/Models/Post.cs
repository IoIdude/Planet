using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Planet.Models
{
    public class Post
    {
        [Key]
        public int id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public byte[] Picture { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}
