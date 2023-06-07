using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class Usuarios
    {

        public Guid  UserId{ get;set;}
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string clave { get; set; }
        public string CreadoPorMi { get; set; }
        public int Telefono { get; set; }
        public string Direccion1 { get; set; }
        public string Direccion2 { get; set; }
        public string Localidad { get; set; }
        public string Provincia { get; set; }
        public int CodigoPostal { get; set; }
        public string Pais { get; set; }

    }
}
