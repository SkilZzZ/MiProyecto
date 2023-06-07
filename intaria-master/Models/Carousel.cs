using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class Carousel
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Enlace { get; set; }
    }
}
