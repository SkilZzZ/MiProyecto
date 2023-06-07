using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class Pedidos
    {
        public  Guid Id { get; set; }
        public Guid IdUsuario  { get; set; }
        public string NombreUsuario { get; set; }
        public int Total  { get; set; }
        public string Estado { get; set; }
        public string FormaPago { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string CodigoEnvio { get; set; }
        public string NombreArchivo { get; set; }
        public string CreadoPorMi { get; set; }


    }
}
