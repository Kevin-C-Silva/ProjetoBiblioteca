using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Autenticacao;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class EmprestimoController : Controller
    {
        private readonly Database db = new Database();

        [HttpGet]
        public IActionResult Vitrine(string? q)
        {
            var itens = new List<Livros>();
            var titulos = new List<string>();

            using var conn = db.GetConnection();

            using (var cmd = new MySqlCommand("sp_vitrine_buscar", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("p_q", q ?? "");
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    itens.Add(new Livros
                    {
                        Id = rd.GetInt32("id"),
                        Titulo = rd.GetString("titulo"),
                        Capa_Arquivo = rd["capa_arquivo"] as string
                    });
                }
            }
            using (var cmdAll = new MySqlCommand("sp_vitrine_buscar", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmdAll.Parameters.AddWithValue("p_q", "");
                using var rd2 = cmdAll.ExecuteReader();
                while (rd2.Read())
                {
                    var titulo = rd2.GetString("titulo");
                    if (!string.IsNullOrWhiteSpace(titulo) && !titulos.Contains(titulo))
                        titulos.Add(titulo);
                }
            }
            ViewBag.q = q ?? "";
            ViewBag.Titulos = titulos;
            return View(itens);
        }

    }
}
