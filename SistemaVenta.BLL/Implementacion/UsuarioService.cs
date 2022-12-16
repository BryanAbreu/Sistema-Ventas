using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repository;
        private readonly IFarebaseServices _FireBaseService;
        private readonly IUtilidadesServices _utilidadesServices;
        private readonly ICorreoService _correoService;

        public UsuarioService(
            IGenericRepository<Usuario> repository,
            IFarebaseServices fireBaseService, 
            IUtilidadesServices utilidadesServices,
            ICorreoService correoService)
        {
            _repository = repository;
            _FireBaseService = fireBaseService;
           _utilidadesServices = utilidadesServices;
            _correoService = correoService;
        }


        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repository.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList(); ;
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            Usuario Usuario_Extiste = await _repository.Obterner(u=>u.Correo == entidad.Correo);
            if (Usuario_Extiste != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                string claveGenerada = _utilidadesServices.GeneralCLave();
                entidad.Clave = _utilidadesServices.ConvertirSha256(claveGenerada);
                entidad.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string UrlFoto = await _FireBaseService.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = UrlFoto;
                }
                Usuario Usuario_Creado = await _repository.Crear(entidad);
                if (Usuario_Creado.IdUsuario == 0)
                    throw new TaskCanceledException("No se pudo crear el Usuario");

                if (UrlPlantillaCorreo != "")
                {
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo]", Usuario_Creado.Correo).Replace("[clave]", claveGenerada);

                    string HTMLCorreo = "";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream datastream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;

                            if (response.CharacterSet == null)
                                readerStream = new StreamReader(datastream);
                            else
                                readerStream = new StreamReader(datastream, Encoding.GetEncoding(response.CharacterSet));
                            HTMLCorreo = readerStream.ReadToEnd();
                            response.Close();
                            readerStream.Close();



                        }

                    }
                    if (HTMLCorreo != null)
                        await _correoService.Enviarcorreo(Usuario_Creado.Correo, "Cuenta Creada", HTMLCorreo);
                }
                IQueryable<Usuario> query = await _repository.Consultar(u=> u.IdUsuario==Usuario_Creado.IdUsuario);
                Usuario_Creado = query.Include(r => r.IdRolNavigation).First();
                return Usuario_Creado;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario Usuario_Extiste = await _repository.Obterner(u => u.Correo == entidad.Correo && u.IdUsuario!=entidad.IdUsuario);
            if (Usuario_Extiste != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repository.Consultar(u => u.IdUsuario == entidad.IdUsuario);
                Usuario Ususario_Editar = queryUsuario.First();
                Ususario_Editar.Nombre = entidad.Nombre;
                Ususario_Editar.Correo = entidad.Correo;
                Ususario_Editar.Telefono = entidad.Telefono;
                Ususario_Editar.IdRol = entidad.IdRol;

                if (Ususario_Editar.NombreFoto == "")
                    Ususario_Editar.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string urlFoto = await _FireBaseService.SubirStorage(Foto, "carpeta_usuario", Ususario_Editar.NombreFoto);
                    Ususario_Editar.UrlFoto = urlFoto;

                }

                bool respuesta = await _repository.Editar(Ususario_Editar);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el usuario");

                Usuario Usuario_Editado = queryUsuario.Include(r => r.IdRolNavigation).First();

                return Usuario_Editado;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                Usuario Usuario_Encontrado = await _repository.Obterner(u => u.IdUsuario == IdUsuario);
                if(Usuario_Encontrado==null)
                    throw new TaskCanceledException("El usuario no existe");

                string NombreFoto = Usuario_Encontrado.NombreFoto;
                bool respuesta = await _repository.eliminar(Usuario_Encontrado);

                if (respuesta)
                    await _FireBaseService.EliminarStorage("carpeta_usuario", NombreFoto);

                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Usuario> ObtenerPorCrendencial(string Correo, string Clave)
        {
            string ClaveEncriptada = _utilidadesServices.ConvertirSha256(Clave);
            Usuario Usuario_Encontrado = await _repository.Obterner(u =>u.Correo.Equals(Correo) 
            && u.Clave.Equals(ClaveEncriptada));
            return Usuario_Encontrado;
        }

        public async Task<Usuario> obtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repository.Consultar(u => u.IdUsuario == IdUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();

            return resultado;

        }

        public async Task<bool> GuardarPerfil(Usuario Entidad)
        {
            try
            {
                Usuario Usuario_Encontrado = await _repository.Obterner(u => u.IdUsuario == Entidad.IdUsuario);

                if(Usuario_Encontrado==null)
                    throw new TaskCanceledException("El ususario no existe");

                Usuario_Encontrado.Correo = Entidad.Correo;
                Usuario_Encontrado.Telefono = Entidad.Telefono;
                bool respuesta = await _repository.Editar(Usuario_Encontrado);

                return respuesta;

            }
            catch (Exception)
            {

                throw;
            }    
        }

        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string CLaveNueva)
        {
            Usuario Usuario_Encontrado = await _repository.Obterner(u => u.IdUsuario == IdUsuario);

            if(Usuario_Encontrado== null)
                throw new TaskCanceledException("El ususario no existe");

            if (Usuario_Encontrado.Clave != _utilidadesServices.ConvertirSha256(ClaveActual))
                throw new TaskCanceledException("La contraseña ingresada como actual no es correcta");

            Usuario_Encontrado.Clave= _utilidadesServices.ConvertirSha256(CLaveNueva);

            bool respuesta = await _repository.Editar(Usuario_Encontrado);

            return respuesta;
        }

        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            Usuario Usuario_Encontrado =await _repository.Obterner(u => u.Correo == Correo);
            if (Usuario_Encontrado == null)
                throw new TaskCanceledException("No encontramos usuario asociado al correo");

            string Clave_Generada = _utilidadesServices.GeneralCLave();
            Usuario_Encontrado.Clave = _utilidadesServices.ConvertirSha256(Clave_Generada);

            UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", Clave_Generada);

            string HTMLCorreo = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (Stream datastream = response.GetResponseStream())
                {
                    StreamReader readerStream = null;

                    if (response.CharacterSet == null)
                        readerStream = new StreamReader(datastream);
                    else
                        readerStream = new StreamReader(datastream, Encoding.GetEncoding(response.CharacterSet));
                    HTMLCorreo = readerStream.ReadToEnd();
                    response.Close();
                    readerStream.Close();
                }
            }
            bool correo_enviado = false;

            if (HTMLCorreo != null)
             correo_enviado=   await _correoService.Enviarcorreo(Correo, "Contraseña Restablecida", HTMLCorreo);

            if(!correo_enviado)
                throw new TaskCanceledException("Tenemos problemas.Por favor intentalo de nuevo mas tarde");

            bool respuesta = await _repository.Editar(Usuario_Encontrado);

            return respuesta;




        }
    }
}
