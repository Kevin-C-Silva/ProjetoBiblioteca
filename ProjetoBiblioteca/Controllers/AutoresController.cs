using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Autenticacao;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    [SessionAuthorize]
    public class AutoresController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            List<Autores> autores = new List<Autores>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct nome, criado_em from Autores order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    autores.Add(new Autores
                    {
                        Nome = reader.GetString("nome"),
                        CriadoEm = reader.GetDateTime("criado_em")
                    });
                }
            }
            return View(autores);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Autores vm)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_autor_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Criar");
        }


        public IActionResult Editar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Editar(Autores vm)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_autor_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Criar");
        }
    }
}
