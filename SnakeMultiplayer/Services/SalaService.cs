using SnakeMultiplayer.Models;
using System.Collections.Concurrent;
using System.Drawing;

namespace SnakeMultiplayer.Services
{
    public class SalaService
    {
        public static ConcurrentDictionary<string, Sala> Salas { get; set; } = new ConcurrentDictionary<string, Sala>();
        public static ConcurrentDictionary<string, string> JugadorEspera { get; set; } = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, Timer> Timer { get; set; } = new ConcurrentDictionary<string, Timer>();

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

            var timer = new Timer((x) =>
            {
                MoverSerpientes(sala);
            }, null, 100, 500);

            Timer[sala.IdSala] = timer;

        }

        public event Action<Sala>? TableroActualizado;

        public void MoverSerpientes(Sala sala)
        {
            var S1 = sala.Tablero.Serpiente1;
            var S2 = sala.Tablero.Serpiente2;

            //BorrarLacolas
            S1.RemoveAt(S1.Count - 1);
            S2.RemoveAt(S2.Count - 1);

            //Avanzar la serpiente 

            var nuevo = new Point(S1[0].X, S1[0].Y);
            S1.Insert(0, nuevo);

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
            S2.Insert(0, nuevo2);
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

            TableroActualizado?.Invoke(sala);
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
    }
}
