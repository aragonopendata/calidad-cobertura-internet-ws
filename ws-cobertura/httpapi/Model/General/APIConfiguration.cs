namespace ws_cobertura.httpapi.Model.Configuration
{
    public class APIConfiguration
    {
        public string token { get; set; }
        public string UrlObtenerMunicipioPorCoordenadas { get; set; }
        public string UrlObtenerCoordenadasPorMunicipio { get; set; }
        public string MultiploRedondeoAnonimizarUTM { get; set; } = "500";
    }
}
