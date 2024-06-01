namespace Choike;

public class Canción
{
    public int Índice { get; set; }
    public string Ruta { get; set; }
    public string Autor { get; set; }
    public string Nombre { get; set; }
    public string Álbum { get; set; }
    public string Detalles { get; set; }
    public TimeSpan Duración { get; set; }
    public string DuraciónFormateada { get; set; }

    public Canción()
    {
        Ruta = string.Empty;
        Autor = string.Empty;
        Nombre = string.Empty;
        Álbum = string.Empty;
        Detalles = string.Empty;
        DuraciónFormateada = string.Empty;
    }
}
