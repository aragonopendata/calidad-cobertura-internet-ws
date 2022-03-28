using System.Text.Json.Serialization;

namespace ws_cobertura.httpapi.Model.Request
{
    public class RegistrarDatosCoberturaRequest
    {
        [JsonPropertyName("timestamp")]
        public string timestamp { get; set; }// Fecha y hora(en la franja horaria UTC) en la que se ha realizado la captura de datos de cobertura. En formato “YYYY-MM-dd HH:mm:ssZ”.
        [JsonPropertyName("coordenadax")]
        public string coordenadax { get; set; }// Coordenada X en el estándar EPSG:25830 de la posición geográfica anonimizada del usuario que ha realizado la captura de datos de cobertura.
        [JsonPropertyName("coordenaday")]
        public string coordenaday { get; set; }// Coordenada Y en el estándar EPSG:25830 de la posición geográfica anonimizada del usuario que ha realizado la captura de datos de cobertura.
        [JsonPropertyName("municipio")]
        public string municipio { get; set; }// Nombre del municipio en el que se encuentra el usuario que ha realizado la captura de datos de cobertura.
        [JsonPropertyName("ine")]
        public string ine { get; set; }// INE del municipio en el que se encuentra el usuario que ha realizado la captura de datos de cobertura.
        [JsonPropertyName("modelo")]
        public string modelo { get; set; }// Modelo del dispositivo del usuario que está realizando la captura.
        [JsonPropertyName("so")]
        public string so { get; set; }// Sistema Operativo del dispositivo del usuario que está realizando la captura. F
        [JsonPropertyName("tipoRed")]
        public string tipoRed { get; set; }// Tipo de Red (2G, 3G, 4G, 5G, WiFI, etc.) a la que está conectado dispositivo del usuario que está realizando la captura.
        [JsonPropertyName("operador")]
        public string operador { get; set; }// Operador(Movistar, Vodafone, Orange, etc.) al que está conectado el dispositivo del usuario que está realizando la captura.
        [JsonPropertyName("valorIntensidadSenial")]
        public string valorIntensidadSenial { get; set; }// Intensidad de la señal de la red a la que está conectado el dispositivo del usuario que está realizando la captura expresada en dBm.
        [JsonPropertyName("rangoIntensidadSenial")]
        public string rangoIntensidadSenial { get; set; }// Intensidad de la señal de la red a la que está conectado el dispositivo del usuario que está realizando la captura expresada mediante un número del 0 al 5. Los posibles valores son:
        /*o 5: Muy alta
        o 4: Alta
        o 3: Media
        o 2: Baja
        o 1: Muy baja
        o 0: Sin señal*/
        [JsonPropertyName("velocidadBajada")]
        public string velocidadBajada { get; set; }// Velocidad de Bajada (Mbits/s) en un test de velocidad que realiza el usuario con su dispositivo durante la captura.
        [JsonPropertyName("velocidadSubida")]
        public string velocidadSubida { get; set; }// Velocidad de Subida(Mbits/s) en un test de velocidad que realiza el usuario con su dispositivo durante la captura.
        [JsonPropertyName("latencia")]
        public string latencia { get; set; }// Latencia(ping) en un test de velocidad que realiza el usuario con su dispositivo durante la captura.
    }
}
