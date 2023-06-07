using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class Fotos
    {
        public Guid Id { get; set; }
        public Guid IdArticulo { get; set; }
        public String Descripcion { get; set; }
        public String NombreArchivo { get; set; }
    }
}
