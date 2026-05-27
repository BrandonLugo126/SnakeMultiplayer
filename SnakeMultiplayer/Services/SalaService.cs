using Microsoft.AspNetCore.SignalR;
using SnakeMultiplayer.Hubs;
using SnakeMultiplayer.Models;
using System.Collections.Concurrent;
using System.Drawing;

namespace SnakeMultiplayer.Services
{
    public class SalaService
    {
        public static ConcurrentDictionary<string, Sala> Salas { get; set; } = new ConcurrentDictionary<string, Sala>();
        public static ConcurrentDictionary<string, string> JugadorEspera { get; set; } = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, Timer> Timers { get; set; } = new ConcurrentDictionary<string, Timer>();

        private readonly IHubContext<GameHub> hub;
        public SalaService(IHubContext<GameHub> hub)
        {
            this.hub = hub;

        }

        public Sala Buscar(string idJugador)
        {
            return Salas.FirstOrDefault(x=>x.Value.IdJugador1==idJugador|| x.Value.IdJugador2==idJugador).Value;
        }
        public Sala? BuscarSala(string id, string nombre)
        {
            if (JugadorEspera.ContainsKey(id))
            {
                return null;
            }
            if (JugadorEspera.Count > 0)
            {
                Sala nueva = new()
                {
                    IdSala = Guid.NewGuid().ToString(),
                    IdJugador1 = JugadorEspera.Keys.First(),
                    NombreJugador1 = JugadorEspera.Values.First(),
                    IdJugador2 = id,
                    NombreJugador2 = nombre,
                };
                JugadorEspera.Remove(nueva.IdJugador1, out string? valor);
                Salas[nueva.IdSala] = nueva;
                return nueva;
            }
            else
            {
                JugadorEspera[id] = nombre;
                return null;
            }
        }

        public void IniciarJuego(Sala sala)
        {
            sala.Tablero.Serpiente1 =
            [
                new(4,6), new(5,6), new(6,6)
            ];
            sala.Tablero.Serpiente2 =
            [
                new(14,7), new(15,7), new(16,7)
            ];
            sala.Tablero.Direccion1 = Direccion.Derecha;
            sala.Tablero.Direccion2 = Direccion.Izquierda;

            sala.Tablero.Puntos1 = 0;
            sala.Tablero.Puntos2 = 0;

            CrearComida(sala);

            var timer = new Timer(async (x) =>
            {
                await MoverSerpientesAsync(sala);
            }, null, 100, 200);

            Timers[sala.IdSala] = timer;

        }


        public async Task MoverSerpientesAsync(Sala sala)
        {
            var S1 = sala.Tablero.Serpiente1;
            var S2 = sala.Tablero.Serpiente2;

            //BorrarLacolas
           
           

            //Avanzar la serpiente 

            var nuevo = new Point(S1[0].X, S1[0].Y);

            switch (sala.Tablero.Direccion1)
            {
                case Direccion.Derecha:

                    nuevo.X++;
                    break;
                case Direccion.Izquierda:
                    nuevo.X--;
                    break;
                case Direccion.Arriba:
                    nuevo.Y--;
                    break;
                case Direccion.Abajo:
                    nuevo.Y++;
                    break;
            }
          
            var nuevo2 = new Point(S2[0].X, S2[0].Y);
           
            switch (sala.Tablero.Direccion2)
            {
                case Direccion.Derecha:
                    nuevo2.X++;
                    break;
                case Direccion.Izquierda:
                    nuevo2.X--;
                    break;
                case Direccion.Arriba:
                    nuevo2.Y--;
                    break;
                case Direccion.Abajo:
                    nuevo2.Y++;
                    break;
            }


          

            //Checar Colisiones

            //colision contra la pared
            if (nuevo.X <0 || nuevo.Y<0 || nuevo.X >= sala.Tablero.Ancho || nuevo.Y >= sala.Tablero.Largo)
            {
                sala.Tablero.Terminado = true;
                await hub.Clients.Clients([sala.IdJugador1??"", sala.IdJugador2??""]).SendAsync("JugadorPerdio",sala.NombreJugador1);
            }
            if (nuevo2.X < 0 || nuevo2.Y < 0 || nuevo2.X >= sala.Tablero.Ancho || nuevo2.Y >= sala.Tablero.Largo)
            {
                sala.Tablero.Terminado = true;
                await hub.Clients.Clients([sala.IdJugador1??"", sala.IdJugador2??""]).SendAsync("JugadorPerdio",sala.NombreJugador1);
            }


            //colision contra otra serpiente

            if (S1.Contains(nuevo)||S2.Contains(nuevo))
            {
                sala.Tablero.Terminado = true;
                await hub.Clients.Clients([sala.IdJugador1 ?? "", sala.IdJugador2 ?? ""]).SendAsync("JugadorPerdio", sala.NombreJugador1);

            }
            if (S2.Contains(nuevo2) || S2.Contains(nuevo2))
            {
                sala.Tablero.Terminado = true;
                await hub.Clients.Clients([sala.IdJugador1 ?? "", sala.IdJugador2 ?? ""]).SendAsync("JugadorPerdio", sala.NombreJugador1);

            }


            //colision con la comida

            if (sala.Tablero.Manzana==nuevo)
            {
                sala.Tablero.Puntos1++;
                CrearComida(sala);
            }

            if (sala.Tablero.Manzana == nuevo2)
            {
                sala.Tablero.Puntos2++;
                CrearComida(sala);
            }


            if (S1.Count() > 3*sala.Tablero.Puntos1) {
                S1.RemoveAt(S1.Count - 1);

            }


            if (S2.Count() > 3 * sala.Tablero.Puntos1)
            {
                S2.RemoveAt(S2.Count - 1);

            }

            S1.Insert(0, nuevo);
            S2.Insert(0, nuevo2);

            if (!sala.Tablero.Terminado)
            {
                await hub.Clients.Client(sala.IdJugador1 ?? "").SendAsync("TableroActualizado", sala.Tablero);

                await hub.Clients.Client(sala.IdJugador2 ?? "").SendAsync("TableroActualizado", sala.Tablero);
            }
            else {

                Timers[sala.IdSala].Dispose();
                Timers.TryRemove(sala.IdSala, out Timer? t);
                Salas.TryRemove(sala.IdSala, out Sala? s);
            }
          

        }

        public void CrearComida(Sala sala)
        {
            //Si no hay espacios es fin de juego
            if (sala.Tablero.Ancho * sala.Tablero.Largo == sala.Tablero.Serpiente1.Count + sala.Tablero.Serpiente2.Count)
            {
                //FIn del juego
            }
            //Si hay espacios, asisgnar al azar

            Random r = new();
            Point point;
            do
            {
                point = new Point(r.Next(sala.Tablero.Ancho), r.Next(sala.Tablero.Largo));
            }
            while (sala.Tablero.Serpiente1.Any(x => x.X == point.X && x.Y == point.Y) || sala.Tablero.Serpiente2.Any(x => x.X == point.X && x.Y == point.Y));
            {
                sala.Tablero.Manzana = point;
            }
        }

        public void CambiarDireccion(Sala sala, string id, Direccion nueva)
        {
            if (sala.IdJugador1==id)
            {
                var actual = sala.Tablero.Direccion1;
                switch (nueva)
                {
                    case Direccion.Arriba:
                        if (actual != Direccion.Abajo)                        
                            sala.Tablero.Direccion1 = nueva;
                        
                        break;
                    case Direccion.Abajo:
                        if (actual != Direccion.Arriba)
                            sala.Tablero.Direccion1 = nueva;
                        break;
                    case Direccion.Izquierda:
                        if (actual != Direccion.Derecha)
                            sala.Tablero.Direccion1 = nueva;
                        break;
                    case Direccion.Derecha:
                        if (actual != Direccion.Izquierda)
                            sala.Tablero.Direccion1 = nueva;
                        break;
                    default:
                        break;
                }

            }
            else
            {
                var actual = sala.Tablero.Direccion2;
                switch (nueva)
                {
                    case Direccion.Arriba:
                        if (actual != Direccion.Abajo)
                            sala.Tablero.Direccion2= nueva;

                        break;
                    case Direccion.Abajo:
                        if (actual != Direccion.Arriba)
                            sala.Tablero.Direccion2 = nueva;
                        break;
                    case Direccion.Izquierda:
                        if (actual != Direccion.Derecha)
                            sala.Tablero.Direccion2 = nueva;
                        break;
                    case Direccion.Derecha:
                        if (actual != Direccion.Izquierda)
                            sala.Tablero.Direccion2 = nueva;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
