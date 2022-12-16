using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IFarebaseServices
    {
        Task<string> SubirStorage(Stream StraemArchivo, string CarpetaDestino, string NombreArchivo);

        Task<bool> EliminarStorage(string CarpetaDestino, string NombreArchivo);
    }
}
