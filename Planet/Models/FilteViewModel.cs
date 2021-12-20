using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Planet.Models
{
    public class FilteViewModel
    {
        public int? SelectId { get; private set; }
        public string SelectEmail { get; private set; }
        public string SelectLogin { get; private set; }
        public FilteViewModel(int? id, string email, string login)
        {
            SelectId = id;
            SelectLogin = login;
            SelectEmail = email;
        }
    }
}
