using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;
using BCrypt.Net;

namespace ProjetoBiblioteca.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            List<Usuarios> usuarios = new List<Usuarios>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct nome, email, role, ativo, criado_Em from Usuarios order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new Usuarios
                    {
                        Nome = reader.GetString("nome"),
                        Email = reader.GetString("email"),
                        Role = reader.GetString("role"),
                        Ativo = reader.GetInt32("ativo"),
                        CriadoEm = reader.GetDateTime("criado_Em")
                    });
                }
            }
            return View(usuarios);
        }

        public IActionResult CriarUsuario()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CriarUsuario(Usuarios vm)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_usuario_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(vm.Senha, workFactor: 12);
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.Parameters.AddWithValue("p_email", vm.Email);
            cmd.Parameters.AddWithValue("p_senha_hash", senhaHash);
            cmd.Parameters.AddWithValue("p_role", vm.Role);
            cmd.ExecuteNonQuery();

            return RedirectToAction("CriarUsuario");
        }
    }
}
