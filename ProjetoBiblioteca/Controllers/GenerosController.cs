using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class GenerosController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            List<Generos> generos = new List<Generos>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct nome, criado_em from Generos order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    generos.Add(new Generos
                    {
                        Nome = reader.GetString("nome"),
                        CriadoEm = reader.GetDateTime("criado_em")
                    });
                }
            }
            return View(generos);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Generos vm)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_genero_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Criar");
        }
    }
}
