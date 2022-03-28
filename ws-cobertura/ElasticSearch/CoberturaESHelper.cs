
using Elasticsearch.Net;
using Messaging.ElasticSearch;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ws_cobertura.httpapi.Model.General;

namespace ws_cobertura.ElasticSearch
{
    public class CoberturaESHelper : ESHelper
    {
        public CoberturaESHelper(ESConfiguration esConfiguration, IndexMappingsConfiguration indexMappingsConfiguration) : base(esConfiguration, indexMappingsConfiguration)
        {
        }

        public static CoberturaESHelper CreateInstance(ESConfiguration esConfiguration, IndexMappingsConfiguration indexMappingsConfiguration, Action<CoberturaESHelper, ElasticClient> createMappings)
        {
            CoberturaESHelper esHelper = new CoberturaESHelper(esConfiguration, indexMappingsConfiguration);
            createMappings(esHelper, esHelper.CreateClient());
            return esHelper;
        }

        protected static new ElasticClient _elasticClient = null;

        protected new ElasticClient CreateClient()
        {
            if (_elasticClient == null)
            {
                var uri = new Uri(_configuration.Url);
                var pool = new SingleNodeConnectionPool(uri);

                ConnectionSettings connectionSettings = new ConnectionSettings(pool)
                    .BasicAuthentication(_configuration.User, _configuration.Secret)
                    .EnableDebugMode()
                    .DefaultFieldNameInferrer(x=> x.Replace("__", " ")) // para los campos a mostrar en tooltip
                    .PrettyJson()
                    .DefaultIndex(GetIndexNameForType("CoberturaReport"))
                    .RequestTimeout(TimeSpan.FromMilliseconds(_configuration.SingleTimeoutMilliseconds));

                _elasticClient = new ElasticClient(connectionSettings);
            }

            return _elasticClient;
        }


        public async Task<DeleteIndexResponse> DeleteIndexCoberturaReportAgrupadoCuadriculaTecnologia(string sMappingProperty = "")
        {
            string sIndexName = GetIndexNameForType("CoberturaReportAgrupadoCuadriculaTecnologia");

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();

            DeleteIndexResponse response = await client.Indices.DeleteAsync(sIndexName);

            return response;
        }

        public async Task<IndexResponse> AddCoberturaReport(CoberturaReport oCoberturaReport, string sMappingProperty = "")
        {
            string sIndexName = GetIndexNameForType("CoberturaReport");

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();

            IndexResponse response = await client.IndexAsync<CoberturaReport>(oCoberturaReport, s => s.Index(sIndexName));

            return response;
        }

        public async Task<IndexResponse> AddUltimaEjecucionCron(CoberturaCronUltimaEjecucion oCoberturaCronUltimaEjecucion, string sMappingProperty = "")
        {
            string sIndexName = GetIndexNameForType("CoberturaUltimaEjecucionCron");

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();

            IndexResponse response = await client.IndexAsync<CoberturaCronUltimaEjecucion>(oCoberturaCronUltimaEjecucion, s => s.Index(sIndexName));

            return response;
        }

        public async Task<UpdateResponse<CoberturaReportAgrupadoCuadriculaTecnologia>> AddListCoberturaReportAgrupadoCuadriculaTecnologiaToIndex(List<CoberturaReportAgrupadoCuadriculaTecnologia> documentList, string sMappingProperty = "")
        {
            string sIndexName = GetIndexNameForType("CoberturaReportAgrupadoCuadriculaTecnologia");

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();
            UpdateResponse<CoberturaReportAgrupadoCuadriculaTecnologia> response = null;

            foreach (CoberturaReportAgrupadoCuadriculaTecnologia oCoberturaReportAgrupadoCuadriculaTecnologia in documentList) {
                response = await client.UpdateAsync<CoberturaReportAgrupadoCuadriculaTecnologia>(new DocumentPath<CoberturaReportAgrupadoCuadriculaTecnologia>(oCoberturaReportAgrupadoCuadriculaTecnologia.clave).Index(sIndexName),
                    u => u.DocAsUpsert(true)
                          .Index(sIndexName)
                          .Doc(oCoberturaReportAgrupadoCuadriculaTecnologia)
                );
            }

            return response;
        }

        
        public async Task<DateTimeOffset?> ObtenerUltimaEjecucionCorrectaCronPorIndexMapping(string sIndexMappingNameBuscar)
        {
            DateTimeOffset? dUltimaFechaEjecucion = null;

            List<CoberturaReportAgrupadoCuadriculaTecnologia> listDocuments = new List<CoberturaReportAgrupadoCuadriculaTecnologia>();

            string sIndexNameUsar = GetIndexNameForType("CoberturaUltimaEjecucionCron");

            ElasticClient client = CreateClient();

            string sIndexNameBuscar = GetIndexNameForType(sIndexMappingNameBuscar);

            var response = await client.SearchAsync<CoberturaCronUltimaEjecucion>(s => s.Index(sIndexNameUsar)
                    .Query(q => q //para filtrar solo las keys modificadas desde ultima ejecucion
                        .Bool(b => b
                            .Must(
                                    new TermQuery()
                                    {
                                        Field = new Nest.Field("nombre_index.keyword"),
                                        Value = sIndexNameBuscar
                                    }
                                )
                            )
                        )
                        .Sort(so => so
                            .Field(fs => fs
                                .Field("fecha_ultima_ejecucion")
                                .Order(SortOrder.Descending)
                            )
                        )
                    .Take(1)
                 );

            if (response != null && response.IsValid)
            {
                if (response.Documents.Count > 0) {
                    dUltimaFechaEjecucion = response.Documents.FirstOrDefault().fecha_ultima_ejecucion;
                }
            }

            return dUltimaFechaEjecucion;
        }

        public async Task<List<CoberturaReportAgrupadoCuadriculaTecnologia>> ObtenerReportesAgrupadosPorCuadriculaYTecnologia(DateTimeOffset? dFechaUltimaEjecucionCron, string sMappingProperty = null)
        {
            List<CoberturaReportAgrupadoCuadriculaTecnologia> listDocuments = new List<CoberturaReportAgrupadoCuadriculaTecnologia>();

            string sIndexName = GetIndexNameForType("CoberturaReport"); 

            if (!string.IsNullOrEmpty(sMappingProperty)) {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();

            AggregationDictionary oAgregadoNested = new AggregationDictionary
            {
                {
                    "mediaVelocidadSubida",
                    new AverageAggregation("mediaVelocidadBajada", "message.velocidadSubida")
                },
                {
                    "mediaVelocidadBajada",
                    new AverageAggregation("mediaVelocidadBajada", "message.velocidadBajada")
                },
                {
                    "mediaLatencia",
                    new AverageAggregation("mediaLatencia", "message.latencia")
                },
                {
                    "mediaValorIntensidadSenial",
                    new AverageAggregation("mediaValorIntensidadSenial", "message.valorIntensidadSenial")
                },
                {
                    "minTimestamp",
                    new MinAggregation("minTimestamp", "message.timestamp_seconds")
                },
                {
                    "maxTimestamp",
                    new MaxAggregation("maxTimestamp", "message.timestamp_seconds")
                }
                ,
                {
                    "coordenadax",
                    new MaxAggregation("coordenadax", "message.coordenadax")
                }
                ,
                {
                    "coordenaday",
                    new MaxAggregation("coordenaday", "message.coordenaday")
                }
                ,
                {
                    "tipoRed",
                    new TopMetricsAggregation("tipoRed")
                    {
                        Metrics = new List<ITopMetricsValue>
                        {
                            new TopMetricsValue("message.tipoRed.keyword")
                        },
                        Size = 1,
                        Sort = new List<ISort> { new FieldSort { Field = "message.timestamp_seconds", Order = SortOrder.Ascending } }
                    }
                }
                ,
                {
                    "locationRaw",
                    new GeoBoundsAggregation("locationRaw", "message.location")
                }
                ,
                {
                    "arrMunicipios",
                    new TermsAggregation("arrMunicipios")
                    {
                        Field = "message.municipio.keyword"
                    }
                }
            };

            AggregationDictionary oAgregadoPrincipal = new AggregationDictionary
            {
                {
                    "agregado",
                    new TermsAggregation("agregado")
                    {
                        Field = "message.agrupado_cuadricula_tecnologia.keyword",
                        Aggregations = oAgregadoNested
                    }
                }
            };

            ISearchResponse<CoberturaReport> response = null;

            if (dFechaUltimaEjecucionCron.HasValue)
            {
                AggregationDictionary oAgregadoPrincipalSinNested = new AggregationDictionary
                {
                    {
                        "agregado",
                        new TermsAggregation("agregado")
                        {
                            Field = "message.agrupado_cuadricula_tecnologia.keyword",
                        }
                    }
                };

                List<string> sKeysModificadasDesdeUltimaEjecucion = new List<string>();

                var responseModificados = await client.SearchAsync<CoberturaReport>(s => s.Index(sIndexName)
                    .Query(q => q //para filtrar solo por los registros desde la última ejecución
                        .Bool(b => b
                            .Filter(f =>
                                f.DateRange(dt => dt
                                    .Field(field => field.insertion_time)
                                    .GreaterThanOrEquals(dFechaUltimaEjecucionCron.Value.UtcDateTime)
                                    .TimeZone("UTC")))))
                    .Take(0) //para traer solo los agregados
                    .Aggregations(oAgregadoPrincipalSinNested)
                );

                if (responseModificados != null && responseModificados.IsValid)
                {
                    if (responseModificados.Aggregations.Count > 0 && responseModificados.Aggregations["agregado"] != null)
                    {
                        var responseAggs = responseModificados.Aggregations.Terms("agregado");

                        foreach (var childAgg in responseAggs.Buckets)
                        {
                            sKeysModificadasDesdeUltimaEjecucion.Add(childAgg.Key);
                        }
                    }


                    if (sKeysModificadasDesdeUltimaEjecucion.Count > 0)
                        response = await client.SearchAsync<CoberturaReport>(s => s.Index(sIndexName)
                            .Query(q => q //para filtrar solo las keys modificadas desde ultima ejecucion
                                .Bool(b => b
                                    .Must(
                                            new TermsQuery()
                                            {
                                                Field = new Nest.Field("message.agrupado_cuadricula_tecnologia.keyword"),
                                                Terms = sKeysModificadasDesdeUltimaEjecucion.ToArray()
                                            }
                                        )
                                    )
                                )
                            .Take(0) //para traer solo los agregados
                            .Aggregations(oAgregadoPrincipal)
                    );
                }
            }
            else {

                response = await client.SearchAsync<CoberturaReport>(s => s.Index(sIndexName)
                    .Take(0) //para traer solo los agregados
                    .Aggregations(oAgregadoPrincipal)
                );
            }

            if (response != null && response.IsValid)
            {
                if (response.Aggregations.Count > 0 && response.Aggregations["agregado"] != null) {
                    var responseAggs = response.Aggregations.Terms("agregado");

                    foreach (var childAgg in responseAggs.Buckets)
                    {
                        CoberturaReportAgrupadoCuadriculaTecnologia oCoberturaReportAgrupadoCuadriculaTecnologia = new CoberturaReportAgrupadoCuadriculaTecnologia();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.clave = childAgg.Key;

                        Nest.ValueAggregate vaMediaVelocidadSubida = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaVelocidadSubida").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMediaVelocidadBajada = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaVelocidadBajada").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMediaLatencia = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaLatencia").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMediaValorIntensidadSenial = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaValorIntensidadSenial").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMinTimestamp = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "minTimestamp").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMaxTimestamp = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "maxTimestamp").Select(x => x.Value).FirstOrDefault();

                        Nest.ValueAggregate vaCoordenadaX = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenadax").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaCoordenadaY = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenaday").Select(x => x.Value).FirstOrDefault();
                        Nest.TopMetricsAggregate vaTipoRed = (Nest.TopMetricsAggregate)childAgg.Where(x => x.Key == "tipoRed").Select(x => x.Value).FirstOrDefault();
                        Nest.GeoBoundsAggregate vaLocation = (Nest.GeoBoundsAggregate)childAgg.Where(x => x.Key == "locationRaw").Select(x => x.Value).FirstOrDefault();
                        Nest.BucketAggregate vaMunicipios = (Nest.BucketAggregate)childAgg.Where(x => x.Key == "arrMunicipios").Select(x => x.Value).FirstOrDefault();

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida = (decimal?) vaMediaVelocidadSubida.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada = (decimal?) vaMediaVelocidadBajada.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia = (decimal?) vaMediaLatencia.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial = (decimal) vaMediaValorIntensidadSenial.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.cantidadMedidas = childAgg.DocCount.HasValue ? childAgg.DocCount.Value : 0;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.coodenadax = (double) vaCoordenadaX.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.coodenaday = (double) vaCoordenadaY.Value;

                        List<string> sListMunicipios = new List<string>();

                        if (vaMunicipios != null && vaMunicipios.Items.Count > 0) {
                            foreach (var municipioBucket in vaMunicipios.Items) {
                                string sMunicipio = ((KeyedBucket<object>)municipioBucket).Key.ToString();

                                if (!string.IsNullOrEmpty(sMunicipio)) {
                                    if (!sListMunicipios.Contains(sMunicipio))
                                    {
                                        sListMunicipios.Add(sMunicipio);
                                    }
                                }
                            }
                        }

                        sListMunicipios = sListMunicipios.OrderBy(x => x).ToList();

                        oCoberturaReportAgrupadoCuadriculaTecnologia.arrMunicipios = sListMunicipios.ToArray();

                        oCoberturaReportAgrupadoCuadriculaTecnologia.location = new CoberturaAgrupadoCuaTecLocation();

                        if (vaTipoRed.Top != null && vaTipoRed.Top.Count > 0 && vaTipoRed.Top.FirstOrDefault() != null && vaTipoRed.Top.FirstOrDefault().Metrics != null && vaTipoRed.Top.FirstOrDefault().Metrics.Count > 0) {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.tipoRed = vaTipoRed.Top.FirstOrDefault().Metrics.FirstOrDefault().Value.ToString();
                        }

                        if (vaLocation.Bounds != null && vaLocation.Bounds.TopLeft != null)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.location.lat = (double)vaLocation.Bounds.TopLeft.Lat;
                            oCoberturaReportAgrupadoCuadriculaTecnologia.location.lon = (double)vaLocation.Bounds.TopLeft.Lon;
                        }

                        if (vaMinTimestamp.Value.HasValue) {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.minTimestamp = Helper.EpochMillisToDateTime((long)vaMinTimestamp.Value.Value);

                        }

                        if (vaMaxTimestamp.Value.HasValue) {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.maxTimestamp = Helper.EpochMillisToDateTime((long)vaMaxTimestamp.Value.Value);
                        }

                        APIHelper oAPIHelper = new APIHelper(null, null);
                        EnumRango.Intensidad oRangoIntensidad = EnumRango.calcularRangoIntensidad(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoIntensidadSenial = (int) oRangoIntensidad;

                        //campos tooltip mapa

                        EnumRango.VelocidadBajada oRangoBajada = EnumRango.calcularRangoVelocidadBajada(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoVelocidadBajada = (int)oRangoBajada;

                        string textoRangoVelocidadBajada = EnumRango.getStringValue(oRangoBajada);
                        string textoVelocidadBajada = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.HasValue ? textoRangoVelocidadBajada + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Velocidad__Bajada = textoVelocidadBajada;

                        EnumRango.VelocidadSubida oRangoSubida = EnumRango.calcularRangoVelocidadSubida(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoVelocidadSubida = (int)oRangoSubida;

                        string textoRangoVelocidadSubida = EnumRango.getStringValue(oRangoSubida);
                        string textoVelocidadSubida = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida.HasValue ? textoRangoVelocidadSubida + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Velocidad__Subida = textoVelocidadSubida;

                        EnumRango.Latencia oRangoLatencia = EnumRango.calcularRangoLatencia(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoLatencia = (int)oRangoLatencia;

                        string tenxtoRangoLatencia = EnumRango.getStringValue(oRangoLatencia);
                        string textoLatencia = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia.HasValue ? tenxtoRangoLatencia + " (" +  Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia.Value, 0, MidpointRounding.AwayFromZero).ToString() + " ms)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Latencia = textoLatencia;


                        string textoRangoIntensidad = EnumRango.getStringValue(oRangoIntensidad);
                        string textoIntensidad = textoRangoIntensidad + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial, 0, MidpointRounding.AwayFromZero).ToString() + " dBm)";
                        
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Intensidad = textoIntensidad;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Tipo__Red = !string.IsNullOrEmpty(oCoberturaReportAgrupadoCuadriculaTecnologia.tipoRed) ? oCoberturaReportAgrupadoCuadriculaTecnologia.tipoRed : Constantes.CAMPO_SIN_DATOS;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Cantidad__Mediciones = oCoberturaReportAgrupadoCuadriculaTecnologia.cantidadMedidas.ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Fecha__Desde = oCoberturaReportAgrupadoCuadriculaTecnologia.minTimestamp.ToString("dd/MM/yyyy");
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Fecha__Hasta = oCoberturaReportAgrupadoCuadriculaTecnologia.maxTimestamp.ToString("dd/MM/yyyy");
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Municipios = oCoberturaReportAgrupadoCuadriculaTecnologia.arrMunicipios.Length > 0 ? string.Join(", ", oCoberturaReportAgrupadoCuadriculaTecnologia.arrMunicipios) : string.Empty;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoVelocidadBajada = EnumRango.obtenerIconoRangoVelocidadBajada(oRangoBajada).ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoVelocidadSubida = EnumRango.obtenerIconoRangoVelocidadSubida(oRangoSubida).ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoLatencia = EnumRango.obtenerRangoIconoLatencia(oRangoLatencia).ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoIntensidad = EnumRango.obtenerIconoRangoIntensidad(oRangoIntensidad).ToString();

                        listDocuments.Add(oCoberturaReportAgrupadoCuadriculaTecnologia);
                    }
                }
            }

            return listDocuments;
        }
    }
}
