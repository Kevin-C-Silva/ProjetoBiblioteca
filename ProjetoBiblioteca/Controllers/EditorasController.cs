using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class EditorasController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            List<Editoras> editoras = new List<Editoras>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct nome, criado_Em from Editoras order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    editoras.Add(new Editoras
                    {
                        Nome = rd.GetString("nome"),
                        CriadoEm = rd.GetDateTime("criado_em")
                    });
                }
            }
            return View(editoras);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Editoras vm)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_editora_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Criar");
        }
    }
}
