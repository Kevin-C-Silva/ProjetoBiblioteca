using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class LivrosController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var lista = new List<Livros>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Livros
                {
                    Id = rd.GetInt32("id"),
                    Titulo = rd.GetString("titulo"),
                    AutorId = rd["autor"] == DBNull.Value ? null : (int?)rd.GetInt32("autor"),
                    EditoraId = rd["editora"] == DBNull.Value ? null : (int?)rd.GetInt32("editora"),
                    GeneroId = rd["genero"] == DBNull.Value ? null : (int?)rd.GetInt32("genero"),
                    Autor = rd["autor_nome"] as string,
                    Editora = rd["editora_nome"] as string,
                    Genero = rd["genero_nome"] as string,
                    Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt16("ano"),
                    Isbn = rd["isbn"] as string,
                    QuantidadeTotal = rd.GetInt32("quantidade_total"),
                    QuantidadeDisponivel = rd.GetInt32("quantidade_disponivel"),
                    CriadoEm = rd.GetDateTime("criado_em")
                });
            }
            return View(lista);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            using var conn = db.GetConnection();
            ViewBag.Autores = CarregarAutores(conn);
            ViewBag.Editoras = CarregarEditoras(conn);
            ViewBag.Generos = CarregarGeneros(conn);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Criar(Livros vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Titulo) || vm.QuantidadeTotal < 1)
            {
                ModelState.AddModelError("", "Informe titulo e uma quantidade total válida (>-1).");
            }

            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_criar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_titulo", vm.Titulo);
            cmd.Parameters.AddWithValue("p_autor", (object?)vm.AutorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_editora", (object?)vm.EditoraId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_genero", (object?)vm.GeneroId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_ano", (object?)vm.Ano ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_isbn", (object?)vm.Isbn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_quantidade", vm.QuantidadeTotal);
            cmd.ExecuteNonQuery();

            TempData["ok"] = "Livro cadastrado!";
            return RedirectToAction(nameof(Index));
        }

        private readonly Database db = new Database();

        // Helpers para carregar os selects via SP (Stored Procedure)
        private List<SelectListItem> CarregarAutores(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_autor_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            return list;
        }
        private List<SelectListItem> CarregarEditoras(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_editora_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            return list;
        }
        private List<SelectListItem> CarregarGeneros(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_genero_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            return list;
        }

    }
}
