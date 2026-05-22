using System.Drawing;

namespace SnakeMultiplayer.Models
{
    public enum Direccion
    {
        Arriba,
        Abajo,
        Izquierda,
        Derecha
    }
    public class Tablero
    {
        public Point Manzana { get; set; }
        int ancho = 20;
        int alto = 15;
        public List<Point> Serpiente1 { get; set; } = [];
        public List<Point> Serpiente2 { get; set; } = [];
        public bool Terminado { get; set; } 
        public string? Ganador { get; set; } 
        public int Puntos1 {  get; set; }
        public int Puntos2 {  get; set; }
        public Direccion Direccion1 { get; set; }
        public Direccion Direccion2 { get; set; }

    }
}
