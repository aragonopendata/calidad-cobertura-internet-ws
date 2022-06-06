using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ws_cobertura.httpapi.Model.General;

namespace ws_cobertura
{
    public static class Helper
    {      
        public static async Task<string> GetRawBodyAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;

            var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            request.Body.Position = 0;

            return body;
        }

        public static void VolcarAConsola(string clase, string metodo, string mensaje, bool esError) {
            try {
                DateTime dTimeAhora = DateTime.Now;

                string mensajeVolcar = string.Empty;

                if (esError == true)
                {
                    mensajeVolcar = "|||ERROR " + dTimeAhora.ToString("dd /MM/yyyy hh:mm:ss") + " " + clase + "." + metodo + " con mensaje: " + mensaje;
                }
                else {
                    mensajeVolcar = "|||DEBUG " + dTimeAhora.ToString("dd /MM/yyyy hh:mm:ss") + " " + clase + "." + metodo + " con mensaje: " + mensaje;
                }

                Console.WriteLine(mensajeVolcar);
            }
            catch (Exception ex) {
                Console.WriteLine("No se pudo volcar a consola");
            }
        }

        public static string DateTimeToEpochSeconds(DateTime dTime)
        {
            string sEpochSeconds = dTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.ToString();

            return sEpochSeconds;
        }

        public static string DateTimeToEpochMillis(DateTime dTime)
        {
            string sEpochMillis = dTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();

            return sEpochMillis;
        }

        public static DateTime EpochSecondsToDateTime(string sSeconds)
        {
            long lSeconds = long.Parse(sSeconds);

            return EpochSecondsToDateTime(lSeconds);
        }

        public static DateTime EpochSecondsToDateTime(long lSeconds) {
            DateTime dTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dTime = dTime.AddSeconds((lSeconds));

            return dTime;
        }

        public static DateTime EpochMillisToDateTime(string sMilliseconds)
        {
            long lMilliseconds = long.Parse(sMilliseconds);

            return EpochSecondsToDateTime(lMilliseconds);
        }

        public static DateTime EpochMillisToDateTime(long lMilliseconds)
        {
            DateTime dTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dTime = dTime.AddMilliseconds((lMilliseconds));

            return dTime;
        }
    }
}
