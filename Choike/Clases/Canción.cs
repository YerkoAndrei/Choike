using System;

namespace Choike.Clases
{
    public class Canción
    {
        public int Índice { get; set; }
        public string Ruta { get; set; }
        public string Autor { get; set; }
        public string Nombre { get; set; }
        public string Album { get; set; }
        public TimeSpan Duración { get; set; }
    }

    public class InfoCanción
    {
        public string Autor { get; set; }
        public string Nombre { get; set; }
        public string Duración { get; set; }
    }
}
