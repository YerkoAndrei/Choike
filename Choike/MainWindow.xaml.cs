﻿// YerkoAndrei
using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;
using Choike.Clases;
using TagLib.Ape;

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
            /*
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (mediaPlayer.IsMuted)
                    return;

                duraciónActual.Text = Constantes.TimeSpanATexto(mediaPlayer.Position);
                porcentajeDuraciónActual.Value = (mediaPlayer.Position.TotalSeconds / cancionActual.Duración.TotalSeconds);
            }));
            */
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
                case Key.VolumeUp:
                case Key.PageUp:
                    mediaPlayer.Volume += volumen.Value;
                    volumen.Value = volumen.LargeChange;
                    break;
                case Key.VolumeDown:
                case Key.PageDown:
                    mediaPlayer.Volume -= volumen.Value;
                    volumen.Value = volumen.LargeChange;
                    break;
                case Key.F9:
                    EnClicAleatorio(sender, e);
                    break;
                case Key.F10:
                    EnClicRepetir(sender, e);
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

            EnfocarCanción(nuevoÍndice);
        }

        private void EnClicSiguiente(object sender, RoutedEventArgs e)
        {
            var nuevoÍndice = índiceActual += 1;
            if (nuevoÍndice >= cancionesActuales.Count)
                nuevoÍndice = 0;

            EnfocarCanción(nuevoÍndice);
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

            listaCanciones.SelectedIndex = -1;
            índiceActual = 0;

            // Datos
            nombreArtista.Text = "Artista";
            nombreCanción.Text = "Canción";
            nombreAlbum.Text = "Álbum";
            imgCarátula.Source = Constantes.ObtenerSinCarátula();
        }

        private void EnClicAleatorio(object sender, RoutedEventArgs e)
        {
            aleatorio = !aleatorio;

            if (aleatorio)
            {
                AleatorizarCanciones();
                índiceActual = 0;
            }
            else
            {
                cancionesActuales = cancionesActuales.OrderBy(o => o.Índice).ToList();
                índiceActual = listaCanciones.SelectedIndex;
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

        private void EnfocarCanción(int nuevoÍndice)
        {
            listaCanciones.SelectedItem = cancionesActuales[nuevoÍndice];
            índiceActual = nuevoÍndice;

            listaCanciones.ScrollIntoView(listaCanciones.SelectedItem);
            pausa = false;
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

        private void SiguienteCanción(object sender, EventArgs e)
        {
            if (repetirCanción)
            {
                EnClicElegirCanción(null, null);
            }
            else
                EnClicSiguiente(null, null);
        }

        private void AleatorizarCanciones()
        {
            var random = new Random();
            cancionesActuales = cancionesActuales.OrderBy(o => random.Next()).ToList();
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

        private void EnClicAgregarAutor(object sender, RoutedEventArgs e)
        {

        }

        private void EnClicElegirCarpeta(object sender, RoutedEventArgs e)
        {
            if (listaCarpetas.SelectedIndex < 0)
                return;

            var carpeta = carpetasActuales[listaCarpetas.SelectedIndex];
            carpetaActual = carpeta;

            AgregarCanciones(carpeta.Ruta);
            ActualizarListaCanciones();

            // PENDIENTE: enfocar si estaba reproduciendo
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

                // Info
                nuevaCanción.Ruta = archivosMúsica[i];
                nuevaCanción.Nombre = tagLib.Tag.Title;
                nuevaCanción.Álbum = tagLib.Tag.Album;
                nuevaCanción.Duración = tagLib.Properties.Duration;
                nuevaCanción.DuraciónFormateada = Constantes.TimeSpanATexto(nuevaCanción.Duración);

                cancionesActuales.Add(nuevaCanción);
            }

            // Crea índice real de canciones
            cancionesActuales = cancionesActuales.OrderBy(o => o.Nombre).ToList();
            cancionesActuales = cancionesActuales.OrderBy(o => o.Autor).ToList();

            for (int i = 0; i < cancionesActuales.Count; i++)
            {
                cancionesActuales[i].Índice = i;
            }

            if (aleatorio)
                AleatorizarCanciones();
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
            nombreAlbum.Text = cancionActual.Álbum;
            duraciónCompleta.Text = Constantes.TimeSpanATexto(cancionActual.Duración);
        }

        private void ActualizarListaCarpeta()
        {
            listaCarpetas.ItemsSource = null;
            listaCarpetas.Items.Clear();
            listaCarpetas.SelectedIndex = -1;
            listaCanciones.SelectedIndex = -1;

            // PENDIENTE: Agregar color a cada tipo
            carpetasActuales = carpetasActuales.OrderBy(o => o.Nombre).ToList();

            for (int i = 0; i < carpetasActuales.Count; i++)
            {
                listaCarpetas.Items.Add(carpetasActuales[i].Nombre);
            }
        }

        private void ActualizarListaCanciones()
        {
            listaCanciones.ItemsSource = null;
            listaCanciones.Items.Clear();
            listaCanciones.SelectedIndex = -1;

            listaCanciones.ItemsSource = cancionesActuales.OrderBy(o => o.Índice);

            if(aleatorio)
                AleatorizarCanciones();
        }
    }
}