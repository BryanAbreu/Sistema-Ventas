using Microsoft.AspNetCore.Mvc;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class PlantillaController : Controller
    {
        public IActionResult EnviarClave(string Correo, string Clave)
        {
            ViewData["Correo"] = Correo;
            ViewData["Clave"] = Clave;
            ViewData["Url"] = $"{this.Request.Scheme}://{this.Request.Host}";
            return View();
        }

        public IActionResult RestablecerCLave(string Clave)
        {
            ViewData["Clave"] = Clave;
            return View();
        }

    }
}
