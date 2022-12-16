using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.BLL.Interfaces;
using Firebase.Auth;
using Firebase.Storage;
using SistemaVenta.Entity;
using SistemaVenta.DAL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    public class FarebaseServicio : IFarebaseServices
    {
        private readonly IGenericRepository<Configuracion> _repository;
        public FarebaseServicio(IGenericRepository<Configuracion> repository)
        {
            _repository = repository;
        }

        public async Task<string> SubirStorage(Stream StraemArchivo, string CarpetaDestino, string NombreArchivo)
        {
            string UrlIamgen = "";
            try
            {
                IQueryable<Configuracion> queri = await _repository.Consultar(c => c.Recurso.Equals("FireBase_Storage"));

                Dictionary<string, string> Config = queri.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                var Auth = new FirebaseAuthProvider(new FirebaseConfig(Config["api_key"]));
                var a = await Auth.SignInWithEmailAndPasswordAsync(Config["email"], Config["clave"]);   
                var Cancellation = new CancellationTokenSource();
                var task = new FirebaseStorage(
                    Config["ruta"],
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    })
                .Child(Config[CarpetaDestino])
                .Child(NombreArchivo)
                .PutAsync(StraemArchivo, Cancellation.Token);

                UrlIamgen = await task;
            }
            catch (Exception ex)
            {
                UrlIamgen = "";
                throw ex;
            }

            return UrlIamgen;
        }

        public async Task<bool> EliminarStorage(string CarpetaDestino, string NombreArchivo)
        {
            try
            {
                IQueryable<Configuracion> queri = await _repository.Consultar(c => c.Recurso.Equals("FireBase_Storage"));

                Dictionary<string, string> Config = queri.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                var Auth = new FirebaseAuthProvider(new FirebaseConfig(Config["api_key"]));
                var a = await Auth.SignInWithEmailAndPasswordAsync(Config["email"], Config["clave"]);
                var Cancellation = new CancellationTokenSource();
                var task = new FirebaseStorage(
                    Config["ruta"],
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    })
                .Child(Config[CarpetaDestino])
                .Child(NombreArchivo)
                .DeleteAsync();

                await task;
                return true;
            }
            
            catch 
            {
                return false;
                
            }

            
        }

    }
}
