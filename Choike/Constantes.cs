// YerkoAndrei
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;

namespace Choike;

public static class Constantes
{
    public static string ExtensionesMúsica = "*.mp3";
    private static string NombreCarpeta = "Choike";
    private static string ArchivoGuardado = "ccarpetas.choike";

    private static string ColorCarpeta = "#ffc8ff";
    private static string ColorAutor = "#ffffc8";

    public static Brush BrochaResaltado = new SolidColorBrush(Color.FromRgb(200, 200, 100));
    public static Brush BrochaGris = new SolidColorBrush(Color.FromRgb(120, 120, 100));
    public static Brush BrochaNegra = new SolidColorBrush(Color.FromRgb(0, 0, 0));

    public static RoutedEventArgs RoutedEvent = new();
    public static EnumerationOptions EnumerationOptions = new()
    {
        // Solo normal
        AttributesToSkip = FileAttributes.ReadOnly |
                           FileAttributes.Hidden |
                           FileAttributes.System |
                           FileAttributes.Directory |
                           FileAttributes.Device |
                           FileAttributes.Temporary |
                           FileAttributes.SparseFile |
                           FileAttributes.ReparsePoint |
                           FileAttributes.Compressed |
                           FileAttributes.Offline |
                           FileAttributes.NotContentIndexed |
                           FileAttributes.Encrypted |
                           FileAttributes.IntegrityStream |
                           FileAttributes.NoScrubData,
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
        ReturnSpecialDirectories = false,
    };

    public enum TipoCarpeta
    {
        carpeta,
        autor,
    }

    public static Bitmap ByteAImagen(byte[] byteData)
    {
        try
        {
            var stream = new MemoryStream(byteData);
            var bitmap = new Bitmap(stream);
            return bitmap;
        }
        catch
        {
            return ObtenerSinCarátula();
        }
    }

    public static Bitmap ObtenerSinCarátula()
    {
        return new Bitmap(AssetLoader.Open(new Uri("avares://Arte/SinCarátula.png")));
    }

    public static List<Carpeta> CargarCarpetasGuardadas()
    {
        var carpetasGuardadas = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), NombreCarpeta);
        var archivoCarpetasGuardadas = Path.Combine(carpetasGuardadas, ArchivoGuardado);

        if (!Directory.Exists(carpetasGuardadas))
            Directory.CreateDirectory(carpetasGuardadas);

        if (!File.Exists(archivoCarpetasGuardadas))
            return new List<Carpeta>();

        try
        {
            // Carpetas guardadas
            var json = File.ReadAllText(archivoCarpetasGuardadas);
            var array = JsonSerializer.Deserialize<Carpeta[]>(json);

            if (array != null)
                return array.ToList();
            else
                return new List<Carpeta>();
        }
        catch
        {
            return new List<Carpeta>();
        }
    }

    public static void ActualizarCarpetasGuardadas(List<Carpeta> listaCarpetas)
    {
        var carpetasGuardadas = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), NombreCarpeta);
        var archivoCarpetasGuardadas = Path.Combine(carpetasGuardadas, ArchivoGuardado);

        if (!Directory.Exists(carpetasGuardadas))
            Directory.CreateDirectory(carpetasGuardadas);

        var json = JsonSerializer.Serialize(listaCarpetas.ToArray());
        File.WriteAllText(archivoCarpetasGuardadas, json);
    }

    public static string TimeSpanATexto(TimeSpan timeSpan)
    {
        if (timeSpan.Hours > 0)
            return $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        else
            return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
    }

    public static int CalcularSemilla()
    {
        var fecha = DateTime.Parse("08-02-1996");
        var tiempo = fecha - DateTime.Now;
        return (int)tiempo.TotalSeconds;
    }

    public static string ObtenerColorPorTipoCarpeta(TipoCarpeta tipoCarpeta)
    {
        switch (tipoCarpeta)
        {
            default:
            case TipoCarpeta.carpeta:
                return ColorCarpeta;
            case TipoCarpeta.autor:
                return ColorAutor;
        }
    }
    /*
    public static Brush ObtenerColorDominante(byte[] byteData)
    {
        try
        {
            var ms = new MemoryStream(byteData);
            var bitmap = WriteableBitmap.Decode(ms);
            var buffer = bitmap.Lock();

            float red = 0;
            float green = 0;
            float blue = 0;
            int total = 0;

            // Revisa pixel por pixel
            for (int x = 0; x < buffer.Size.Width; x++)
            {
                for (int y = 0; y < buffer.Size.Height; y++)
                {
                    //var pixel = bitmap.GetPixel(buffer, x, y);

                    // Brga8888
                    byte b = pixel[0];
                    byte g = pixel[1];
                    byte r = pixel[2];

                    // Se salta pixeles muy blancos o negros
                    if (r > 250 && g > 250 && b > 250 ||
                        r < 5 && g < 5 && b < 5)
                    {
                        total++;
                        continue;
                    }

                    red += r;
                    green += g;
                    blue += b;

                    total++;
                }
            }

            red /= total;
            green /= total;
            blue /= total;
            
            return new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
        }
        catch
        {
            return BrochaGris;
        }
    }*/
}
