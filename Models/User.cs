using System;
using System.Collections.Generic;
using System.Text;

namespace blockchain.Models
{
    public class User
    {
        public long id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public float RemainingMoney { get; set; }
        public string Role { get; set; }
    }
}
