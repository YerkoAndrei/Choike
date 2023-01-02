// YerkoAndrei
using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using Choike.Clases;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;

namespace Choike
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer;

        private List<Canción> cancionesActuales;
        private List<Carpeta> carpetasActuales;

        private bool pausa;
        private bool repetirCanción;
        private bool aleatorio;

        private Carpeta carpetaActual;
        private Canción cancionActual;
        private int índiceActual;

        private Timer contador;

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            // Valores predeterminados
            pausa = false;
            aleatorio = true;
            repetirCanción = false;
            índiceActual = 0;

            volumen.Value = 0.5;
            mediaPlayer.Volume = volumen.Value;

            carpetaActual = new Carpeta();
            cancionActual = new Canción();

            cancionesActuales = new List<Canción>();
            carpetasActuales = new List<Carpeta>();

            contador = new Timer();
            contador.Interval = 10;
            contador.Enabled = false;
            contador.Elapsed += new ElapsedEventHandler(IntervaloTiempo);

            // Carpetas guardadas
            carpetasActuales = Constantes.CargarCarpetasGuardadas();
            foreach (var carpeta in carpetasActuales)
            {
                listaCarpetas.Items.Add(carpeta.Nombre);
            }
        }

        private void IntervaloTiempo(object sender, EventArgs e)
        {
            // intento multi hilo?
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (mediaPlayer.IsMuted)
                    return;

                duraciónActual.Text = Constantes.TimeSpanATexto(mediaPlayer.Position);
                porcentajeDuraciónActual.Value = (mediaPlayer.Position.TotalSeconds / cancionActual.Duración.TotalSeconds);
            }));
        }


        // --- Botones ---


        private void EnTecla(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Space:
                case Key.MediaPlayPause:
                    EnClicPausa(sender, e);
                    break;
                case Key.MediaNextTrack:
                    EnClicSiguiente(sender, e);
                    break;
                case Key.MediaPreviousTrack:
                    EnClicAnterior(sender, e);
                    break;
                case Key.MediaStop:
                    EnClicDetener(sender, e);
                    break;
                case Key.VolumeMute:
                    EnClicSilencio(sender, e);
                    break;
                case Key.PageUp:
                    mediaPlayer.Volume += volumen.Value;
                    volumen.Value = volumen.LargeChange;
                    break;
                case Key.PageDown:
                    mediaPlayer.Volume -= volumen.Value;
                    volumen.Value = volumen.LargeChange;
                    break;
            }
        }

        private void EnClicPausa(object sender, RoutedEventArgs e)
        {
            if (pausa)
                mediaPlayer.Play();
            else
                mediaPlayer.Pause();

            pausa = !pausa;
        }

        private void EnClicAnterior(object sender, RoutedEventArgs e)
        {
            var nuevoÍndice = índiceActual - 1;

            if (nuevoÍndice < 0)
                nuevoÍndice = cancionesActuales.Count - 1;

            listaCanciones.SelectedIndex = cancionesActuales[nuevoÍndice].Índice;
            índiceActual = nuevoÍndice;
            pausa = false;

        }

        private void EnClicSiguiente(object sender, RoutedEventArgs e)
        {
            var nuevoÍndice = índiceActual += 1;

            if (nuevoÍndice >= cancionesActuales.Count)
                nuevoÍndice = 0;

            listaCanciones.SelectedIndex = cancionesActuales[nuevoÍndice].Índice;
            índiceActual = nuevoÍndice;
            pausa = false;
        }

        private void EnClicSilencio(object sender, RoutedEventArgs e)
        {
            volumen.Value = 0;
            mediaPlayer.Volume = volumen.Value;
        }

        private void EnClicDetener(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            contador.Stop();
        }

        private void EnClicAleatorio(object sender, RoutedEventArgs e)
        {
            aleatorio = !aleatorio;

            if(aleatorio)
            {
                var random = new Random();
                cancionesActuales = cancionesActuales.OrderBy(o => random.Next()).ToList();
                índiceActual = 0;
            }
            else
            {
                cancionesActuales = cancionesActuales.OrderBy(o => o.Índice).ToList();
                índiceActual = 0;
            }
        }

        private void EnClicRepetir(object sender, RoutedEventArgs e)
        {
            repetirCanción = !repetirCanción;
        }

        private void EnCambioVolumen(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumen.Value;
        }

        private void EnCambioTiempo(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (string.IsNullOrEmpty(cancionActual.Ruta))
                return;

            mediaPlayer.Position = TimeSpan.FromSeconds(porcentajeDuraciónActual.Value * cancionActual.Duración.TotalSeconds);
        }

        // --- Órden canciones ---


        private void EnClicElegirCanción(object sender, SelectionChangedEventArgs e)
        {
            if (listaCanciones.SelectedIndex < 0)
                return;

            var canción = cancionesActuales[listaCanciones.SelectedIndex];

            MostrarDatosCanción(canción, canción.Ruta);

            // Reproducción
            mediaPlayer.Open(new Uri(canción.Ruta));
            mediaPlayer.MediaEnded += new EventHandler(SiguienteCanción);
            mediaPlayer.Play();
            contador.Start();
            pausa = false;
        }

        private void EnClicElegirCanciónEnCarpeta(object sender, SelectionChangedEventArgs e)
        {
            if (listaCancionesEnCarpeta.SelectedIndex < 0)
                return;

            var canción = cancionesActuales[listaCancionesEnCarpeta.SelectedIndex];

            MostrarDatosCanción(canción, canción.Ruta);

            // Reproducción
            mediaPlayer.Open(new Uri(canción.Ruta));
            mediaPlayer.MediaEnded += new EventHandler(SiguienteCanción);
            mediaPlayer.Play();
            contador.Start();
            pausa = false;

            ActualizarListaCanciones();
        }
                
        private void SiguienteCanción(object sender, EventArgs e)
        {
            EnClicSiguiente(null, null);
        }


        // --- Carpetas ---


        private void EnClicAgregarCarpeta(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.Multiselect = false;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var rutaCarpeta = dialog.FileName;
                var carpetaCortada = rutaCarpeta.Split('\\');
                var nombreCarpeta = carpetaCortada[carpetaCortada.Length - 1];

                // Si la carpeta existe
                if (!Directory.Exists(rutaCarpeta))
                    return;

                // Archivos
                var archivosMúsica = Directory.GetFiles(rutaCarpeta, Constantes.extensionesMúsica);

                // Evita repetir carpetas
                if (carpetasActuales.Any(o => o.Ruta == rutaCarpeta))
                    return;

                // Agregar carpeta seleccionada
                var nuevaCarpeta = new Carpeta();
                nuevaCarpeta.Ruta = rutaCarpeta;
                nuevaCarpeta.Nombre = nombreCarpeta;

                carpetasActuales.Add(nuevaCarpeta);
                carpetaActual = nuevaCarpeta;

                // Agregar canciones de carpeta
                Constantes.ActualizarCarpetasGuardadas(carpetasActuales);
                AgregarCanciones(rutaCarpeta);
                ActualizarListaCarpeta();
            }
        }

        private void EnClicElegirCarpeta(object sender, RoutedEventArgs e)
        {
            if (listaCarpetas.SelectedIndex < 0)
                return;

            var carpeta = carpetasActuales[listaCarpetas.SelectedIndex];
            carpetaActual = carpeta;

            AgregarCanciones(carpeta.Ruta);
            //ActualizarListaCanciones();
            ActualizarListaCancionesEnCarpeta();
        }

        private void EnClicEliminarCarpeta(object sender, RoutedEventArgs e)
        {
            var rutaCarpeta = carpetasActuales[listaCarpetas.SelectedIndex];
            carpetasActuales.Remove(rutaCarpeta);

            Constantes.ActualizarCarpetasGuardadas(carpetasActuales);
            ActualizarListaCarpeta();
        }

        private void AgregarCanciones(string carpeta)
        {
            var archivosMúsica = Directory.GetFiles(carpeta, Constantes.extensionesMúsica);
            cancionesActuales = new List<Canción>();

            for (int i = 0; i < archivosMúsica.Length; i++)
            {
                var tagLib = TagLib.File.Create(archivosMúsica[i]);
                var nuevaCanción = new Canción();

                if (tagLib.Tag.Performers.Length > 0)
                {
                    nuevaCanción.Autor = tagLib.Tag.Performers[0];

                    for (int ii = 1; ii < tagLib.Tag.Performers.Length; ii++)
                    {
                        nuevaCanción.Autor += " & " + tagLib.Tag.Performers[ii];
                    }
                }

                nuevaCanción.Índice = i;
                nuevaCanción.Ruta = archivosMúsica[i];
                nuevaCanción.Nombre = tagLib.Tag.Title;
                nuevaCanción.Album = tagLib.Tag.Album;
                nuevaCanción.Duración = tagLib.Properties.Duration;

                cancionesActuales.Add(nuevaCanción);
            }

            if (aleatorio)
            {
                var random = new Random();
                cancionesActuales = cancionesActuales.OrderBy(o => random.Next()).ToList();
                índiceActual = 0;
            }
        }

        // --- Interfaz ---

        private void MostrarEstadoCanción(string nombreArchivo)
        {

        }

        private void MostrarDatosCanción(Canción canción, string nombreArchivo)
        {
            cancionActual = canción;

            // Carátula
            TagLib.File tagLibFile = TagLib.File.Create(nombreArchivo);

            var imágenes = tagLibFile.Tag.Pictures;

            // Revisa si tiene carátula
            if (imágenes.Length > 0 && imágenes[0].Data.Data != null)
            {
                var imagenÁlbum = (byte[])(imágenes[0].Data.Data);
                imgCarátula.Source = Constantes.ByteAImagen(imagenÁlbum);
            }
            else
            {
                imgCarátula.Source = Constantes.ObtenerSinCarátula();
            }

            // Datos
            nombreArtista.Text = cancionActual.Autor;
            nombreCanción.Text = cancionActual.Nombre;
            nombreAlbum.Text = cancionActual.Album;
            duraciónCompleta.Text = Constantes.TimeSpanATexto(cancionActual.Duración);
        }

        private void ActualizarListaCarpeta()
        {
            listaCarpetas.Items.Clear();
            listaCarpetas.SelectedIndex = -1;
            listaCanciones.SelectedIndex = -1;

            carpetasActuales = carpetasActuales.OrderBy(o => o.Nombre).ToList();

            for (int i = 0; i < carpetasActuales.Count; i++)
            {
                listaCarpetas.Items.Add(carpetasActuales[i].Nombre);
            }
        }

        private void ActualizarListaCanciones()
        {
            listaCanciones.Items.Clear();
            listaCanciones.SelectedIndex = -1;

            if(aleatorio)
            {
                // corregir
                cancionesActuales = cancionesActuales.OrderBy(o => o.Índice).ToList();
            }
            else
            {
                cancionesActuales = cancionesActuales.OrderBy(o => o.Nombre).ToList();
                cancionesActuales = cancionesActuales.OrderBy(o => o.Autor).ToList();
            }

            var infoCanciones = new List<InfoCanción>();
            for (int i = 0; i < cancionesActuales.Count; i++)
            {
                var nuevaCanción = new InfoCanción();
                nuevaCanción.Autor = cancionesActuales[i].Autor;
                nuevaCanción.Nombre = cancionesActuales[i].Nombre;
                nuevaCanción.Duración = Constantes.TimeSpanATexto(cancionesActuales[i].Duración);
                infoCanciones.Add(nuevaCanción);
            }

            listaCanciones.ItemsSource = infoCanciones;
        }

        private void ActualizarListaCancionesEnCarpeta()
        {
            listaCancionesEnCarpeta.Items.Clear();
            listaCancionesEnCarpeta.SelectedIndex = -1;

            cancionesActuales = cancionesActuales.OrderBy(o => o.Nombre).ToList();
            cancionesActuales = cancionesActuales.OrderBy(o => o.Autor).ToList();

            var infoCanciones = new List<InfoCanción>();
            for (int i = 0; i < cancionesActuales.Count; i++)
            {
                var nuevaCanción = new InfoCanción();
                nuevaCanción.Autor = cancionesActuales[i].Autor;
                nuevaCanción.Nombre = cancionesActuales[i].Nombre;
                nuevaCanción.Duración = Constantes.TimeSpanATexto(cancionesActuales[i].Duración);
                infoCanciones.Add(nuevaCanción);
            }

            listaCancionesEnCarpeta.ItemsSource = infoCanciones;
        }
    }
}
