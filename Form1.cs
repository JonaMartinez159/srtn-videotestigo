using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using static System.Windows.Forms.DataFormats;

namespace GrabacionTestigoSRTN
{
    public partial class Form1 : Form
    {

        public bool proceso_iniciado = false;
        private CancellationTokenSource _cts;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Interval = 120 * 60 * 1000; // 2 horas

            button1.Enabled = false;
            button1.Text = "En grabacion";

            timerRevisaHora.Enabled = true;
            //iniciarProcesoAsync();
        }

        public async Task iniciarProcesoAsync()
        {
            bool hora_valida = CompararHora();

            if (this.proceso_iniciado == false)
            {
                if (hora_valida == true)
                {
                    //Iniciar proceso
                    _cts = new CancellationTokenSource();
                    this.proceso_iniciado = true;
                    timer1.Start();
                    timerRevisaHora.Stop();

                    DateTime now = DateTime.Now;
                    archivo_actual.Text = "srtn_testigo_" + now.ToString("yyyy-MM-dd-HH-mm-ss");
                    await IniciarGrabacion(_cts.Token, textBox2.Text, textBox1.Text);
                }
                else
                {
                    timer1.Stop();
                    archivo_actual.Text = "Esperando Hora de inicio de grabacion";
                }
            }
        }

        public static bool CompararHora()
        {
            string horaInicioStr = "06:00:00 a. m.";
            string horaFinStr = "23:59:00 p. m.";

            //obtener fecha actual
            DateTime now = DateTime.Now;
            //obtener solo hora string
            String horaStr = now.ToString("HH:mm:ss tt");

            //convertir a datetime solo la hora
            DateTime hora = DateTime.ParseExact(horaStr, "HH:mm:ss tt", null);

            //Crear hora fin e inicio en formato datetime para poder comparar
            DateTime horaInicio = DateTime.ParseExact(horaInicioStr, "HH:mm:ss tt", null);
            DateTime horaFin = DateTime.ParseExact(horaFinStr, "HH:mm:ss tt", null);

            if (hora >= horaInicio && hora <= horaFin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static async Task IniciarGrabacion(CancellationToken token, String ruta_salida, String crf)
        {
            //obtener un nombre unico basado en fecha hora minuto y segundo
            DateTime now = DateTime.Now;

            //obtener solo hora string
            String nombre_archiv = "srtn_testigo_" + now.ToString("yyyy-MM-dd-HH-mm-ss");
            nombre_archiv = nombre_archiv + ".mp4";

            // Crea un nuevo proceso
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\ffmpeg\bin\ffmpeg.exe";
            //startInfo.Arguments = $"ffmpeg -f dshow -i video=\"FY HD Video\" -c:v libx264 -crf 35 \"C:\\ffmpeg\\bin\\{nombre_archiv}";
            startInfo.Arguments = $"-f dshow -i video=\"FY HD Video\":audio=\"Interfaz de sonido digital (FY HD Audio)\" -rtbufsize 2147.48M  -vf \"scale=1080:720\" -c:v libx264 -c:a copy -preset ultrafast -crf {crf} {ruta_salida}\\{nombre_archiv}\"";

            startInfo.UseShellExecute = false; // No usa la interfaz de shell del sistema
            startInfo.RedirectStandardInput = true;
            startInfo.CreateNoWindow = true; // No crea una nueva ventana de consola

            Process process = new Process();
            process.StartInfo = startInfo;

            await Task.Run(() =>
            {
                /*
                process.WaitForExit();

                if (CompararHora() == false || cts.Token.IsCancellationRequested)
                {
                    process.Kill();
                }*/
                process.Start();
                while (!process.HasExited)
                {
                    if (CompararHora() == false || token.IsCancellationRequested)
                    {
                        process.StandardInput.WriteLine("q");
                        Thread.Sleep(1000);

                        process.Kill();
                        break;
                    }
                    //Thread.Sleep(1000);
                }
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _cts?.Cancel();
            this.proceso_iniciado = false;
            timerRevisaHora.Start();

            //Thread.Sleep(5000);
            //iniciarProcesoAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Detener grabacion
            timer1.Stop();
            timerRevisaHora.Stop();

            this.proceso_iniciado = false;
            _cts?.Cancel();
        }

        private void timerRevisaHora_Tick(object sender, EventArgs e)
        {
            iniciarProcesoAsync();
        }
    }
}
