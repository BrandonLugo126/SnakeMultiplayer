using Microsoft.AspNetCore.SignalR;
using SnakeMultiplayer.Models;
using SnakeMultiplayer.Services;

namespace SnakeMultiplayer.Hubs
{
    public class GameHub : Hub
    {
        private readonly SalaService service;
        public GameHub(SalaService service)
        {
            this.service = service;
        }

        

        public async Task Conectar(string nombreJugador)
        {
            var id = Context.ConnectionId;
            if (!string.IsNullOrWhiteSpace(nombreJugador)) { 
                var sala =service.BuscarSala(id,nombreJugador);
                if (sala == null) // esta en espera
                {
                    await Clients.Caller.SendAsync("EsperandoConexion");
                }
                else if(sala.IdJugador1!=null && sala.IdJugador2!=null)
                {
                    service.IniciarJuego(sala);

                    await Clients.Client(sala.IdJugador1).SendAsync("JuegoIniciado", sala.NombreJugador2, sala.Tablero);
                    await Clients.Caller.SendAsync("JuegoIniciado", sala.NombreJugador1, sala.Tablero);

                }
            }
        }

        public async Task Mover(string nombreJugador,string direccion)
        {
            var id = Context.ConnectionId;
            var sala = service.Buscar(id);

            if (sala!=null)
            {
                var dir =(Direccion)Enum.Parse(typeof(Direccion), direccion);

                service.CambiarDireccion(sala,id,dir);
            }
        }
    }
}
