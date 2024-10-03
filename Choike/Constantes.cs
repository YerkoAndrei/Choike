// YerkoAndrei
using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Color = System.Windows.Media.Color;

namespace Choike;

public static class Constantes
{
    public static string ExtensionesMúsica = "*.mp3";
    private static string NombreCarpeta = "Choike";
    private static string ArchivoGuardado = "carpetas.choike";

    private static string ColorCarpeta = "#ffc8ff";
    private static string ColorAutor = "#ffffc8";

    public static Color ColorGris = Color.FromRgb(120, 120, 100);
    public static System.Windows.Media.Brush BrochaResaltado = new SolidColorBrush(Color.FromRgb(200, 200, 100));

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

    public static ImageSource ByteAImagen(byte[] byteData)
    {
        try
        {
            var bitImg = new BitmapImage();
            var ms = new MemoryStream(byteData);
            bitImg.BeginInit();
            bitImg.StreamSource = ms;
            bitImg.EndInit();

            ImageSource imgSrc = bitImg as ImageSource;
            return imgSrc;
        }
        catch
        {
            return ObtenerSinCarátula();
        }
    }

    public static ImageSource ObtenerSinCarátula()
    {
        var bitImg = new BitmapImage();

        bitImg.BeginInit();
        bitImg.UriSource = new Uri("pack://application:,,,/Arte/SinCarátula.png");
        bitImg.EndInit();

        ImageSource imgSrc = bitImg as ImageSource;
        return imgSrc;
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

            if(array != null)
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

    public static Color ObtenerColorDominante(byte[] byteData)
    {
        try
        {
            var ms = new MemoryStream(byteData);
            var bitmap = new Bitmap(ms);

            float r = 0;
            float g = 0;
            float b = 0;
            int total = 0;

            // Revisa pixel por pixel
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var clr = bitmap.GetPixel(x, y);

                    // Se salta pixeles blancos y negros
                    if (clr.R > 250 && clr.G > 250 && clr.B > 250 ||
                        clr.R < 5 && clr.G < 5 && clr.B < 5)
                    {
                        total++;
                        continue;
                    }

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            r /= total;
            g /= total;
            b /= total;

            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }
        catch
        {
            return ColorGris;
        }
    }
}
