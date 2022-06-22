using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ws_cobertura.ElasticSearch;

namespace ws_cobertura.httpapi.Model.Response
{
    public class ReportesFiltradosResponse
    {
        public List<DatosCobertura> documents { get; set; }

    }
    public class DatosCobertura
    {
        public static DocumentFormat.OpenXml.Spreadsheet.CellValues getOpenXMLPropertyDataType(string jsonPropertyName)
        {

            if (!string.IsNullOrEmpty(jsonPropertyName))
            {
                switch (jsonPropertyName)
                {
                    case "fecha":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Date;
                    case "categoria":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "calidad":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "municipio":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;

                    case "ine":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "modelo":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "so":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "tipoRed":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "operador":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    case "coordenadaX":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "coordenadaY":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "latitud":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "longitud":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "valorIntensidadSenial":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "rangoIntensidadSenial":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "velocidadBajada":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "rangoVelocidadBajada":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "velocidadSubida":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "rangoVelocidadSubida":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "latencia":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    case "rangoLatencia":

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                    default:

                        return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                }
            }
            else
            {
                return DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
            }
        }

        [Newtonsoft.Json.JsonProperty(PropertyName = "fecha", Order = 1)]
        public DateTime fecha { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "categoria", Order = 2)]
        public string categoria { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "calidad", Order = 3)]
        public string calidad { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "municipio", Order = 4)]
        public string municipio { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "ine", Order = 5)]
        public string ine { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "modelo", Order = 6)]
        public string modelo { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "so", Order = 7)]
        public string so { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "tipoRed", Order = 8)]
        public string tipoRed { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "operador", Order = 9)]
        public string operador { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "coordenadaX", Order = 10)]
        public double? coordenadaX { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "coordenadaY", Order = 11)]
        public double? coordenadaY { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "latitud", Order = 12)]
        public double? latitud { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "longitud", Order = 13)]
        public double? longitud { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "valorIntensidadSenial", Order = 14)]
        public decimal? valorIntensidadSenial { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "rangoIntensidadSenial", Order = 15)]
        public int rangoIntensidadSenial { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "velocidadBajada", Order = 16)]
        public decimal? velocidadBajada { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "rangoVelocidadBajada", Order = 17)]
        public int rangoVelocidadBajada { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "velocidadSubida", Order = 18)]
        public decimal? velocidadSubida { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "rangoVelocidadSubida", Order = 19)]
        public int rangoVelocidadSubida { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "latencia", Order = 20)]
        public decimal? latencia { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "rangoLatencia", Order = 21)]
        public int rangoLatencia { get; set; }
    }
    
}
