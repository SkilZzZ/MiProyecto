using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class CategoriasEbay
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public Guid CategoriaPadre { get; set; }
    }
}
