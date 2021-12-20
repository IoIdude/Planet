using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Planet.Models
{
    public class IndexViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Post> Posts { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilteViewModel FilteViewModel { get; set; }
        public SortViewModel SortViewModel { get; set; }
        public FilteForProfile FilteForProfile { get; set; }
        public int ID { get; set; }
        public int IdentityUserLog { get; set; }
    }
}
