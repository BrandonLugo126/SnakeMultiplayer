namespace SnakeMultiplayer.Models
{
    public class Sala
    {
        public string IdSala { get; set; } = "";
        public Tablero Tablero { get; set; } = new Tablero();
        public string? IdJugador1 { get; set; } 
        public string? IdJugador2 { get; set; } 
        public string? NombreJugador1 { get; set; }
        public string? NombreJugador2 { get; set; }
    }
}
