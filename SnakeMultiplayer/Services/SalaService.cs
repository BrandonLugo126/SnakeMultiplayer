using SnakeMultiplayer.Models;
using System.Collections.Concurrent;

namespace SnakeMultiplayer.Services
{
    public class SalaService
    {
        public static ConcurrentDictionary<string, Sala> Salas { get; set; } = new ConcurrentDictionary<string, Sala>();
        public static ConcurrentDictionary<string, string> JugadorEspera { get; set; } = new ConcurrentDictionary<string, string>();

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


        }
    }
}
