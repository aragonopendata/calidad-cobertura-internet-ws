namespace ws_cobertura.httpapi.Model.Configuration
{
    public class APIConfiguration
    {
        public string token { get; set; }
        public string tokenCarga { get; set; }
        public string UrlObtenerMunicipioPorCoordenadas { get; set; }
        public string UrlObtenerCoordenadasPorMunicipio { get; set; }
        /*public string MultiploRedondeoAnonimizarUTM { get; set; } = "500";
        public string SumaRedondeoAnonimizarUTM { get; set; } = "0";*/
    }
}
