using System;
using System.Collections.Generic;
using System.Text;

namespace blockchain.Models
{
    public class DonateViewModel
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public int Amount { get; set; }
    }
}
