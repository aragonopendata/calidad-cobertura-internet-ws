using Messaging;
using System;
using System.Text.Json.Serialization;

namespace ws_cobertura.RabbitMQ
{
    public class CoberturaMessage : IMessage
    {
        /*[JsonPropertyName("id")]
        public string id { get; set; } // id generado para elastic*/
        [JsonPropertyName("timestamp")]
        public string timestamp { get; set; }// Fecha y hora(en la franja horaria UTC) en la que se ha realizado la captura de datos de cobertura. En formato “YYYY-MM-dd HH:mm:ssZ”.
        [JsonPropertyName("timestamp_seconds")]
        public string timestamp_seconds { get; set; } // Fecha convertida a epoch_seconds para envío a elastic
        [JsonPropertyName("coordenadax")]
        public double? coordenadax { get; set; }// Coordenada X en el estándar EPSG:25830 de la posición geográfica anonimizada del usuario que ha realizado la captura de datos de cobertura.
        [JsonPropertyName("coordenaday")]
        public double? coordenaday { get; set; }// Coordenada Y en el estándar EPSG:25830 de la posición geográfica anonimizada del usuario que ha realizado la captura de datos de cobertura.
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
        public decimal valorIntensidadSenial { get; set; }// Intensidad de la señal de la red a la que está conectado el dispositivo del usuario que está realizando la captura expresada en dBm.
        [JsonPropertyName("rangoIntensidadSenial")]
        public int rangoIntensidadSenial { get; set; }// Intensidad de la señal de la red a la que está conectado el dispositivo del usuario que está realizando la captura expresada mediante un número del 0 al 5. Los posibles valores son:
        /*o 5: Muy alta
        o 4: Alta
        o 3: Media
        o 2: Baja
        o 1: Muy baja
        o 0: Sin señal*/
        [JsonPropertyName("velocidadBajada")]
        public decimal? velocidadBajada { get; set; }// Velocidad de Bajada (Mbits/s) en un test de velocidad que realiza el usuario con su dispositivo durante la captura.
        [JsonPropertyName("rangoVelocidadBajada")]
        public int rangoVelocidadBajada { get; set; }
        [JsonPropertyName("velocidadSubida")]
        public decimal? velocidadSubida { get; set; }// Velocidad de Subida(Mbits/s) en un test de velocidad que realiza el usuario con su dispositivo durante la captura.
        [JsonPropertyName("rangoVelocidadSubida")]
        public int rangoVelocidadSubida { get; set; }
        [JsonPropertyName("latencia")]
        public decimal? latencia { get; set; }
        [JsonPropertyName("rangoLatencia")]
        public int rangoLatencia { get; set; }
        [JsonPropertyName("agrupado_cuadricula_tecnologia")]
        public string agrupado_cuadricula_tecnologia { get; set; }// Mensaje agrupado con los campos claves para el indice por coordenadas y tecnologia
        [JsonPropertyName("agrupado_ine_tecnologia")]
        public string agrupado_ine_tecnologia { get; set; }// Mensaje agrupado con los campos claves para el indice por coordenadas y tecnologia
        [JsonPropertyName("agrupado_municipio_tecnologia")]
        public string agrupado_municipio_tecnologia { get; set; }// Mensaje agrupado con los campos claves para el indice por coordenadas y tecnologia
        [JsonPropertyName("location")]
        public CoberturaMessageLocation location { get; set; }// Mensaje agrupado con los campos claves para el indice por coordenadas y tecnologia



        public CoberturaMessage()
        {
            //this.id = Guid.NewGuid().ToString();
        }

        int IMessage.GetPriority()
        {
            return 5;
        }

        public static System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public string Serialize()
        {
            String json = System.Text.Json.JsonSerializer.Serialize(this, jsonSerializerOptions);
            return json;
        }

        public String GetId()
        {
            //return this.id;
            throw new NotImplementedException();
        }

        public String GetCorrelationId()
        {
            //return this.id;
            throw new NotImplementedException();
        }

        /*protected DateTimeOffset Timestamp
        {
            get
            {
                return DateTimeOffset.ParseExact(this.timestamp, "yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        protected String HexDate
        {
            get
            {
                byte[] buffer = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteByte((byte)(this.Timestamp.Year - 2000));
                    ms.WriteByte((byte)this.Timestamp.Month);
                    ms.WriteByte((byte)this.Timestamp.Day);
                    ms.WriteByte((byte)this.Timestamp.Hour);
                    ms.WriteByte((byte)this.Timestamp.Minute);
                    ms.WriteByte((byte)this.Timestamp.Second);
                    buffer = ms.ToArray();
                }

                return buffer.BytesToHex();
            }
        }*/
        
    }

    public class CoberturaMessageLocation
    {
        [JsonPropertyName("lat")]
        public double lat;
        [JsonPropertyName("lon")]
        public double lon;
    }
}
