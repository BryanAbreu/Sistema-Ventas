using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DBVENTAContext _dbcontext;

        public VentaRepository(DBVENTAContext dbcontext): base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventagenerada = new Venta();
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    foreach (DetalleVenta dv in entidad.DetalleVenta)
                    {
                        Producto producto_encontrado = _dbcontext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();
                        producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad;
                        _dbcontext.Productos.Update(producto_encontrado);
                    }
                    await _dbcontext.SaveChangesAsync();

                    NumeroCorrelativo correlativo = _dbcontext.NumeroCorrelativos.Where(n => n.Gestion == "ventas").First();

                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaActualizacion = DateTime.Now;
                    _dbcontext.NumeroCorrelativos.Update(correlativo);
                    await _dbcontext.SaveChangesAsync();

                    string ceros = string.Concat(Enumerable.Repeat(0, correlativo.CantidadDigitos.Value));
                    string numeroventa = ceros + correlativo.UltimoNumero.ToString();
                    numeroventa = numeroventa.Substring(numeroventa.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);

                    entidad.NumeroVenta = numeroventa;

                    await _dbcontext.Venta.AddAsync(entidad);
                    await _dbcontext.SaveChangesAsync();
                    ventagenerada = entidad;

                    transaction.Commit();
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                return ventagenerada;
            }
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime Fechainicio, DateTime Fechafin)
        {
            List<DetalleVenta> listaResumen = await _dbcontext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= Fechainicio.Date &&
                dv.IdVentaNavigation.FechaRegistro.Value.Date <= Fechafin).ToListAsync();

            return listaResumen;
        }
    }
}
