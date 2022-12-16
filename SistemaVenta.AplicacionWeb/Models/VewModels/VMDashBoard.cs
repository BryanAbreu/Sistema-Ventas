namespace SistemaVenta.AplicacionWeb.Models.VewModels
{
    public class VMDashBoard
    {
        public int TotalVentas { get; set; }
        public string? TotalIngresos { get; set; }
        public int TotalProductos { get; set; }
        public int TotalCategoriads { get; set; }

        public List<VMVentaSemana> VentasUltimaSemana { get; set; }
        public List<VMProductosSemana> ProductosUltimaSemana { get; set; }


    }
}
