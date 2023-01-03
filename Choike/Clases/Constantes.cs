// YerkoAndrei
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Choike.Clases
{
    public class Constantes
    {
        public static string extensionesMúsica = "*.mp3";
        private static string archivoGuardado = "\\listaCarpetas.choike";

        private static string colorCarpeta = "#ffc8ff";
        private static string colorAutor = "#ffffc8";

        public enum TipoCarpeta
        {
            carpeta,
            autor,
        }

        public static ImageSource ByteAImagen(byte[] byteData)
        {
            try
            {
                BitmapImage bitImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(byteData);
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
            BitmapImage bitImg = new BitmapImage();

            bitImg.BeginInit();
            bitImg.UriSource = new Uri("C:\\Users\\YerkoAndrei\\Desktop\\Proyectos\\Choike\\Choike\\Arte\\SinCarátula.png");
            bitImg.EndInit();

            ImageSource imgSrc = bitImg as ImageSource;
            return imgSrc;
        }

        public static ImageSource ObtenerCarátulaDañada()
        {
            BitmapImage bitImg = new BitmapImage();

            bitImg.BeginInit();
            bitImg.UriSource = new Uri("C:\\Users\\YerkoAndrei\\Desktop\\Proyectos\\Choike\\Choike\\Arte\\CarátulaDañada.png");
            bitImg.EndInit();

            ImageSource imgSrc = bitImg as ImageSource;
            return imgSrc;
        }

        public static List<Carpeta> CargarCarpetasGuardadas()
        {
            var carpetasGuardadas = Directory.GetCurrentDirectory() + archivoGuardado;

            if (!File.Exists(carpetasGuardadas))
                return new List<Carpeta>();

            try
            {
                // Carpetas guardadas
                var json = File.ReadAllText(carpetasGuardadas);
                var array = JsonConvert.DeserializeObject<Carpeta[]>(json);

                return array.ToList();
            }
            catch
            {
                return new List<Carpeta>();
            }
        }

        public static void ActualizarCarpetasGuardadas(List<Carpeta> listaCarpetas)
        {
            var carpetasGuardadas = Directory.GetCurrentDirectory() + archivoGuardado;

            var json = JsonConvert.SerializeObject(listaCarpetas.ToArray());
            File.WriteAllText(carpetasGuardadas, json);
        }

        public static string TimeSpanATexto(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
        }

        public static string ObtenerColorPorTipoCarpeta(TipoCarpeta tipoCarpeta)
        {
            switch(tipoCarpeta)
            {
                default:
                case TipoCarpeta.carpeta:
                    return colorCarpeta;
                case TipoCarpeta.autor:
                    return colorAutor;
            }
        }
    }
}
