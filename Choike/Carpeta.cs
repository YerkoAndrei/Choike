namespace Choike;

public class Carpeta
{
    public Constantes.TipoCarpeta Tipo { get; set; }
    public string Nombre { get; set; }
    public string Ruta { get; set; }
    public string Color { get; set; }

    public Carpeta()
    {
        Nombre = string.Empty;
        Ruta = string.Empty;
        Color = string.Empty;
    }
}
