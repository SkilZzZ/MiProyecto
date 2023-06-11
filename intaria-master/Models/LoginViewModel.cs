using System.ComponentModel.DataAnnotations;

namespace Intaria.Models
{
    public class LoginViewModel
    {
        public string Message { get; set; }

        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "El campo Email debe tener un formato de correo electrónico válido.")]
        public string Email { get; set; }

        public string Token { get; set; }
        public int Phone { get; set; }

    }
}
