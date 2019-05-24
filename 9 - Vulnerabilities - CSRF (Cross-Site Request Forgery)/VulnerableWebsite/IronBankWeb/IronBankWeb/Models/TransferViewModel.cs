using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IronBankWeb.Models
{
    public class TransferViewModel
    {
        public string DestinationAccount { get; set; }
        public int? Amount { get; set; }
    }
}
