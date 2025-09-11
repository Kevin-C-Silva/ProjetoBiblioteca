using System.Security.Cryptography.X509Certificates;

namespace ProjetoBiblioteca.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Role { get; set; }
        public int Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
