using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL;
using SistemaVenta.Entity;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.AplicacionWeb.Models.VewModels;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class UsuarioController : Controller
    {
        
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolservicio;
        private readonly IMapper _mapper;

        public UsuarioController(IUsuarioService usuarioService, IRolService rolservicio, IMapper mapper)
        {
            _usuarioService = usuarioService;
            _rolservicio = rolservicio;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaRol()
        {
         
        List<VMRol>vmListaRoles = _mapper.Map<List<VMRol>>(await _rolservicio.lista());
            return StatusCode(StatusCodes.Status200OK, vmListaRoles);
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {

            List<VMUsuario> vmListaUsuaros = _mapper.Map<List<VMUsuario>>(await _usuarioService.Lista());
            return StatusCode(StatusCodes.Status200OK, new {data= vmListaUsuaros });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm]IFormFile foto, [FromForm]string modelo)
        {
            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();
            try
            {
                VMUsuario vmUsusario = JsonConvert.DeserializeObject<VMUsuario>(modelo);
                string nombreFoto = "";
                Stream FotoStraem = null;

                if(foto!=null)
                {
                    string nombrecodigo = Guid.NewGuid().ToString("N");
                    string extention = Path.GetExtension(foto.FileName);
                    nombreFoto = string.Concat(nombrecodigo, extention);
                    FotoStraem = foto.OpenReadStream(); 

                }
                string UrlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/EnviarClave?=[correo]&clave[clave]";

                Usuario usuario_creado= await _usuarioService.Crear(_mapper.Map<Usuario>(vmUsusario), FotoStraem, nombreFoto, UrlPlantillaCorreo);

                vmUsusario = _mapper.Map<VMUsuario>(usuario_creado);
                gResponse.Estado = true;
                gResponse.Objeto = vmUsusario;

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
                throw;
            }
            return StatusCode(StatusCodes.Status200OK,gResponse);
        }


        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();
            try
            {
                VMUsuario vmUsusario = JsonConvert.DeserializeObject<VMUsuario>(modelo);
                string nombreFoto = "";
                Stream FotoStraem = null;

                if (foto != null)
                {
                    string nombrecodigo = Guid.NewGuid().ToString("N");
                    string extention = Path.GetExtension(foto.FileName);
                    nombreFoto = string.Concat(nombrecodigo, extention);
                    FotoStraem = foto.OpenReadStream();

                }
                
                Usuario usuario_editado = await _usuarioService.Editar(_mapper.Map<Usuario>(vmUsusario), FotoStraem, nombreFoto);

                vmUsusario = _mapper.Map<VMUsuario>(usuario_editado);
                gResponse.Estado = true;
                gResponse.Objeto = vmUsusario;

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
                throw;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int IdUsuario)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.Estado = await _usuarioService.Eliminar(IdUsuario);

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
                throw;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
