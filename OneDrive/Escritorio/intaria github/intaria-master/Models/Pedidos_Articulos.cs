using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class Pedidos_Articulos
    {
        public Guid Id { get; set; }
        public Guid IdPedido{get;set;}
        public Guid IdArticulo { get; set; }
        public int PrecioVenta { get; set; }
        public string Comentario { get; set; }

    }
}
