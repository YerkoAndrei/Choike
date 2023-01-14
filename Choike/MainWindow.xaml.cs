// YerkoAndrei
using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;
using Choike.Clases;

namespace Choike
{
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer;

        private List<Canción> cancionesActuales;
        private List<Carpeta> carpetasActuales;

        private bool pausa;
        private bool parado;
        private bool aleatorio;
        private bool repetirCanción;
        private bool moviendoTiempoCanción;

        private Carpeta carpetaActual;
        private Canción cancionActual;
        private double volumenAnterior;
        private int índiceActual;

        private Timer contador;
        private Action mostrarEstadoCanción;

        public Color ColorBase = Color.FromRgb(120, 120, 100);
        public Color ColorResaltado = Color.FromRgb(200, 200, 100);
        public Brush BrochaResaltado;

        public double fuentePrincipal = 155;        // 18

        public double fuenteBotonesControl = 50;    // 52
        public double fuenteBotonesCarpeta = 80;    // 32
        public double fuenteVolumen = 58;           // 50
        public double fuenteNúmeroVolumen = 170;    // 15
        public double fuenteTiempoCanción = 110;    // 25

        public double fuenteNombreCanción = 85;     // 30
        public double fuenteAutorCanción = 130;     // 20
        public double fuenteÁlbumCanción = 130;     // 20

        public MainWindow()
        {
            // PENDIENTE
            // botones sin mirar ventana

            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            // Valores predeterminados
            pausa = false;
            parado = true;
            aleatorio = true;
            repetirCanción = false;
            moviendoTiempoCanción = false;
            índiceActual = 0;

            carpetaActual = new Carpeta();
            cancionActual = new Canción();

            cancionesActuales = new List<Canción>();
            carpetasActuales = new List<Carpeta>();

            BrochaResaltado = (SolidColorBrush)new BrushConverter().ConvertFrom(ColorResaltado.ToString());

            // Volumen predeterminado
            volumen.Value = 0.75;
            mediaPlayer.Volume = volumen.Value;

            // Tiempo canción
            mostrarEstadoCanción = () =>{ MostrarEstadoCanción(); };

            contador = new Timer();
            contador.Interval = 100;
            contador.Enabled = false;
            contador.Elapsed += new ElapsedEventHandler(IntervaloTiempo);

            // Interfaz
            MostrarAleatorio();
            MostrarVolumen();
            MostrarRepetir();

            // Carpetas guardadas
            carpetasActuales = Constantes.CargarCarpetasGuardadas();
            ActualizarListaCarpetas();
        }

        private void IntervaloTiempo(object sender, EventArgs e)
        {
            if(Application.Current != null)
                Application.Current.Dispatcher.Invoke(mostrarEstadoCanción);
        }


        // --- Botones ---


        private void EnTecla(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.MediaPlayPause:
                case Key.Pause:
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
                    volumen.Value += volumen.LargeChange;
                    mediaPlayer.Volume = volumen.Value;
                    break;
                case Key.PageDown:
                    volumen.Value -= volumen.LargeChange;
                    mediaPlayer.Volume = volumen.Value;
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
            if (parado)
                return;

            if (pausa)
                mediaPlayer.Play();
            else
                mediaPlayer.Pause();

            pausa = !pausa;

            if (pausa)
                botónPausa.Text = "⏵";
            else
                botónPausa.Text = "⏸";
        }

        private void EnClicAnterior(object sender, RoutedEventArgs e)
        {
            if (listaCanciones.SelectedIndex < 0)
                return;

            var nuevoÍndice = índiceActual - 1;
            if (nuevoÍndice < 0)
                nuevoÍndice = cancionesActuales.Count - 1;

            EnfocarCanción(nuevoÍndice);
        }

        private void EnClicSiguiente(object sender, RoutedEventArgs e)
        {
            if (listaCanciones.SelectedIndex < 0)
                return;

            var nuevoÍndice = índiceActual += 1;
            if (nuevoÍndice >= cancionesActuales.Count)
                nuevoÍndice = 0;

            EnfocarCanción(nuevoÍndice);
        }

        private void EnClicSilencio(object sender, RoutedEventArgs e)
        {
            if (volumen.Value > 0)
            {
                volumenAnterior = volumen.Value;
                volumen.Value = 0;
            }
            else
                volumen.Value = volumenAnterior;

            mediaPlayer.Volume = volumen.Value;
            MostrarVolumen();
        }

        private void EnClicDetener(object sender, RoutedEventArgs e)
        {
            if (listaCanciones.SelectedIndex < 0)
                return;

            parado = !parado;

            mediaPlayer.Stop();
            contador.Stop();

            duraciónActual.Text = "00:00";
            duraciónCompleta.Text = "00:00";
            duraciónObjetivo.Text = string.Empty;

            porcentajeDuraciónActual.Value = 0;
            botónPausa.Text = "⏯";
            listaCanciones.SelectedIndex = -1;
            índiceActual = 0;

            // Datos
            nombreCanción.Text = "Canción";
            nombreAutor.Text = "Autor";
            nombreAlbum.Text = "Álbum";
            nombreDetalles.Text = string.Empty;
            imgCarátula.Source = Constantes.ObtenerSinCarátula();
        }

        private void EnClicAleatorio(object sender, RoutedEventArgs e)
        {
            aleatorio = !aleatorio;
            MostrarAleatorio();

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
            MostrarRepetir();
        }

        private void EnCambioVolumen(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumen.Value;
            MostrarVolumen();
        }

        private void EnMoverTiempoCanción(object sender, DragStartedEventArgs e)
        {
            if (parado)
                return;

            moviendoTiempoCanción = true;
        }

        private void EnCambioTiempoCanción(object sender, DragCompletedEventArgs e)
        {
            if (string.IsNullOrEmpty(cancionActual.Ruta))
                return;

            duraciónObjetivo.Text = string.Empty;
            moviendoTiempoCanción = false;
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

            botónPausa.Text = "⏸";
            duraciónObjetivo.Text = string.Empty;
            var canción = (Canción)listaCanciones.SelectedItem;
            MostrarDatosCanción(canción, canción.Ruta);

            // Reproducción
            mediaPlayer.Open(new Uri(canción.Ruta));
            mediaPlayer.MediaEnded += new EventHandler(SiguienteCanción);
            mediaPlayer.Play();
            contador.Start();
            pausa = false;
            parado = false;
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
            dialog.Title = "Agregar Carpeta";
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

                // Evita repetir carpetas
                if (carpetasActuales.Any(o => o.Ruta == rutaCarpeta))
                    return;

                // Agregar carpeta seleccionada
                var nuevaCarpeta = new Carpeta();
                nuevaCarpeta.Tipo = Constantes.TipoCarpeta.carpeta;
                nuevaCarpeta.Ruta = rutaCarpeta;
                nuevaCarpeta.Nombre = nombreCarpeta;
                nuevaCarpeta.Color = Constantes.ObtenerColorPorTipoCarpeta(nuevaCarpeta.Tipo);

                carpetasActuales.Add(nuevaCarpeta);
                carpetaActual = nuevaCarpeta;

                // Agregar canciones de carpeta
                Constantes.ActualizarCarpetasGuardadas(carpetasActuales);
                AgregarCanciones(rutaCarpeta);
                ActualizarListaCarpetas();
            }
        }

        private void EnClicAgregarAutor(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.Title = "Agregar Autor";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var rutaArchivo = dialog.FileName;
                var rutaCortada = rutaArchivo.Split('\\');
                var rutaCarpeta = rutaArchivo.Replace(rutaCortada[rutaCortada.Length - 1], string.Empty);

                // Si la carpeta existe
                if (!Directory.Exists(rutaCarpeta))
                    return;

                // Si contiene autores
                var tagLib = TagLib.File.Create(rutaArchivo);
                if (tagLib.Tag.Performers.Length <= 0)
                    return;

                var nombreAutor = tagLib.Tag.Performers[0];

                // Evita repetir autor
                if (carpetasActuales.Any(o => o.Nombre == nombreAutor))
                    return;

                // Agregar autor seleccionado
                var nuevaCarpeta = new Carpeta();
                nuevaCarpeta.Tipo = Constantes.TipoCarpeta.autor;
                nuevaCarpeta.Ruta = rutaCarpeta;
                nuevaCarpeta.Nombre = nombreAutor;
                nuevaCarpeta.Color = Constantes.ObtenerColorPorTipoCarpeta(nuevaCarpeta.Tipo);

                carpetasActuales.Add(nuevaCarpeta);
                carpetaActual = nuevaCarpeta;

                // Agregar canciones de autor
                Constantes.ActualizarCarpetasGuardadas(carpetasActuales);
                AgregarCanciones(rutaCarpeta);
                ActualizarListaCarpetas();
            }
        }

        private void EnClicEliminarCarpeta(object sender, RoutedEventArgs e)
        {
            var rutaCarpeta = carpetasActuales[listaCarpetas.SelectedIndex];
            carpetasActuales.Remove(rutaCarpeta);

            Constantes.ActualizarCarpetasGuardadas(carpetasActuales);
            ActualizarListaCarpetas();
        }

        private void EnClicElegirCarpeta(object sender, RoutedEventArgs e)
        {
            if (listaCarpetas.SelectedIndex < 0)
                return;

            var carpeta = carpetasActuales[listaCarpetas.SelectedIndex];
            carpetaActual = carpeta;

            AgregarCanciones(carpeta.Ruta);
            ActualizarListaCanciones();

            // Volver a enfocar
            if (string.IsNullOrEmpty(cancionActual.Ruta))
                return;

            if (cancionActual.Ruta.Contains(carpetaActual.Nombre))
            {
                // PENDIENTE
                //listaCanciones.SelectedItem = cancionesActuales[índiceActual];
                listaCanciones.ScrollIntoView(listaCanciones.SelectedItem);
            }
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

                // Si solo busca autor
                if (carpetaActual.Tipo == Constantes.TipoCarpeta.autor)
                {
                    if (!nuevaCanción.Autor.Contains(carpetaActual.Nombre))
                        continue;
                }

                // Info
                nuevaCanción.Ruta = archivosMúsica[i];
                nuevaCanción.Nombre = tagLib.Tag.Title;
                nuevaCanción.Álbum = tagLib.Tag.Album;
                nuevaCanción.Detalles = tagLib.Properties.AudioBitrate + "kbps";
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


        private void MostrarEstadoCanción()
        {
            if (parado)
                return;

            duraciónActual.Text = Constantes.TimeSpanATexto(mediaPlayer.Position);

            if (moviendoTiempoCanción)
            {
                duraciónObjetivo.Text = Constantes.TimeSpanATexto(cancionActual.Duración * porcentajeDuraciónActual.Value);
                return;
            }

            porcentajeDuraciónActual.Value = (mediaPlayer.Position.TotalSeconds / cancionActual.Duración.TotalSeconds);
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
                colorCanción.Color = Constantes.ObtenerColorDominante(imagenÁlbum);
            }
            else
            {
                imgCarátula.Source = Constantes.ObtenerSinCarátula();
                colorCanción.Color = Constantes.ObtenerColorGris();
            }

            // Datos
            nombreAutor.Text = cancionActual.Autor;
            nombreCanción.Text = cancionActual.Nombre;
            nombreAlbum.Text = cancionActual.Álbum;
            nombreDetalles.Text = cancionActual.Detalles;
            duraciónCompleta.Text = Constantes.TimeSpanATexto(cancionActual.Duración);
        }

        private void ActualizarListaCarpetas()
        {
            listaCarpetas.ItemsSource = null;
            listaCarpetas.Items.Clear();
            listaCarpetas.SelectedIndex = -1;
            listaCanciones.SelectedIndex = -1;

            carpetasActuales = carpetasActuales.OrderBy(o => o.Nombre).ToList();
            carpetasActuales = carpetasActuales.OrderBy(o => o.Tipo).ToList();

            listaCarpetas.ItemsSource = carpetasActuales;
        }


        private void ActualizarListaCanciones()
        {
            listaCanciones.ItemsSource = null;
            listaCanciones.Items.Clear();
            listaCanciones.SelectedIndex = -1;

            listaCanciones.ItemsSource = cancionesActuales.OrderBy(o => o.Índice);

            if(aleatorio)
                AleatorizarCanciones();

            ContrarCancionesEnCarpeta();
        }

        private void ContrarCancionesEnCarpeta()
        {
            var duracion = new TimeSpan();

            for(int i=0; i < cancionesActuales.Count; i++)
            {
                duracion += cancionesActuales[i].Duración;
            }

            cantidadCanciones.Text = cancionesActuales.Count.ToString() + " Canciones";
            duraciónCanciones.Text = Constantes.TimeSpanATexto(duracion);
        }

        private void MostrarVolumen()
        {
            volumenActual.Text = (volumen.Value * 100).ToString("00");

            if (volumen.Value >= 0.5)
                botónSilencio.Text = "🔊";
            else if (volumen.Value > 0)
                botónSilencio.Text = "🔉";
            else
                botónSilencio.Text = "🔇";
        }


        private void MostrarAleatorio()
        {
            if (aleatorio)
                botónAleatorio.Foreground = BrochaResaltado;
            else
                botónAleatorio.Foreground = Brushes.Black;
        }

        private void MostrarRepetir()
        {
            if (repetirCanción)
                botónRepetir.Foreground = BrochaResaltado;
            else
                botónRepetir.Foreground = Brushes.Black;
        }

        // Barra título
        private void EnClicMinimizar(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void EnClicMaximizar(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                botónMaximizar.Text = "🗖";
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                botónMaximizar.Text = "🗗";
            }
        }

        private void EnClicCerrar(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            Application.Current.Shutdown();
        }

        public void EnCambioTamaño(object sender, SizeChangedEventArgs e)
        {
            var pantallaActual = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);

            // Diferencia bordes
            Application.Current.MainWindow.MaxHeight = pantallaActual.WorkingArea.Height + 14;
            Application.Current.MainWindow.MaxWidth = pantallaActual.WorkingArea.Width + 14;

            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.BorderThickness = new Thickness(7);
                botónMaximizar.Text = "🗗";
            }
            else
                botónMaximizar.Text = "🗖";
            
            // Fuentes
            Application.Current.Resources.Remove("fuentePrincipal");

            Application.Current.Resources.Remove("fuenteBotonesControl");
            Application.Current.Resources.Remove("fuenteBotonesCarpeta");
            Application.Current.Resources.Remove("fuenteVolumen");
            Application.Current.Resources.Remove("fuenteNúmeroVolumen");
            Application.Current.Resources.Remove("fuenteTiempoCanción");

            Application.Current.Resources.Remove("fuenteNombreCanción");
            Application.Current.Resources.Remove("fuenteAutorCanción");
            Application.Current.Resources.Remove("fuenteÁlbumCanción");

            var anchoPantalla = Width + Height;
            var multiplicador = 1.35;

            Application.Current.Resources.Add("fuentePrincipal", Math.Clamp(((anchoPantalla / fuentePrincipal) * multiplicador), 5, FontSize));

            Application.Current.Resources.Add("fuenteBotonesControl", (anchoPantalla / fuenteBotonesControl) * multiplicador);
            Application.Current.Resources.Add("fuenteBotonesCarpeta", Math.Clamp(((anchoPantalla / fuenteBotonesCarpeta) * multiplicador), 5, 40));
            Application.Current.Resources.Add("fuenteVolumen",        (anchoPantalla / fuenteVolumen) * multiplicador);
            Application.Current.Resources.Add("fuenteNúmeroVolumen",  (anchoPantalla / fuenteNúmeroVolumen) * multiplicador);
            Application.Current.Resources.Add("fuenteTiempoCanción",  (anchoPantalla / fuenteTiempoCanción) * multiplicador);
            
            Application.Current.Resources.Add("fuenteNombreCanción",  (anchoPantalla / fuenteNombreCanción) * multiplicador);
            Application.Current.Resources.Add("fuenteAutorCanción",  (anchoPantalla / fuenteAutorCanción) * multiplicador);
            Application.Current.Resources.Add("fuenteÁlbumCanción",  (anchoPantalla / fuenteÁlbumCanción) * multiplicador);
        }
    }
}
