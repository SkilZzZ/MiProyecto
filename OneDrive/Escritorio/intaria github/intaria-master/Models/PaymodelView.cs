using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class PaymodelView
    {
        public string Token { get; set; }
        public double Total { get; set; }
        public string Email { get; set; }
        public string TipoPago { get; set; }
    }
}
