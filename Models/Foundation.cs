using System;
using System.Collections.Generic;
using System.Text;

namespace blockchain.Models
{
    public class Foundation
    {
        public long id { get; set; }
        public string NameEn { get; set; }
        public DateTime? Deadline { get; set; }
        public float DonateGoal { get; set; }
        public float TotalUnDonate { get; set; }
        public float TotalDonate { get; set; }
    }
}