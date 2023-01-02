using System;
using System.Windows.Controls;

namespace Choike.Clases
{
    public class Canción
    {
        public int Índice { get; set; }
        public string Ruta { get; set; }
        public string Autor { get; set; }
        public string Nombre { get; set; }
        public string Álbum { get; set; }
        public TimeSpan Duración { get; set; }
        public string DuraciónFormateada { get; set; }
    }
}
