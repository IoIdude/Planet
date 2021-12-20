using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Planet.Models
{
    public class FilteForProfile
    {
        public string SelectLogin { get; private set; }

        public FilteForProfile(string login)
        {
            SelectLogin = login;
        }
    }
}
