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
                var sql = "select distinct nome, email, role, ativo, criado_Em from Usuarios order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new Usuarios
                    {
                        Nome = reader.GetString("nome"),
                        Email = reader.GetString("email"),
                        Imagem = reader.GetString("imagem"),
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
                         = rd["generoId"] == DBNull.Value ? null : (int?)rd.GetInt32("generoId"),
                        Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt16("ano"),
                        Isbn = rd["isbn"] as string,
                        Capa_Arquivo = rd["capa_arquivo"] as string, // Capa_Arquivo = rd["capa_arquivo"] == DBNull.Value ? null : (string?)rd.GetString("capa_arquivo"),
                        QuantidadeTotal = rd.GetInt32("quantidade_total")
                    };
                }
            }

            if (livro == null) return NotFound();

            return View(livro);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Livros model)
        {
            string? relPath = null;

            if (model.Id <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(model.Titulo) || model.QuantidadeTotal < 1)
            {
                ModelState.AddModelError("", "Informe título e quantidade total (>=1).");
            }

            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_atualizar", conn2) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id", model.Id);
            cmd.Parameters.AddWithValue("p_titulo", model.Titulo);
            cmd.Parameters.AddWithValue("p_autor", model.AutorId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_editora", model.EditoraId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_genero", model.GeneroId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_ano", model.Ano ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_isbn", (object?)model.Isbn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_capa_arquivo", (object?)relPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_novo_total", model.QuantidadeTotal);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }
    }
}
