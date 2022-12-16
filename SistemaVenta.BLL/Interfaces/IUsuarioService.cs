using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> Lista();
        Task<Usuario> Crear(Usuario entidad, Stream Foto = null,string NombreFoto = "",string UrlPlantillaCorreo="" );
        Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "");

        Task<bool> Eliminar(int IdUsuario);
        Task<Usuario> ObtenerPorCrendencial(string Correo, string Clave);

        Task<Usuario> obtenerPorId(int IdUsuario);
        Task<bool> GuardarPerfil(Usuario Entidad);
        Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string CLaveNueva);

        Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo);

    }
}
