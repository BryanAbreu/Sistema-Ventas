using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUtilidadesServices
    {
        string GeneralCLave();
        string ConvertirSha256(string texto);

    }
}
