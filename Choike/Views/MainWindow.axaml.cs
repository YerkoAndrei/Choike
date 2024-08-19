using Avalonia.Controls;
using Avalonia.Interactivity;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
//using System.Windows;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Interop;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using Microsoft.WindowsAPICodePack.Dialogs;
using Timer = System.Timers.Timer;

namespace Choike.Views;
using static Constantes;

public partial class MainWindow : Window
{
    private static bool Pausa;
    private static bool Parado;
    private static bool Aleatorio;
    private static bool RepetirCanción;
    private static bool ElejidoPorLista;
    private static bool MoviendoTiempoCanción;
    private static bool MirandoTiempoCanción;
    private static bool BloquearCambioCanción;

    private static bool TamañoTiempoNormalActual;
    private static bool TamañoTiempoNormalCompleta;
    private static bool TamañoTiempoNormalObjetivo;

    private static int ÍndiceAleatorioActual;
    private static double VolumenAnterior;

    private MediaPlayer mediaPlayer;

    private List<Canción> cancionesActuales;
    private List<Carpeta> carpetasActuales;

    private Canción canciónActual;
    private Carpeta carpetaActual;
    //private OyenteTeclado oyente;
    private Action mostrarEstadoCanción;
    private Timer contador;

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

    private LibVLC VLC;

    public MainWindow()
    {
        InitializeComponent();
        VLC = new LibVLC();
        mediaPlayer = new MediaPlayer(VLC);

        // Valores predeterminados
        Pausa = false;
        Parado = true;
        Aleatorio = true;
        RepetirCanción = false;
        ElejidoPorLista = false;
        //MoviendoTiempoCanción = false;
        ÍndiceAleatorioActual = 0;

        TamañoTiempoNormalActual = true;
        TamañoTiempoNormalCompleta = true;
        TamañoTiempoNormalObjetivo = true;

        carpetaActual = new Carpeta();
        canciónActual = new Canción();

        cancionesActuales = new List<Canción>();
        carpetasActuales = new List<Carpeta>();

        // Volumen predeterminado
        volumen.Value = 0.8;
        mediaPlayer.Volume = (int)(volumen.Value * 100);

        // Tiempo canción
        contador = new Timer
        {
            Interval = 100,
            Enabled = false
        };
        mostrarEstadoCanción = MostrarEstadoCanción;
        contador.Elapsed += new ElapsedEventHandler(IntervaloTiempo);

        // Interfaz
        MostrarAleatorio();
        MostrarVolumen();
        MostrarRepetir();

        // Carpetas guardadas
        carpetasActuales = CargarCarpetasGuardadas();
        ActualizarListaCarpetas();

        // Escuchar teclado
        //oyente = new OyenteTeclado(EnTecla);
    }

    private void IntervaloTiempo(object? sender, EventArgs? e)
    {
        if (Parado || Pausa)
            return;

        // TaskCanceledException
        /*try
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(mostrarEstadoCanción);
        }
        catch { }*/
    }


    // --- Botones ---


    private void EnClicPausa(object? sender, RoutedEventArgs e)
    {
        if (Parado)
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
            if (Aleatorio)
                AleatorizarCanciones();

            EnfocarCanción(0);
            return;
        }

        if (Pausa)
        {
            mediaPlayer.Play();
            botónPausa.Text = "⏸";
        }
        else
        {
            mediaPlayer.Pause();
            botónPausa.Text = "⏵";
        }

        Pausa = !Pausa;
    }

    private void EnClicAnterior(object? sender, RoutedEventArgs e)
    {
        if (Parado)
            return;

        // Mitad final repite canción
        if (mediaPlayer.Position > 0.5)
        {
            EnfocarCanción(ÍndiceAleatorioActual);
        }
        else
        {
            int nuevoÍndice;
            if (Aleatorio)
                nuevoÍndice = ÍndiceAleatorioActual - 1;
            else
                nuevoÍndice = canciónActual.Índice - 1;

            if (nuevoÍndice < 0)
                nuevoÍndice = cancionesActuales.Count - 1;

            EnfocarCanción(nuevoÍndice);
        }
    }

    private void EnClicSiguiente(object? sender, RoutedEventArgs? e)
    {
        if (Parado)
            return;

        int nuevoÍndice;
        if (Aleatorio)
            nuevoÍndice = ÍndiceAleatorioActual + 1;
        else
            nuevoÍndice = canciónActual.Índice + 1;

        if (nuevoÍndice >= cancionesActuales.Count)
            nuevoÍndice = 0;

        EnfocarCanción(nuevoÍndice);
    }

    private void EnClicSilencio(object? sender, RoutedEventArgs e)
    {
        if (volumen.Value > 0)
        {
            VolumenAnterior = volumen.Value;
            volumen.Value = 0;
        }
        else
            volumen.Value = VolumenAnterior;

        mediaPlayer.Volume = (int)(volumen.Value * 100);
        MostrarVolumen();
    }

    private void EnClicDetener(object? sender, RoutedEventArgs e)
    {
        if (Parado)
            return;

        Parado = !Parado;

        mediaPlayer.Stop();
        contador.Stop();

        duraciónActual.Text = "00:00";
        duraciónCompleta.Text = "00:00";
        duraciónObjetivo.Text = string.Empty;

        porcentajeDuraciónActual.Value = 0;
        botónPausa.Text = "⏯";
        listaCanciones.SelectedIndex = -1;
        ÍndiceAleatorioActual = 0;

        // Datos
        nombreCanción.Text = "Canción";
        nombreAutor.Text = "Autor";
        nombreAlbum.Text = "Álbum";
        nombreDetalles.Text = string.Empty;
        imgCarátula.Source = ObtenerSinCarátula();
    }

    private void EnClicAleatorio(object? sender, RoutedEventArgs e)
    {
        Aleatorio = !Aleatorio;
        MostrarAleatorio();

        if (Aleatorio)
            AleatorizarCanciones();
        else
        {
            cancionesActuales = cancionesActuales.OrderBy(o => o.Índice).ToList();
            ÍndiceAleatorioActual = listaCanciones.SelectedIndex;
        }
    }

    private void EnClicRepetir(object? sender, RoutedEventArgs e)
    {
        RepetirCanción = !RepetirCanción;
        MostrarRepetir();
    }
    /*
    private void EnCambioVolumen(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        mediaPlayer.Volume = (int)(volumen.Value * 100);
        MostrarVolumen();
    }
    
    private void EnMoverTiempoCanción(object sender, DragStartedEventArgs e)
    {
        if (Parado)
            return;

        MoviendoTiempoCanción = true;
    }

    private void EnCursorEntraDuración(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            MirandoTiempoCanción = true;
    }

    private void EnCursorFueraDuración(object sender, MouseButtonEventArgs e)
    {
        MirandoTiempoCanción = true;
    }

    private void EnClicDuración(object sender, MouseEventArgs e)
    {
        if (MoviendoTiempoCanción || e.LeftButton != MouseButtonState.Pressed || !MirandoTiempoCanción)
            return;

        MirandoTiempoCanción = false;
        EnCambioTiempoCanción(sender, null);
    }

    private void EnCambioTiempoCanción(object sender, DragCompletedEventArgs? e)
    {
        if (string.IsNullOrEmpty(canciónActual.Ruta))
            return;

        duraciónObjetivo.Text = string.Empty;
        MoviendoTiempoCanción = false;
        mediaPlayer.Position = TimeSpan.FromSeconds(porcentajeDuraciónActual.Value * canciónActual.Duración.TotalSeconds);
    }*/

    private void EnfocarCanción(int nuevoÍndice)
    {
        // Permite repetir canción
        if (nuevoÍndice == ÍndiceAleatorioActual)
            listaCanciones.SelectedItem = null;

        listaCanciones.SelectedItem = cancionesActuales[nuevoÍndice];
        ÍndiceAleatorioActual = nuevoÍndice;

        listaCanciones.ScrollIntoView(listaCanciones.SelectedItem);
        Pausa = false;
    }


    // --- Órden canciones ---


    private void EnSeleccionarCanción(object? sender, SelectionChangedEventArgs? e)
    {
        if (listaCanciones.SelectedIndex < 0)
            return;

        // En cambio de carpeta
        if (BloquearCambioCanción)
        {
            BloquearCambioCanción = false;
            return;
        }

        botónPausa.Text = "⏸";
        duraciónObjetivo.Text = string.Empty;

        var canción = (Canción)listaCanciones.SelectedItem;
        MostrarDatosCanción(canción, canción.Ruta);

        // Elige canción al seleccionar en lista
        if (ElejidoPorLista && Aleatorio)
        {
            ElejidoPorLista = false;
            AleatorizarCanciones();
        }

        // Reproducción
        mediaPlayer.Media = new Media(VLC, canción.Ruta);
        mediaPlayer.EndReached += SiguienteCanción;
        mediaPlayer.Play();
        contador.Start();
        Pausa = false;
        Parado = false;
    }

    private void SiguienteCanción(object? sender, EventArgs? e)
    {
        if (RepetirCanción)
            EnSeleccionarCanción(sender, null);
        else
            EnClicSiguiente(sender, null);
    }

    private void AleatorizarCanciones()
    {
        var aleatorio = new Random(CalcularSemilla());

        // Primera aleatorización
        if (listaCanciones.SelectedIndex < 0)
        {
            cancionesActuales = cancionesActuales.OrderBy(o => aleatorio.Next()).ToList();
            ÍndiceAleatorioActual = 0;
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
        ÍndiceAleatorioActual = 0;
    }
    /*
    // Se reconoce primero el clic antes del cambio de lista
    private void EnClicCanción(object sender, MouseEventArgs e)
    {
        ElejidoPorLista = true;
    }*/


    // --- Carpetas ---


    private async void EnClicAgregarCarpeta(object sender, RoutedEventArgs e)
    {
        var diálogo = new OpenFolderDialog
        {
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Agregar Carpeta"
        };

        var rutaCarpeta = await diálogo.ShowAsync(this);
        if (string.IsNullOrEmpty(rutaCarpeta))
            return;

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

    private async void EnClicAgregarAutor(object sender, RoutedEventArgs e)
    {
        var diálogo = new OpenFileDialog
        {
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Agregar Autor",
            AllowMultiple = false
        };

        var resultado = await diálogo.ShowAsync(this);
        var rutaArchivo = resultado[0];
        if (string.IsNullOrEmpty(rutaArchivo))
            return;

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

    private void EnClicEliminarCarpeta(object sender, RoutedEventArgs e)
    {
        var rutaCarpeta = carpetasActuales[listaCarpetas.SelectedIndex];
        carpetasActuales.Remove(rutaCarpeta);

        ActualizarCarpetasGuardadas(carpetasActuales);
        ActualizarListaCarpetas();
    }

    private void EnClicSeleccionarCarpeta(object sender, SelectionChangedEventArgs e)
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
            BloquearCambioCanción = true;
            listaCanciones.SelectedIndex = canciónActual.Índice;
            listaCanciones.UpdateLayout();
            listaCanciones.Focus();
            listaCanciones.ScrollIntoView(listaCanciones.SelectedItem);
        }
    }

    private void AgregarCanciones(string carpeta)
    {
        string[] archivosMúsica = Directory.GetFiles(carpeta, ExtensionesMúsica, EnumerationOptions);
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
        duraciónActual.Text = TimeSpanATexto(mediaPlayer.Position * canciónActual.Duración);

        // Más de una hora
        if (duraciónActual.Text.Length > 5 && TamañoTiempoNormalActual)
            EnCambioTamaño(null, null);
        else if (duraciónActual.Text.Length <= 5 && !TamañoTiempoNormalActual)
            EnCambioTamaño(null, null);

        if (MoviendoTiempoCanción)
        {
            duraciónObjetivo.Text = TimeSpanATexto(canciónActual.Duración * porcentajeDuraciónActual.Value);

            // Más de una hora
            if (duraciónObjetivo.Text.Length > 5 && TamañoTiempoNormalObjetivo)
                EnCambioTamaño(null, null);
            else if (duraciónObjetivo.Text.Length <= 5 && !TamañoTiempoNormalObjetivo)
                EnCambioTamaño(null, null);
            return;
        }

        if (canciónActual.Duración.TotalSeconds > 0)
            porcentajeDuraciónActual.Value = mediaPlayer.Position;
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
            //colorCanción.Color = ObtenerColorDominante(imagenÁlbum);
        }
        else
        {
            imgCarátula.Source = ObtenerSinCarátula();
            //colorCanción.Color = ColorGris;
        }

        // Datos
        nombreAutor.Text = canciónActual.Autor;
        nombreCanción.Text = canciónActual.Nombre;
        nombreAlbum.Text = canciónActual.Álbum;
        nombreDetalles.Text = canciónActual.Detalles;
        duraciónCompleta.Text = TimeSpanATexto(canciónActual.Duración);

        // Más de una hora
        if (duraciónCompleta.Text.Length > 5 && TamañoTiempoNormalCompleta)
            EnCambioTamaño(null, null);
        else if (duraciónCompleta.Text.Length <= 5 && !TamañoTiempoNormalCompleta)
            EnCambioTamaño(null, null);
    }

    private void ActualizarListaCarpetas()
    {
        try
        {
            // Falla cuando se borra úlimo elemento
            listaCarpetas.ItemsSource = null;
            listaCarpetas.Items.Clear();
            listaCarpetas.SelectedIndex = -1;
        }
        catch { }

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

        if (Aleatorio)
            AleatorizarCanciones();

        ContarCancionesEnCarpeta();
    }

    private void ContarCancionesEnCarpeta()
    {
        var duracion = new TimeSpan();

        for (int i = 0; i < cancionesActuales.Count; i++)
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
    {/*
        if (Aleatorio)
            botónAleatorio.Foreground = BrochaResaltado;
        else
            botónAleatorio.Foreground = Brushes.Black;*/
    }

    private void MostrarRepetir()
    {/*
        if (RepetirCanción)
            botónRepetir.Foreground = BrochaResaltado;
        else
            botónRepetir.Foreground = Brushes.Black;*/
    }

    private void BloquearBotones(object sender/*, KeyEventArgs e*/)
    {/*
        if (e.Key == Key.PageUp || e.Key == Key.PageDown)
            e.Handled = true;*/
    }

    // Barra título
    private void EnClicCerrar(object sender, RoutedEventArgs e)
    {
        mediaPlayer.Stop();
        //oyente.DesvincularTeclado();
    }
    
    public void EnCambioTamaño(object? sender, SizeChangedEventArgs? e)
    {/*
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
        Application.Current.Resources.Add("fuenteVolumen", (anchoPantalla / fuenteVolumen) * multiplicador);
        Application.Current.Resources.Add("fuenteNúmeroVolumen", (anchoPantalla / fuenteNúmeroVolumen) * multiplicador);

        Application.Current.Resources.Add("fuenteNombreCanción", (anchoPantalla / fuenteNombreCanción) * multiplicador);
        Application.Current.Resources.Add("fuenteAutorCanción", (anchoPantalla / fuenteAutorCanción) * multiplicador);
        Application.Current.Resources.Add("fuenteÁlbumCanción", (anchoPantalla / fuenteÁlbumCanción) * multiplicador);

        // Cambio tamaño tiempos
        Application.Current.Resources.Remove("fuenteTiempoActual");
        Application.Current.Resources.Remove("fuenteTiempoCompleta");
        Application.Current.Resources.Remove("fuenteTiempoObjetivo");

        if (duraciónCompleta.Text.Length > 5)
        {
            TamañoTiempoNormalCompleta = false;
            Application.Current.Resources.Add("fuenteTiempoCompleta", (anchoPantalla / (fuenteTiempoCompleta * 1.3)) * multiplicador);
        }
        else
        {
            TamañoTiempoNormalCompleta = true;
            Application.Current.Resources.Add("fuenteTiempoCompleta", (anchoPantalla / fuenteTiempoCompleta) * multiplicador);
        }

        if (duraciónActual.Text.Length > 5)
        {
            TamañoTiempoNormalActual = false;
            Application.Current.Resources.Add("fuenteTiempoActual", (anchoPantalla / (fuenteTiempoActual * 1.3)) * multiplicador);
        }
        else
        {
            TamañoTiempoNormalActual = true;
            Application.Current.Resources.Add("fuenteTiempoActual", (anchoPantalla / fuenteTiempoActual) * multiplicador);
        }

        if (duraciónObjetivo.Text.Length > 5)
        {
            TamañoTiempoNormalObjetivo = false;
            Application.Current.Resources.Add("fuenteTiempoObjetivo", (anchoPantalla / (fuenteTiempoObjetivo * 1.3)) * multiplicador);
        }
        else
        {
            TamañoTiempoNormalObjetivo = true;
            Application.Current.Resources.Add("fuenteTiempoObjetivo", (anchoPantalla / fuenteTiempoObjetivo) * multiplicador);
        }*/
    }

    // --- Botones fuera de foco ---
    /*
    private void EnTecla(object? sender, KeyPressedArgs? e)
    {
        if (e == null)
            return;

        switch (e.KeyPressed)
        {
            case Key.MediaPlayPause:
            case Key.Pause:
                EnClicPausa(sender, RoutedEvent);
                break;
            case Key.MediaNextTrack:
                EnClicSiguiente(sender, RoutedEvent);
                break;
            case Key.MediaPreviousTrack:
                EnClicAnterior(sender, RoutedEvent);
                break;
            case Key.MediaStop:
                EnClicDetener(sender, RoutedEvent);
                break;
            case Key.VolumeMute:
                EnClicSilencio(sender, RoutedEvent);
                break;
            case Key.PageUp:
                volumen.Value += volumen.LargeChange;
                mediaPlayer.Volume = volumen.Value;
                break;
            case Key.PageDown:
                volumen.Value -= volumen.LargeChange;
                mediaPlayer.Volume = volumen.Value;
                break;
            case Key.F7:
                EnClicAleatorio(sender, RoutedEvent);
                break;
            case Key.F8:
                EnClicRepetir(sender, RoutedEvent);
                break;
        }
    }*/
}
