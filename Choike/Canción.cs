using System;

namespace Choike;

public class Canción
{
    public int Índice { get; set; }
    public string Ruta { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Álbum { get; set; } = string.Empty;
    public string Detalles { get; set; } = string.Empty;
    public TimeSpan Duración { get; set; }
    public string DuraciónFormateada { get; set; } = string.Empty;
}
