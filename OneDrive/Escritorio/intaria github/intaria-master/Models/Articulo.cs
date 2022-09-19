using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class Articulo
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Guid IdCategoria { get; set; }
        public int Precio { get; set; }
        public int Cantidad { get; set; }
        public string Estado { get; set; }
        public string NombreArchivo { get; set; }
        public Guid IdCatEbay { get; set; }

    }
}
