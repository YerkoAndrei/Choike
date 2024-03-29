﻿// YerkoAndrei
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

namespace Choike;
using static Constantes;

public partial class MainWindow : Window
{
    private MediaPlayer mediaPlayer;

    private List<Canción> cancionesActuales;
    private List<Carpeta> carpetasActuales;

    private bool pausa;
    private bool parado;
    private bool aleatorio;
    private bool repetirCanción;
    private bool elejidoPorLista;
    private bool moviendoTiempoCanción;
    private bool mirandoTiempoCanción;
    private bool bloquearCambioCanción;

    private bool tamañoTiempoNormalActual;
    private bool tamañoTiempoNormalCompleta;
    private bool tamañoTiempoNormalObjetivo;

    private Carpeta carpetaActual;
    private Canción canciónActual;
    private double volumenAnterior;
    private int índiceActual;

    private Timer contador;
    private Action mostrarEstadoCanción;

    public Color ColorResaltado = Color.FromRgb(200, 200, 100);
    public Brush BrochaResaltado;

    // Tamaños fuentes dinámicas
    public int fuentePrincipal = 155;            // 18

    public int fuenteBotonesControlPequeño = 45; // 52
    public int fuenteBotonesControlGrande = 38;  // 52
    public int fuenteBotonesCarpeta = 80;        // 32
    public int fuenteVolumen = 58;               // 50
    public int fuenteNúmeroVolumen = 170;        // 15

    public int fuenteTiempoActual = 110;         // 25
    public int fuenteTiempoCompleta = 110;       // 25
    public int fuenteTiempoObjetivo = 110;       // 25

    public int fuenteNombreCanción = 85;         // 30
    public int fuenteAutorCanción = 130;         // 20
    public int fuenteÁlbumCanción = 130;         // 20

    // Botones fuera de foco
    private OyenteTeclado oyente;

    public MainWindow()
    {
        InitializeComponent();

        mediaPlayer = new MediaPlayer();

        // Valores predeterminados
        pausa = false;
        parado = true;
        aleatorio = true;
        repetirCanción = false;
        elejidoPorLista = false;
        moviendoTiempoCanción = false;
        índiceActual = 0;

        tamañoTiempoNormalActual = true;
        tamañoTiempoNormalCompleta = true;
        tamañoTiempoNormalObjetivo = true;

        carpetaActual = new Carpeta();
        canciónActual = new Canción();

        cancionesActuales = new List<Canción>();
        carpetasActuales = new List<Carpeta>();

        BrochaResaltado = new SolidColorBrush(ColorResaltado);

        // Volumen predeterminado
        volumen.Value = 0.75;
        mediaPlayer.Volume = volumen.Value;

        // Tiempo canción
        mostrarEstadoCanción = () => { MostrarEstadoCanción(); };

        contador = new Timer
        {
            Interval = 100,
            Enabled = false
        };
        contador.Elapsed += new ElapsedEventHandler(IntervaloTiempo);

        // Interfaz
        MostrarAleatorio();
        MostrarVolumen();
        MostrarRepetir();

        // Carpetas guardadas
        carpetasActuales = CargarCarpetasGuardadas();
        ActualizarListaCarpetas();

        // Escuchar teclado
        oyente = new OyenteTeclado();
        oyente.OnKeyPressed += EnTecla;
        oyente.VincularTeclado();
    }

    private void IntervaloTiempo(object sender, EventArgs e)
    {
        if (parado || pausa)
            return;

        // TaskCanceledException
        try
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(mostrarEstadoCanción);
        }
        catch{ }
    }


    // --- Botones ---


    private void EnClicPausa(object sender, RoutedEventArgs e)
    {
        if (parado)
        {
            // Reproduce carpeta aleatoria
            if (cancionesActuales.Count <= 0)
            {
                var aleatorio = new Random(CalcularSemilla()).Next(0, carpetasActuales.Count);
                carpetaActual = carpetasActuales[aleatorio];

                AgregarCanciones(carpetaActual.Ruta);
                ActualizarListaCanciones();
            }

            // Si hay una carpeta seleccionada
            if (aleatorio)
                AleatorizarCanciones();

            EnfocarCanción(0);
            return;
        }

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
        if (parado)
            return;

        // Mitad final repite canción
        var mitadFinal = (mediaPlayer.Position.TotalSeconds / canciónActual.Duración.TotalSeconds) > 0.5;
        if (mitadFinal)
        {
            EnfocarCanción(índiceActual);
        }
        else
        {
            var nuevoÍndice = índiceActual - 1;
            if (nuevoÍndice < 0)
                nuevoÍndice = cancionesActuales.Count - 1;

            EnfocarCanción(nuevoÍndice);
        }
    }

    private void EnClicSiguiente(object sender, RoutedEventArgs e)
    {
        if (parado)
            return;

        var nuevoÍndice = índiceActual + 1;
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
        if (parado)
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
        imgCarátula.Source = ObtenerSinCarátula();
    }

    private void EnClicAleatorio(object sender, RoutedEventArgs e)
    {
        aleatorio = !aleatorio;
        MostrarAleatorio();

        if (aleatorio)
            AleatorizarCanciones();
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

    private void EnCursorEntraDuración(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            mirandoTiempoCanción = true;
    }

    private void EnCursorFueraDuración(object sender, MouseButtonEventArgs e)
    {
        mirandoTiempoCanción = true;
    }

    private void EnClicDuración(object sender, MouseEventArgs e)
    {
        if (moviendoTiempoCanción || e.LeftButton != MouseButtonState.Pressed || !mirandoTiempoCanción)
            return;

        mirandoTiempoCanción = false;
        EnCambioTiempoCanción(sender, null);
    }

    private void EnCambioTiempoCanción(object sender, DragCompletedEventArgs e)
    {
        if (string.IsNullOrEmpty(canciónActual.Ruta))
            return;

        duraciónObjetivo.Text = string.Empty;
        moviendoTiempoCanción = false;
        mediaPlayer.Position = TimeSpan.FromSeconds(porcentajeDuraciónActual.Value * canciónActual.Duración.TotalSeconds);
    }

    private void EnfocarCanción(int nuevoÍndice)
    {
        // Permite repetir canción
        if(nuevoÍndice == índiceActual)
            listaCanciones.SelectedItem = null;

        listaCanciones.SelectedItem = cancionesActuales[nuevoÍndice];
        índiceActual = nuevoÍndice;

        listaCanciones.ScrollIntoView(listaCanciones.SelectedItem);
        pausa = false;
    }


    // --- Órden canciones ---


    private void EnSeleccionarCanción(object sender, SelectionChangedEventArgs e)
    {
        if (listaCanciones.SelectedIndex < 0)
            return;

        // En cambio de carpeta
        if (bloquearCambioCanción)
        {
            bloquearCambioCanción = false;
            return;
        }

        botónPausa.Text = "⏸";
        duraciónObjetivo.Text = string.Empty;

        var canción = (Canción)listaCanciones.SelectedItem;
        MostrarDatosCanción(canción, canción.Ruta);

        // Elige canción al seleccionar en lista
        if (elejidoPorLista && aleatorio)
        {
            elejidoPorLista = false;
            AleatorizarCanciones();
        }

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
            EnSeleccionarCanción(sender, (SelectionChangedEventArgs)e);
        else
            EnClicSiguiente(sender, (SelectionChangedEventArgs)e);
    }

    private void AleatorizarCanciones()
    {
        var aleatorio = new Random(CalcularSemilla());

        // Primera aleatorización
        if (listaCanciones.SelectedIndex < 0)
        {
            cancionesActuales = cancionesActuales.OrderBy(o => aleatorio.Next()).ToList();
            índiceActual = 0;
            return;
        }

        var canción = (Canción)listaCanciones.SelectedItem;
        var cancionesSinActual = cancionesActuales.Where(o => o.Índice != canción.Índice).ToArray();

        // Aleatorio sin actual
        cancionesSinActual = cancionesSinActual.OrderBy(o => aleatorio.Next()).ToArray();

        // Crea nueva lista con la seleccionada como primera
        var listaFinal = new List<Canción>
        {
            canción
        };
        listaFinal.AddRange(cancionesSinActual);

        cancionesActuales = listaFinal;
        índiceActual = 0;
    }

    // Se reconoce primero el clic antes del cambio de lista
    private void EnClicCanción(object sender, MouseEventArgs e)
    {
        elejidoPorLista = true;
    }

    private int CalcularSemilla()
    {
        var fecha = DateTime.Parse("08-02-1996");
        var tiempo = fecha - DateTime.Now;
        return (int)tiempo.TotalSeconds;
    }


    // --- Carpetas ---


    private void EnClicAgregarCarpeta(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog
        {
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Agregar Carpeta",
            Multiselect = false,
            IsFolderPicker = true
        };

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
            var nuevaCarpeta = new Carpeta
            {
                Tipo = TipoCarpeta.carpeta,
                Ruta = rutaCarpeta,
                Nombre = nombreCarpeta
            };
            nuevaCarpeta.Color = ObtenerColorPorTipoCarpeta(nuevaCarpeta.Tipo);

            carpetasActuales.Add(nuevaCarpeta);
            carpetaActual = nuevaCarpeta;

            // Agregar canciones de carpeta
            ActualizarCarpetasGuardadas(carpetasActuales);
            AgregarCanciones(rutaCarpeta);
            ActualizarListaCarpetas();
        }
    }

    private void EnClicAgregarAutor(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog
        {
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Agregar Autor",
            Multiselect = false
        };

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
            var nuevaCarpeta = new Carpeta
            {
                Tipo = TipoCarpeta.autor,
                Ruta = rutaCarpeta,
                Nombre = nombreAutor
            };
            nuevaCarpeta.Color = ObtenerColorPorTipoCarpeta(nuevaCarpeta.Tipo);

            carpetasActuales.Add(nuevaCarpeta);
            carpetaActual = nuevaCarpeta;

            // Agregar canciones de autor
            ActualizarCarpetasGuardadas(carpetasActuales);
            AgregarCanciones(rutaCarpeta);
            ActualizarListaCarpetas();
        }
    }

    private void EnClicEliminarCarpeta(object sender, RoutedEventArgs e)
    {
        var rutaCarpeta = carpetasActuales[listaCarpetas.SelectedIndex];
        carpetasActuales.Remove(rutaCarpeta);

        ActualizarCarpetasGuardadas(carpetasActuales);
        ActualizarListaCarpetas();
    }

    private void EnClicElegirCarpeta(object sender, RoutedEventArgs e)
    {
        if (listaCarpetas.SelectedIndex < 0)
            return;

        carpetaActual = carpetasActuales[listaCarpetas.SelectedIndex];
        AgregarCanciones(carpetaActual.Ruta);
        ActualizarListaCanciones();

        // Volver a enfocar
        if (string.IsNullOrEmpty(canciónActual.Ruta))
            return;

        if (canciónActual.Ruta.Contains(carpetaActual.Nombre))
        {
            bloquearCambioCanción = true;
            listaCanciones.SelectedIndex = canciónActual.Índice;
            listaCanciones.UpdateLayout();
            listaCanciones.Focus();
            listaCanciones.ScrollIntoView(listaCanciones.SelectedItem);
        }
    }

    private void AgregarCanciones(string carpeta)
    {
        string[] archivosMúsica = Directory.GetFiles(carpeta, extensionesMúsica, enumerationOptions);
        cancionesActuales = new List<Canción>();

        TagLib.File tagLib;
        Canción nuevaCanción;

        var cantidadCanciones = archivosMúsica.Length;
        for (int i = 0; i < cantidadCanciones; i++)
        {
            try
            {
                tagLib = TagLib.File.Create(archivosMúsica[i]);
            }
            catch
            {
                continue;
            }

            nuevaCanción = new Canción();

            // Artista
            if (tagLib.Tag.Performers.Length > 0)
                nuevaCanción.Autor = tagLib.Tag.Performers[0];

            // Multiples artistas
            if (tagLib.Tag.Performers.Length > 1)
            {
                for (int ii = 1; ii < tagLib.Tag.Performers.Length; ii++)
                {
                    nuevaCanción.Autor += " & " + tagLib.Tag.Performers[ii];
                }
            }

            // Si solo busca autor
            if (carpetaActual.Tipo == TipoCarpeta.autor)
            {
                if (!nuevaCanción.Autor.Contains(carpetaActual.Nombre))
                    continue;
            }

            // Info
            nuevaCanción.Ruta = archivosMúsica[i];
            nuevaCanción.Nombre = tagLib.Tag.Title;
            nuevaCanción.Álbum = tagLib.Tag.Album;
            nuevaCanción.Detalles = tagLib.Properties.AudioBitrate + " kbps";
            nuevaCanción.Duración = tagLib.Properties.Duration;
            nuevaCanción.DuraciónFormateada = TimeSpanATexto(nuevaCanción.Duración);

            // Si no puede leer data
            if (string.IsNullOrEmpty(nuevaCanción.Nombre) && string.IsNullOrEmpty(nuevaCanción.Autor))
                nuevaCanción.Nombre = archivosMúsica[i].Split('\\')[^1];

            cancionesActuales.Add(nuevaCanción);
        }

        // Crea índice real de canciones
        cancionesActuales = cancionesActuales.OrderBy(o => o.Nombre).ToList();
        cancionesActuales = cancionesActuales.OrderBy(o => o.Autor).ToList();

        for (int i = 0; i < cancionesActuales.Count; i++)
        {
            cancionesActuales[i].Índice = i;
        }
    }


    // --- Interfaz ---


    private void MostrarEstadoCanción()
    {
        duraciónActual.Text = TimeSpanATexto(mediaPlayer.Position);
       
        // Más de una hora
        if (duraciónActual.Text.Length > 5 && tamañoTiempoNormalActual)
            EnCambioTamaño(null, null);
        else if (duraciónActual.Text.Length <= 5 && !tamañoTiempoNormalActual)
            EnCambioTamaño(null, null);
        
        if (moviendoTiempoCanción)
        {
            duraciónObjetivo.Text = TimeSpanATexto(canciónActual.Duración * porcentajeDuraciónActual.Value);
            
            // Más de una hora
            if (duraciónObjetivo.Text.Length > 5 && tamañoTiempoNormalObjetivo)
                EnCambioTamaño(null, null);
            else if (duraciónObjetivo.Text.Length <= 5 && !tamañoTiempoNormalObjetivo)
                EnCambioTamaño(null, null);
            return;
        }

        porcentajeDuraciónActual.Value = (mediaPlayer.Position.TotalSeconds / canciónActual.Duración.TotalSeconds);
    }

    private void MostrarDatosCanción(Canción canción, string nombreArchivo)
    {
        canciónActual = canción;

        // Carátula
        TagLib.File tagLibFile = TagLib.File.Create(nombreArchivo);

        var imágenes = tagLibFile.Tag.Pictures;

        // Revisa si tiene carátula
        if (imágenes.Length > 0 && imágenes[0].Data.Data != null)
        {
            var imagenÁlbum = (byte[])(imágenes[0].Data.Data);
            imgCarátula.Source = ByteAImagen(imagenÁlbum);
            colorCanción.Color = ObtenerColorDominante(imagenÁlbum);
        }
        else
        {
            imgCarátula.Source = ObtenerSinCarátula();
            colorCanción.Color = ObtenerColorGris();
        }

        // Datos
        nombreAutor.Text = canciónActual.Autor;
        nombreCanción.Text = canciónActual.Nombre;
        nombreAlbum.Text = canciónActual.Álbum;
        nombreDetalles.Text = canciónActual.Detalles;
        duraciónCompleta.Text = TimeSpanATexto(canciónActual.Duración);

        // Más de una hora
        if (duraciónCompleta.Text.Length > 5 && tamañoTiempoNormalCompleta)
            EnCambioTamaño(null, null);
        else if(duraciónCompleta.Text.Length <= 5 && !tamañoTiempoNormalCompleta)
            EnCambioTamaño(null, null);
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

        ContarCancionesEnCarpeta();
    }

    private void ContarCancionesEnCarpeta()
    {
        var duracion = new TimeSpan();

        for(int i=0; i < cancionesActuales.Count; i++)
        {
            duracion += cancionesActuales[i].Duración;
        }

        cantidadCanciones.Text = cancionesActuales.Count.ToString() + " Canciones";
        duraciónCanciones.Text = TimeSpanATexto(duracion);
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

    private void BloquearBotones(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.PageUp || e.Key == Key.PageDown)
            e.Handled = true;
    }

    // Barra título
    private void EnClicMinimizar(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void EnClicMaximizar(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        else
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
    }

    private void EnClicCerrar(object sender, RoutedEventArgs e)
    {
        mediaPlayer.Stop();
        oyente.DesvincularTeclado();

        Application.Current.Shutdown();
    }

    public void EnCambioTamaño(object sender, SizeChangedEventArgs e)
    {
        var pantallaActual = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);

        // Diferencia bordes
        Application.Current.MainWindow.MaxHeight = pantallaActual.WorkingArea.Height + 14;
        Application.Current.MainWindow.MaxWidth = pantallaActual.WorkingArea.Width + 14;

        double anchoPantalla = 0;
        var multiplicador = 1.35;

        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
        {
            anchoPantalla = Application.Current.MainWindow.MaxWidth + Application.Current.MainWindow.MaxHeight;
            Application.Current.MainWindow.BorderThickness = new Thickness(7);
            botónMaximizar.Text = "🗗";
        }
        else
        {
            anchoPantalla = Width + Height;
            botónMaximizar.Text = "🗖";
        }
        
        // Fuentes
        Application.Current.Resources.Remove("fuentePrincipal");

        Application.Current.Resources.Remove("fuenteBotonesControlPequeño");
        Application.Current.Resources.Remove("fuenteBotonesControlGrande");
        Application.Current.Resources.Remove("fuenteBotonesCarpeta");
        Application.Current.Resources.Remove("fuenteVolumen");
        Application.Current.Resources.Remove("fuenteNúmeroVolumen");

        Application.Current.Resources.Remove("fuenteNombreCanción");
        Application.Current.Resources.Remove("fuenteAutorCanción");
        Application.Current.Resources.Remove("fuenteÁlbumCanción");

        // Cambio tamaño fuentes
        Application.Current.Resources.Add("fuentePrincipal", Math.Clamp(((anchoPantalla / fuentePrincipal) * multiplicador), 5, FontSize));

        Application.Current.Resources.Add("fuenteBotonesControlPequeño", (anchoPantalla / fuenteBotonesControlPequeño) * multiplicador);
        Application.Current.Resources.Add("fuenteBotonesControlGrande", (anchoPantalla / fuenteBotonesControlGrande) * multiplicador);
        Application.Current.Resources.Add("fuenteBotonesCarpeta", Math.Clamp(((anchoPantalla / fuenteBotonesCarpeta) * multiplicador), 5, 40));
        Application.Current.Resources.Add("fuenteVolumen",        (anchoPantalla / fuenteVolumen) * multiplicador);
        Application.Current.Resources.Add("fuenteNúmeroVolumen",  (anchoPantalla / fuenteNúmeroVolumen) * multiplicador);
        
        Application.Current.Resources.Add("fuenteNombreCanción",  (anchoPantalla / fuenteNombreCanción) * multiplicador);
        Application.Current.Resources.Add("fuenteAutorCanción",  (anchoPantalla / fuenteAutorCanción) * multiplicador);
        Application.Current.Resources.Add("fuenteÁlbumCanción",  (anchoPantalla / fuenteÁlbumCanción) * multiplicador);

        // Cambio tamaño tiempos
        Application.Current.Resources.Remove("fuenteTiempoActual");
        Application.Current.Resources.Remove("fuenteTiempoCompleta");
        Application.Current.Resources.Remove("fuenteTiempoObjetivo");

        if (duraciónCompleta.Text.Length > 5)
        {
            tamañoTiempoNormalCompleta = false;
            Application.Current.Resources.Add("fuenteTiempoCompleta", (anchoPantalla / (fuenteTiempoCompleta * 1.3)) * multiplicador);
        }
        else
        {
            tamañoTiempoNormalCompleta = true;
            Application.Current.Resources.Add("fuenteTiempoCompleta", (anchoPantalla / fuenteTiempoCompleta) * multiplicador);
        }

        if (duraciónActual.Text.Length > 5)
        {
            tamañoTiempoNormalActual = false;
            Application.Current.Resources.Add("fuenteTiempoActual", (anchoPantalla / (fuenteTiempoActual * 1.3)) * multiplicador);
        }
        else
        {
            tamañoTiempoNormalActual = true;
            Application.Current.Resources.Add("fuenteTiempoActual", (anchoPantalla / fuenteTiempoActual) * multiplicador);
        }

        if (duraciónObjetivo.Text.Length > 5)
        {
            tamañoTiempoNormalObjetivo = false;
            Application.Current.Resources.Add("fuenteTiempoObjetivo", (anchoPantalla / (fuenteTiempoObjetivo * 1.3)) * multiplicador);
        }
        else
        {
            tamañoTiempoNormalObjetivo = true;
            Application.Current.Resources.Add("fuenteTiempoObjetivo", (anchoPantalla / fuenteTiempoObjetivo) * multiplicador);
        }
    }

    // --- Botones fuera de foco ---

    void EnTecla(object sender, KeyPressedArgs e)
    {
        switch (e.KeyPressed)
        {
            case Key.MediaPlayPause:
            case Key.Pause:
                EnClicPausa(sender, routedEvent);
                break;
            case Key.MediaNextTrack:
                EnClicSiguiente(sender, routedEvent);
                break;
            case Key.MediaPreviousTrack:
                EnClicAnterior(sender, routedEvent);
                break;
            case Key.MediaStop:
                EnClicDetener(sender, routedEvent);
                break;
            case Key.VolumeMute:
                EnClicSilencio(sender, routedEvent);
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
                EnClicAleatorio(sender, routedEvent);
                break;
            case Key.F10:
                EnClicRepetir(sender, routedEvent);
                break;
        }
    }
}
