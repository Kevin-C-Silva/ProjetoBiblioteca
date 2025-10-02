using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;
using BCrypt.Net;
using ProjetoBiblioteca.Autenticacao;
using System.Data;

namespace ProjetoBiblioteca.Controllers
{
    [SessionAuthorize(RoleAnyOf = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            List<Usuarios> usuarios = new List<Usuarios>();
            using (var conn = db.GetConnection())
            {
                using var cmd = new MySqlCommand("sp_usuario_listar", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new Usuarios
                    {
                        Nome = reader.GetString("nome"),
                        Email = reader.GetString("email"),
                        Imagem = reader["Imagem"] == DBNull.Value ? null : (string?)reader.GetString("imagem"),
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

            cmd.CommandType = CommandType.StoredProcedure;
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(vm.Senha, workFactor: 12);
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.Parameters.AddWithValue("p_email", vm.Email);
            cmd.Parameters.AddWithValue("p_senha_hash", senhaHash);
            cmd.Parameters.AddWithValue("p_role", vm.Role);
            cmd.ExecuteNonQuery();
            return RedirectToAction("CriarUsuario");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var conn = db.GetConnection();
            Usuarios? usuarios = null;

            using (var cmd = new MySqlCommand("sp_usuario_obter", conn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("p_id", id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    usuarios = new Usuarios
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),
                        Email = rd["email"] == DBNull.Value ? null : (string?)rd.GetString("email"),
                        Senha = rd["senha_hash"] == DBNull.Value ? null : (string?)rd.GetString("senha_hash"),
                        Imagem = rd["imagem"] == DBNull.Value ? null : (string?)rd.GetString("imagem"),
                        Role = rd["role"] == DBNull.Value ? null : (string?)rd.GetString("role"),
                        Ativo = rd.GetInt32("ativo"),
                        CriadoEm = rd.GetDateTime("ativo"), // Capa_Arquivo = rd["capa_arquivo"] == DBNull.Value ? null : (string?)rd.GetString("capa_arquivo"),
                    };
                }
            }

            if (usuarios == null) return NotFound();

            return View(usuarios);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Usuarios vm)
        {
            string? relPath = null;

            if (vm.Id <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(vm.Nome))
            {
                ModelState.AddModelError("", "Informe nome.");
            }

            using var conn2 = db.GetConnection();
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(vm.Senha, workFactor: 12);
            using var cmd = new MySqlCommand("sp_usuario_atualizar", conn2) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id", vm.Id);
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.Parameters.AddWithValue("p_email", vm.Email);
            cmd.Parameters.AddWithValue("p_imagem", vm.Imagem);
            cmd.Parameters.AddWithValue("p_senha_hash", senhaHash);
            cmd.Parameters.AddWithValue("p_role", vm.Role);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }
    }
}
