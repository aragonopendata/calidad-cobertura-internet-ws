
using Elasticsearch.Net;
using Messaging.ElasticSearch;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ws_cobertura.httpapi.Model.General;
using ws_cobertura.httpapi.Model.Response;

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


        public async Task<DeleteIndexResponse> DeleteIndexCoberturaReportAgrupadoCuadricula(string sMappingProperty)
        {
            //string sIndexName = GetIndexNameForType("CoberturaReportAgrupadoCuadriculaTecnologia");
            string sIndexName = string.Empty;

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

        public async Task<UpdateResponse<CoberturaReportAgrupadoCuadricula>> AddListCoberturaReportAgrupadoCuadriculaToIndex(List<CoberturaReportAgrupadoCuadricula> documentList, string sMappingProperty)
        {
            //string sIndexName = GetIndexNameForType("CoberturaReportAgrupadoCuadriculaTecnologia");
            string sIndexName = string.Empty;

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();
            UpdateResponse<CoberturaReportAgrupadoCuadricula> response = null;

            foreach (CoberturaReportAgrupadoCuadricula oCoberturaReportAgrupadoCuadriculaTecnologia in documentList) {
                response = await client.UpdateAsync<CoberturaReportAgrupadoCuadricula>(new DocumentPath<CoberturaReportAgrupadoCuadricula>(oCoberturaReportAgrupadoCuadriculaTecnologia.clave).Index(sIndexName),
                    u => u.DocAsUpsert(true)
                          .Index(sIndexName)
                          .Doc(oCoberturaReportAgrupadoCuadriculaTecnologia)
                );
            }

            return response;
        }

        public async Task<UpdateResponse<CoberturaReport>> AddListCoberturaReportToIndex(List<CoberturaReport> documentList, string sMappingProperty)
        {
            string sIndexName = string.Empty;

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();
            UpdateResponse<CoberturaReport> response = null;

            foreach (CoberturaReport oCoberturaReport in documentList)
            {
                response = await client.UpdateAsync<CoberturaReport>(new DocumentPath<CoberturaReport>(oCoberturaReport.IndexIdProperty).Index(sIndexName),
                    u => u.DocAsUpsert(true)
                          .Index(sIndexName)
                          .Doc(oCoberturaReport)
                );
            }

            return response;
        }

        public async Task<DateTimeOffset?> ObtenerUltimaEjecucionCorrectaCronPorIndexMapping(string sIndexMappingNameBuscar)
        {
            DateTimeOffset? dUltimaFechaEjecucion = null;

            List<CoberturaReportAgrupadoCuadricula> listDocuments = new List<CoberturaReportAgrupadoCuadricula>();

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

        public async Task<List<CoberturaReportAgrupadoCuadricula>> ObtenerReportesAgrupadosPorCuadricula(DateTimeOffset? dFechaUltimaEjecucionCron, int iRangoAgrupado, string sMappingProperty = null)
        {
            List<CoberturaReportAgrupadoCuadricula> listDocuments = new List<CoberturaReportAgrupadoCuadricula>();

            string sIndexName = GetIndexNameForType("CoberturaReport"); 

            if (!string.IsNullOrEmpty(sMappingProperty)) {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();

            AggregationDictionary oAgregadoNested = new AggregationDictionary
            {
                {
                    "mediaVelocidadSubida",
                    new PercentilesAggregation("mediaVelocidadSubida", "message.velocidadSubida") {
                        Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaVelocidadBajada",
                    new PercentilesAggregation("mediaVelocidadBajada", "message.velocidadBajada") {
                        Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaLatencia",
                    new PercentilesAggregation("mediaLatencia", "message.latencia") {
                        Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaValorIntensidadSenial",
                    new PercentilesAggregation("mediaValorIntensidadSenial", "message.valorIntensidadSenial") {
                    Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaRangoCalidad",
                    new PercentilesAggregation("mediaRangoCalidad", "message.valorIntensidadSenial") {
                    Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaRangoVelocidadSubida",
                    new PercentilesAggregation("mediaRangoVelocidadSubida", "message.rangoVelocidadSubida") {
                        Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaRangoVelocidadBajada",
                    new PercentilesAggregation("mediaRangoVelocidadBajada", "message.rangoVelocidadBajada") {
                        Percents = new[] { 50.0 }
                    }
                },                
                {
                    "mediaRangoLatencia",
                    new PercentilesAggregation("mediaRangoLatencia", "message.rangoLatencia") {
                        Percents = new[] { 50.0 }
                    }
                },
                {
                    "mediaRangoIntensidadSenial",
                    new PercentilesAggregation("mediaRangoIntensidadSenial", "message.rangoIntensidadSenial") {
                    Percents = new[] { 50.0 }
                    }
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
                    "coordenadax5000",
                    new MaxAggregation("coordenadax5000", "message.coordenadax5000")
                }
                ,
                {
                    "coordenaday5000",
                    new MaxAggregation("coordenaday5000", "message.coordenaday5000")
                }
                                ,
                {
                    "coordenadax20000",
                    new MaxAggregation("coordenadax20000", "message.coordenadax20000")
                }
                ,
                {
                    "coordenaday20000",
                    new MaxAggregation("coordenaday20000", "message.coordenaday20000")
                }
                ,
                {
                    "categoria",
                    new TopMetricsAggregation("categoria")
                    {
                        Metrics = new List<ITopMetricsValue>
                        {
                            new TopMetricsValue("message.categoria.keyword")
                        },
                        Size = 1,
                        Sort = new List<ISort> { new FieldSort { Field = "message.timestamp_seconds", Order = SortOrder.Ascending } }
                    }
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
                ,
                {
                    "arrCategorias",
                    new TermsAggregation("arrCategorias")
                    {
                        Field = "message.categoria.keyword"
                    }
                },
                {
                    "arrTipoRed",
                    new TermsAggregation("arrTipoRed")
                    {
                        Field = "message.tipoRed.keyword"
                    }
                }
            };

            /* AggregationDictionary oAgregadoNested = new AggregationDictionary
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
                     "mediaRangoCalidad",
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
             };*/

            AggregationDictionary oAgregadoPrincipal = null;

            if (iRangoAgrupado == 500)
            {
                oAgregadoPrincipal = new AggregationDictionary
                {
                    {
                        "agregado",
                        new TermsAggregation("agregado")
                        {
                            Field = "message.agrupado_cuadricula_500_categoria.keyword",
                            Aggregations = oAgregadoNested,
                            Size = 150000
                        }
                    }
                };
            }
            else if (iRangoAgrupado == 5000)
            {
                oAgregadoPrincipal = new AggregationDictionary
                {
                    {
                        "agregado",
                        new TermsAggregation("agregado")
                        {
                            Field = "message.agrupado_cuadricula_5000_categoria.keyword",
                            Aggregations = oAgregadoNested,
                            Size = 150000
                        }
                    }
                };
            }
            else if (iRangoAgrupado == 20000)
            {
                oAgregadoPrincipal = new AggregationDictionary
                {
                    {
                        "agregado",
                        new TermsAggregation("agregado")
                        {
                            Field = "message.agrupado_cuadricula_20000_categoria.keyword",
                            Aggregations = oAgregadoNested,
                            Size = 150000
                        }
                    }
                };
            }

            ISearchResponse<CoberturaReport> response = null;

            if (dFechaUltimaEjecucionCron.HasValue)
            {
                /* AggregationDictionary oAgregadoPrincipalSinNested = new AggregationDictionary
                 {
                     {
                         "agregado",
                         new TermsAggregation("agregado")
                         {
                             Field = "message.agrupado_cuadricula_tecnologia.keyword",
                             Size = 10000
                         }
                     }
                 };*/
                AggregationDictionary oAgregadoPrincipalSinNested = null;
                Field oFieldKey = null;

                if (iRangoAgrupado == 500)
                {
                    oAgregadoPrincipalSinNested = new AggregationDictionary
                    {
                        {
                             "agregado",
                             new TermsAggregation("agregado")
                             {
                                 Field = "message.agrupado_cuadricula_500_categoria.keyword",
                                 Size = 150000
                             }
                        }
                    };

                    oFieldKey = new Nest.Field("message.agrupado_cuadricula_500_categoria.keyword");
                }
                else if (iRangoAgrupado == 5000)
                {
                    oAgregadoPrincipalSinNested = new AggregationDictionary
                    {
                        {
                             "agregado",
                             new TermsAggregation("agregado")
                             {
                                 Field = "message.agrupado_cuadricula_5000_categoria.keyword",
                                 Size = 150000
                             }
                        }
                    };

                    oFieldKey = new Nest.Field("message.agrupado_cuadricula_5000_categoria.keyword");
                }
                else if (iRangoAgrupado == 20000)
                {
                    oAgregadoPrincipalSinNested = new AggregationDictionary
                    {
                        {
                             "agregado",
                             new TermsAggregation("agregado")
                             {
                                 Field = "message.agrupado_cuadricula_20000_categoria.keyword",
                                 Size = 150000
                             }
                        }
                    };

                    oFieldKey = new Nest.Field("message.agrupado_cuadricula_20000_categoria.keyword");
                }

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
                                                Field = oFieldKey,
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
                if (response.Aggregations.Count > 0 && response.Aggregations["agregado"] != null)
                {
                    var responseAggs = response.Aggregations.Terms("agregado");

                    foreach (var childAgg in responseAggs.Buckets)
                    {
                        CoberturaReportAgrupadoCuadricula oCoberturaReportAgrupadoCuadriculaTecnologia = new CoberturaReportAgrupadoCuadricula();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.clave = childAgg.Key;

                        /*Nest.ValueAggregate vaMediaVelocidadSubida = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaVelocidadSubida").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMediaVelocidadBajada = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaVelocidadBajada").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMediaLatencia = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaLatencia").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMediaValorIntensidadSenial = (Nest.ValueAggregate) childAgg.Where(x => x.Key == "mediaValorIntensidadSenial").Select(x => x.Value).FirstOrDefault();*/
                        Nest.PercentilesAggregate vaMediaVelocidadSubida = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaVelocidadSubida").Select(x => x.Value).FirstOrDefault();
                        Nest.PercentilesAggregate vaMediaVelocidadBajada = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaVelocidadBajada").Select(x => x.Value).FirstOrDefault();
                        Nest.PercentilesAggregate vaMediaLatencia = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaLatencia").Select(x => x.Value).FirstOrDefault();
                        Nest.PercentilesAggregate vaMediaValorIntensidadSenial = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaValorIntensidadSenial").Select(x => x.Value).FirstOrDefault();

                        Nest.ValueAggregate vaMinTimestamp = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "minTimestamp").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaMaxTimestamp = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "maxTimestamp").Select(x => x.Value).FirstOrDefault();

                        Nest.ValueAggregate vaCoordenadaX = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenadax").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaCoordenadaY = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenaday").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaCoordenadaX5000 = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenadax5000").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaCoordenadaY5000 = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenaday5000").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaCoordenadaX20000 = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenadax20000").Select(x => x.Value).FirstOrDefault();
                        Nest.ValueAggregate vaCoordenadaY20000 = (Nest.ValueAggregate)childAgg.Where(x => x.Key == "coordenaday20000").Select(x => x.Value).FirstOrDefault();

                        Nest.TopMetricsAggregate vaCategoria = (Nest.TopMetricsAggregate)childAgg.Where(x => x.Key == "categoria").Select(x => x.Value).FirstOrDefault();

                        Nest.TopMetricsAggregate vaTipoRed = (Nest.TopMetricsAggregate)childAgg.Where(x => x.Key == "tipoRed").Select(x => x.Value).FirstOrDefault();
                        Nest.GeoBoundsAggregate vaLocation = (Nest.GeoBoundsAggregate)childAgg.Where(x => x.Key == "locationRaw").Select(x => x.Value).FirstOrDefault();
                        Nest.BucketAggregate vaMunicipios = (Nest.BucketAggregate)childAgg.Where(x => x.Key == "arrMunicipios").Select(x => x.Value).FirstOrDefault();
                        //Nest.BucketAggregate vaCategorias = (Nest.BucketAggregate)childAgg.Where(x => x.Key == "arrCategorias").Select(x => x.Value).FirstOrDefault();
                        Nest.BucketAggregate vaArrTipoRed = (Nest.BucketAggregate)childAgg.Where(x => x.Key == "arrTipoRed").Select(x => x.Value).FirstOrDefault();

                        Nest.PercentilesAggregate vaMediaRangoVelocidadSubida = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaRangoVelocidadSubida").Select(x => x.Value).FirstOrDefault();
                        Nest.PercentilesAggregate vaMediaRangoVelocidadBajada = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaRangoVelocidadBajada").Select(x => x.Value).FirstOrDefault();
                        Nest.PercentilesAggregate vaMediaRangoLatencia = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaRangoLatencia").Select(x => x.Value).FirstOrDefault();
                        Nest.PercentilesAggregate vaMediaRangoIntensidadSenial = (Nest.PercentilesAggregate)childAgg.Where(x => x.Key == "mediaRangoIntensidadSenial").Select(x => x.Value).FirstOrDefault();

                        /*oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida = (decimal?) vaMediaVelocidadSubida.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada = (decimal?) vaMediaVelocidadBajada.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia = (decimal?) vaMediaLatencia.Value;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial = (decimal?)vaMediaValorIntensidadSenial.Value;*/
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida = vaMediaVelocidadSubida.Items != null && vaMediaVelocidadSubida.Items.Count > 0 ? (decimal?)vaMediaVelocidadSubida.Items[0].Value : null;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada = vaMediaVelocidadBajada.Items != null && vaMediaVelocidadBajada.Items.Count > 0 ? (decimal?)vaMediaVelocidadBajada.Items[0].Value : null;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia = vaMediaLatencia.Items != null && vaMediaLatencia.Items.Count > 0 ? (decimal?)vaMediaLatencia.Items[0].Value : null;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial = vaMediaValorIntensidadSenial.Items != null && vaMediaValorIntensidadSenial.Items.Count > 0 ? (decimal?)vaMediaValorIntensidadSenial.Items[0].Value : null;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.cantidadMedidas = childAgg.DocCount.HasValue ? childAgg.DocCount.Value : 0;

                        if (iRangoAgrupado == 500)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.coodenadax = (double)vaCoordenadaX.Value;
                            oCoberturaReportAgrupadoCuadriculaTecnologia.coodenaday = (double)vaCoordenadaY.Value;
                        }
                        else if (iRangoAgrupado == 5000)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.coodenadax = (double)vaCoordenadaX5000.Value;
                            oCoberturaReportAgrupadoCuadriculaTecnologia.coodenaday = (double)vaCoordenadaY5000.Value;
                        }
                        else if (iRangoAgrupado == 20000)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.coodenadax = (double)vaCoordenadaX20000.Value;
                            oCoberturaReportAgrupadoCuadriculaTecnologia.coodenaday = (double)vaCoordenadaY20000.Value;
                        }

                        oCoberturaReportAgrupadoCuadriculaTecnologia.location = new CoberturaAgrupadoCuaLocation();
                        /*if (vaLocation.Bounds != null && vaLocation.Bounds.TopLeft != null)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.location.lat = (double)vaLocation.Bounds.TopLeft.Lat;
                            oCoberturaReportAgrupadoCuadriculaTecnologia.location.lon = (double)vaLocation.Bounds.TopLeft.Lon;
                        }*/

                        Location oLocation = new Location();

                        oLocation.InverseReproject((double)oCoberturaReportAgrupadoCuadriculaTecnologia.coodenadax, (double)oCoberturaReportAgrupadoCuadriculaTecnologia.coodenaday);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.location.lat = oLocation.geo_x;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.location.lon = oLocation.geo_y;

                        List<string> sListMunicipios = new List<string>();

                        if (vaMunicipios != null && vaMunicipios.Items.Count > 0)
                        {
                            foreach (var municipioBucket in vaMunicipios.Items)
                            {
                                string sMunicipio = ((KeyedBucket<object>)municipioBucket).Key.ToString();

                                if (!string.IsNullOrEmpty(sMunicipio))
                                {
                                    if (!sListMunicipios.Contains(sMunicipio))
                                    {
                                        sListMunicipios.Add(sMunicipio);
                                    }
                                }
                            }
                        }

                        sListMunicipios = sListMunicipios.OrderBy(x => x).ToList();

                        oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrMunicipios = sListMunicipios.ToArray();

                        /*List<string> sListCategorias = new List<string>();

                        if (vaCategorias != null && vaCategorias.Items.Count > 0)
                        {
                            foreach (var categoriaBucket in vaCategorias.Items)
                            {
                                string sCategoria = ((KeyedBucket<object>)categoriaBucket).Key.ToString();

                                if (!string.IsNullOrEmpty(sCategoria))
                                {
                                    if (!sListCategorias.Contains(sCategoria))
                                    {
                                        sListCategorias.Add(sCategoria);
                                    }
                                }
                            }
                        }

                        sListCategorias = sListCategorias.OrderBy(x => x).ToList();*/
                        string sCategoria = Constantes.RED_MOVIL;

                        if (vaCategoria.Top != null && vaCategoria.Top.Count > 0 && vaCategoria.Top.FirstOrDefault() != null && vaCategoria.Top.FirstOrDefault().Metrics != null && vaCategoria.Top.FirstOrDefault().Metrics.Count > 0)
                        {
                            sCategoria = vaCategoria.Top.FirstOrDefault().Metrics.FirstOrDefault().Value.ToString();
                        }

                        List<string> sListCategorias = new List<string>();

                        sListCategorias.Add(sCategoria);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrCategorias = sListCategorias.ToArray();

                        List<string> sListTiposRed = new List<string>();

                        if (vaArrTipoRed != null && vaArrTipoRed.Items.Count > 0)
                        {
                            foreach (var tipoRedbucket in vaArrTipoRed.Items)
                            {
                                string sTipoRed = ((KeyedBucket<object>)tipoRedbucket).Key.ToString();

                                if (!string.IsNullOrEmpty(sTipoRed))
                                {
                                    if (!sListTiposRed.Contains(sTipoRed))
                                    {
                                        sListTiposRed.Add(sTipoRed);
                                    }
                                }
                            }
                        }

                        sListTiposRed = sListTiposRed.OrderBy(x => x).ToList();

                        oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrTipoRed = sListTiposRed.ToArray();

                        if (vaTipoRed.Top != null && vaTipoRed.Top.Count > 0 && vaTipoRed.Top.FirstOrDefault() != null && vaTipoRed.Top.FirstOrDefault().Metrics != null && vaTipoRed.Top.FirstOrDefault().Metrics.Count > 0)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.message.tipoRed = vaTipoRed.Top.FirstOrDefault().Metrics.FirstOrDefault().Value.ToString();
                        }

                        if (vaMinTimestamp.Value.HasValue)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.minTimestamp = Helper.EpochMillisToDateTime((long)vaMinTimestamp.Value.Value);

                        }

                        if (vaMaxTimestamp.Value.HasValue)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.maxTimestamp = Helper.EpochMillisToDateTime((long)vaMaxTimestamp.Value.Value);
                        }

                        APIHelper oAPIHelper = new APIHelper(null, null);
                        //EnumRango.Intensidad oRangoIntensidad = EnumRango.calcularRangoIntensidad(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial, oCoberturaReportAgrupadoCuadriculaTecnologia.message.tipoRed);

                        //oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoIntensidadSenial = (int)oRangoIntensidad;

                        //campos tooltip mapa

                        /*EnumRango.VelocidadBajada oRangoBajada = EnumRango.calcularRangoVelocidadBajada(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoVelocidadBajada = vaMediaRangoVelocidadBajada.Items != null && vaMediaRangoVelocidadBajada.Items.Count > 0 ? (int)vaMediaRangoVelocidadBajada.Items[0].Value : 0;

                        string textoRangoVelocidadBajada = EnumRango.getStringValue(oRangoBajada);
                        string textoVelocidadBajada = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.HasValue ? textoRangoVelocidadBajada + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;
                        
                        oCoberturaReportAgrupadoCuadriculaTecnologia.message.textoRangoVelocidadBajada = ((int)oRangoBajada).ToString() + " - " + textoRangoVelocidadBajada;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Calidad = oCoberturaReportAgrupadoCuadriculaTecnologia.message.textoRangoVelocidadBajada;

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
                        string textoIntensidad = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial.HasValue ? textoRangoIntensidad + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial.Value, 0, MidpointRounding.AwayFromZero).ToString() + " dBm)" : Constantes.CAMPO_SIN_DATOS;
                        
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Intensidad = textoIntensidad;*/

                        if (!oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.HasValue)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada = 0;
                        }

                        EnumRango.VelocidadBajada oRangoBajada = EnumRango.calcularRangoVelocidadBajada(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada, sCategoria);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoVelocidadBajada = (int)oRangoBajada;

                        string textoRangoVelocidadBajada = EnumRango.getStringValue(oRangoBajada);

                        string textoVelocidadBajada = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.HasValue ? textoRangoVelocidadBajada + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadBajada.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.message.textoRangoVelocidadBajada = ((int)oRangoBajada).ToString() + " - " + textoRangoVelocidadBajada;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Calidad = oCoberturaReportAgrupadoCuadriculaTecnologia.message.textoRangoVelocidadBajada;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Velocidad__Bajada = textoVelocidadBajada;

                        if (!oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida.HasValue)
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida = 0;
                        }

                        EnumRango.VelocidadSubida oRangoSubida = EnumRango.calcularRangoVelocidadSubida(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida, sCategoria);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoVelocidadSubida = (int)oRangoSubida;

                        string textoRangoVelocidadSubida = EnumRango.getStringValue(oRangoSubida);

                        string textoVelocidadSubida = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida.HasValue ? textoRangoVelocidadSubida + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaVelocidadSubida.Value, 2, MidpointRounding.AwayFromZero).ToString() + " Mbps)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Velocidad__Subida = textoVelocidadSubida;

                        EnumRango.Latencia oRangoLatencia = EnumRango.calcularRangoLatencia(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia, sCategoria);

                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoLatencia = (int)oRangoLatencia;

                        string tenxtoRangoLatencia = EnumRango.getStringValue(oRangoLatencia);
                        string textoLatencia = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia.HasValue ? tenxtoRangoLatencia + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaLatencia.Value, 0, MidpointRounding.AwayFromZero).ToString() + " ms)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Latencia = textoLatencia;


                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoIntensidadSenial = vaMediaRangoIntensidadSenial.Items != null && vaMediaRangoIntensidadSenial.Items.Count > 0 ? (int)vaMediaRangoIntensidadSenial.Items[0].Value : 0;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoIntensidadSenial = EnumRango.normalizarRangoIntensidad(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoIntensidadSenial);

                        EnumRango.Intensidad oRangoIntensidad = (EnumRango.Intensidad)oCoberturaReportAgrupadoCuadriculaTecnologia.mediaRangoIntensidadSenial;

                        string textoRangoIntensidad = EnumRango.getStringValue(oRangoIntensidad);
                        string textoIntensidad = oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial.HasValue ? textoRangoIntensidad + " (" + Decimal.Round(oCoberturaReportAgrupadoCuadriculaTecnologia.mediaValorIntensidadSenial.Value, 0, MidpointRounding.AwayFromZero).ToString() + " dBm)" : Constantes.CAMPO_SIN_DATOS;

                        oCoberturaReportAgrupadoCuadriculaTecnologia.Intensidad = textoIntensidad;


                        oCoberturaReportAgrupadoCuadriculaTecnologia.Cantidad__Mediciones = oCoberturaReportAgrupadoCuadriculaTecnologia.cantidadMedidas.ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Datos__Desde = oCoberturaReportAgrupadoCuadriculaTecnologia.minTimestamp.ToString("dd/MM/yyyy");
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Datos__Hasta = oCoberturaReportAgrupadoCuadriculaTecnologia.maxTimestamp.ToString("dd/MM/yyyy");
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Municipios = oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrMunicipios.Length > 0 ? string.Join(", ", oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrMunicipios) : string.Empty;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Categoria = oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrCategorias.Length > 0 ? string.Join(", ", oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrCategorias) : string.Empty;
                        oCoberturaReportAgrupadoCuadriculaTecnologia.Tipo__Red = oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrTipoRed.Length > 0 ? string.Join(", ", oCoberturaReportAgrupadoCuadriculaTecnologia.message.arrTipoRed) : string.Empty;

                        if (string.IsNullOrEmpty(oCoberturaReportAgrupadoCuadriculaTecnologia.Tipo__Red))
                        {
                            oCoberturaReportAgrupadoCuadriculaTecnologia.Tipo__Red = !string.IsNullOrEmpty(oCoberturaReportAgrupadoCuadriculaTecnologia.message.tipoRed) ? oCoberturaReportAgrupadoCuadriculaTecnologia.message.tipoRed : Constantes.CAMPO_SIN_DATOS;
                        }

                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoVelocidadBajada = EnumRango.obtenerIconoRangoVelocidadBajada(oRangoBajada).ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoVelocidadSubida = EnumRango.obtenerIconoRangoVelocidadSubida(oRangoSubida).ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoLatencia = EnumRango.obtenerRangoIconoLatencia(oRangoLatencia).ToString();
                        oCoberturaReportAgrupadoCuadriculaTecnologia.iconoIntensidad = EnumRango.obtenerIconoRangoIntensidad(oRangoIntensidad).ToString();

                        listDocuments.Add(oCoberturaReportAgrupadoCuadriculaTecnologia);
                    }
                }
            }
            else {
                listDocuments = null;
            }

            return listDocuments;
        }

        public async Task<ReportesAccesibilidadResponse> ObtenerReportesPaginado(string sIndexMappingNameBuscar, int iSkip, int iTake, string sortBy, string sortOrder, List<string> municipios, List<string> categorias, List<string> calidades)
        {
            ReportesAccesibilidadResponse oResponse = new ReportesAccesibilidadResponse();

            string sIndexNameUsar = GetIndexNameForType(sIndexMappingNameBuscar);

            ElasticClient client = CreateClient();

            /*var response = await client.SearchAsync<CoberturaReport>(s => s.Index(sIndexNameUsar)
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
                                .Field("timestamp_seconds")
                                .Order(SortOrder.Descending)
                            )
                        )
                    .Skip(iSkip)
                    .Take(iTake)
                 );*/

            SearchDescriptor<CoberturaReport> oSearch = new SearchDescriptor<CoberturaReport>();
            CountDescriptor<CoberturaReport> oCount = new CountDescriptor<CoberturaReport>();

            oSearch = oSearch.Index(sIndexNameUsar);
            oCount = oCount.Index(sIndexNameUsar);

            SortOrder oSortOrder = SortOrder.Descending;

            if (municipios.Count > 0 || categorias.Count > 0 || calidades.Count > 0) {

                List<QueryContainer> oListQueryContainer = new List<QueryContainer>();

                Field oFieldKey = null;

                if (municipios.Count > 0)
                {
                    oFieldKey = new Nest.Field("Municipio.keyword");

                    QueryContainer oQueryContainer = new QueryContainer(new TermsQuery()
                    {
                        Field = oFieldKey,
                        Terms = municipios.ToArray()
                    });

                    oListQueryContainer.Add(oQueryContainer);
                }

                if (categorias.Count > 0)
                {
                    oFieldKey = new Nest.Field("Categoria.keyword");

                    QueryContainer oQueryContainer = new QueryContainer(new TermsQuery()
                    {
                        Field = oFieldKey,
                        Terms = categorias.ToArray()
                    });

                    oListQueryContainer.Add(oQueryContainer);
                }

                if (calidades.Count > 0)
                {
                    oFieldKey = new Nest.Field("Calidad.keyword");

                    QueryContainer oQueryContainer = new QueryContainer(new TermsQuery()
                    {
                        Field = oFieldKey,
                        Terms = calidades.ToArray()
                    });

                    oListQueryContainer.Add(oQueryContainer);
                }
                oCount.Query(q => q
                    .Bool(b => b
                        .Must(oListQueryContainer.ToArray()))
                );

                oSearch.Query(q => q
                    .Bool(b => b
                        .Must(oListQueryContainer.ToArray()))
                );
            }

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                if (sortOrder.ToLower() == "asc")
                {
                    oSortOrder = SortOrder.Ascending;
                }

                if (sortBy == "Fecha")
                {
                    sortBy = "message.timestamp_seconds";
                }
                else if (sortBy == "Municipio") {
                    sortBy = "Municipio.keyword";
                }
                else if (sortBy == "Categoria")
                {
                    sortBy = "Categoria.keyword";
                }
                else if (sortBy == "Calidad")
                {
                    sortBy = "Calidad.keyword";
                }
                else if (sortBy == "Velocidad__Bajada")
                {
                    sortBy = "message.velocidadBajada";
                }

                oSearch.Sort(so => so
                    .Field(fs => fs
                        .Field(sortBy)
                        .Order(oSortOrder)
                    )
                    /*.Field(fs => fs
                        .Field("_id")
                        .Order(SortOrder.Descending)
                    )*/
                );

            }
            else
            {
                sortBy = "message.timestamp_seconds";

                oSearch.Sort(so => so
                     .Field(fs => fs
                         .Field(sortBy)
                         .Order(oSortOrder)
                     )
                    /*.Field(fs => fs
                        .Field("_id")
                        .Order(SortOrder.Descending)
                    )*/
                );
            }

            oSearch.Skip(iSkip);
            oSearch.Take(iTake);

            /*if (oCoberturaPaginatorHelper != null) {
                oSearch.SearchAfter(new List<object> { oCoberturaPaginatorHelper.lastSort, oCoberturaPaginatorHelper.lastId });
                oSearch.TrackScores(true);
            }*/

            var countResponse = await client.CountAsync<CoberturaReport>(x=> x = oCount);
            var searchResponse = await client.SearchAsync<CoberturaReport>(oSearch);

            long? lTotalRegistros = null;

            if (countResponse != null && countResponse.IsValid)
            {
                lTotalRegistros = countResponse.Count;
            }

            if (searchResponse != null && searchResponse.IsValid)
            {
                if (searchResponse.Documents.Count > 0)
                {
                    oResponse.paginator = new CoberturaPaginatorHelper();
                    oResponse.paginator.lastId = searchResponse.Hits.Last().Id;

                    if (lTotalRegistros.HasValue)
                    {
                        oResponse.total = lTotalRegistros.Value;
                    }
                    else {
                        oResponse.total = searchResponse.Total;
                    }

                    List<CoberturaReport> oListDocumentosRecibidos = searchResponse.Documents.ToList();
                    List<DatosAccesibilidad> oListDatosDevolver = new List<DatosAccesibilidad>();

                    if (oListDocumentosRecibidos.Count > 0) {

                        if (sortBy == "Fecha")
                        {
                            oResponse.paginator.lastSort = searchResponse.Documents.Last().message.timestamp_seconds;
                        }
                        else if (sortBy == "Municipio")
                        {
                            oResponse.paginator.lastSort = searchResponse.Documents.Last().Municipio;
                        }
                        else if (sortBy == "Categoria")
                        {
                            oResponse.paginator.lastSort = searchResponse.Documents.Last().Categoria;
                        }
                        else if (sortBy == "Calidad")
                        {
                            oResponse.paginator.lastSort = searchResponse.Documents.Last().Calidad;
                        }
                        else if (sortBy == "Velocidad__Bajada")
                        {
                            oResponse.paginator.lastSort = searchResponse.Documents.Last().message.velocidadBajada.HasValue ? searchResponse.Documents.Last().message.velocidadBajada.Value.ToString("0.##") : null;
                        }

                        foreach (CoberturaReport oReport in oListDocumentosRecibidos)
                        {
                            DatosAccesibilidad oDatosAccesibilidad = new DatosAccesibilidad();

                            oDatosAccesibilidad.Fecha = oReport.Fecha;
                            oDatosAccesibilidad.Municipio = oReport.Municipio;
                            oDatosAccesibilidad.Categoria = oReport.Categoria;
                            oDatosAccesibilidad.Calidad = oReport.Calidad;
                            oDatosAccesibilidad.Velocidad__Bajada = oReport.message.velocidadBajada.HasValue ? oReport.message.velocidadBajada.Value.ToString("0.##") + " Mbps" : string.Empty;

                            oListDatosDevolver.Add(oDatosAccesibilidad);
                        }
                    }

                    oResponse.documents = oListDatosDevolver;
                }
            }

            return oResponse;
        }


        public ReportesFiltradosResponse ObtenerReportesFiltrados(string sIndexMappingNameBuscar, DateTime? dFechaDesde, DateTime? dFechaHasta, string sMunicipio, string sCodigoINE) {

            ReportesFiltradosResponse oResponse = new ReportesFiltradosResponse();

            oResponse.documents = new List<DatosCobertura>();

            string sIndexNameUsar = GetIndexNameForType(sIndexMappingNameBuscar);
            string sTimeoutScroll = "1m"; //timeout scroll
            //sTimeoutScroll = "30s";

            ElasticClient client = CreateClient();

            SearchDescriptor<CoberturaReport> oSearch = new SearchDescriptor<CoberturaReport>();

            oSearch = oSearch.Index(sIndexNameUsar);

            List<QueryContainer> oListQueryContainer = new List<QueryContainer>();
            List<QueryContainerDescriptor<DatosCobertura>> oListQueryContainerDescriptor = new List<QueryContainerDescriptor<DatosCobertura>>();

            if (dFechaDesde.HasValue || dFechaHasta.HasValue)
            {
                Field oFieldKey = new Nest.Field("message.timestamp_seconds");

                DateRangeQuery oDateRangeQuery = new DateRangeQuery();

                oDateRangeQuery.Field = oFieldKey;

                if (dFechaDesde.HasValue) {

                    string sSegundosFechaDesde = Helper.DateTimeToEpochSeconds(dFechaDesde.Value);
                    sSegundosFechaDesde = sSegundosFechaDesde.Replace(",", ".");

                    oDateRangeQuery.GreaterThanOrEqualTo = sSegundosFechaDesde;
                }

                if (dFechaHasta.HasValue) {
                    string sSegundosFechaHasta = Helper.DateTimeToEpochSeconds(dFechaHasta.Value);
                    sSegundosFechaHasta = sSegundosFechaHasta.Replace(",", ".");

                    oDateRangeQuery.LessThanOrEqualTo = sSegundosFechaHasta;
                }

                QueryContainerDescriptor<DatosCobertura> oQueryContainerDescriptor = new QueryContainerDescriptor<DatosCobertura>();

                oQueryContainerDescriptor.DateRange(dr=> oDateRangeQuery);

                oListQueryContainerDescriptor.Add(oQueryContainerDescriptor);
            }

            if (!string.IsNullOrEmpty(sMunicipio))
            {
                sMunicipio = sMunicipio.ToLower();
                sMunicipio = sMunicipio.Replace("*", "");
                sMunicipio = "*" + sMunicipio + "*";

                Field oFieldKey = new Nest.Field("message.municipio");

                QueryContainer oQueryContainer = new QueryContainer(new WildcardQuery()
                {
                    Field = oFieldKey,
                    Value = sMunicipio
                });

                oListQueryContainer.Add(oQueryContainer);
            }

            if (!string.IsNullOrEmpty(sCodigoINE))
            {
                sCodigoINE = sCodigoINE.ToLower();

                Field oFieldKey = new Nest.Field("message.ine");

                QueryContainer oQueryContainer = new QueryContainer(new PrefixQuery()
                {
                    Field = oFieldKey,
                    Value = sCodigoINE
                });

                oListQueryContainer.Add(oQueryContainer);
            }

            BoolQueryDescriptor<DatosCobertura> oBoolQueryDescriptor = null;

            if (oListQueryContainer.Count > 0 || oListQueryContainerDescriptor.Count > 0)
            {
                oBoolQueryDescriptor = new BoolQueryDescriptor<DatosCobertura>();

                if (oListQueryContainer.Count > 0) {
                    oBoolQueryDescriptor.Must(oListQueryContainer.ToArray());
                }

                if (oListQueryContainerDescriptor.Count > 0)
                {
                    oBoolQueryDescriptor.Filter(oListQueryContainerDescriptor.ToArray());
                }
            }

            if (oBoolQueryDescriptor != null)
            {
                oSearch.Query(q => q
                        .Bool(b => oBoolQueryDescriptor)
                );
            }
            else {
                oSearch.MatchAll();
            }

            oSearch.From(0);
            oSearch.Size(10000);

            oSearch.Source(sf => sf
                .Includes(i => i
                    .Fields(f => f.message.categoria, f => f.Calidad, f => f.message.municipio, f => f.message.ine, f => f.message.modelo, f => f.message.so
                    , f => f.message.tipoRed, f => f.message.operador, f => f.message.coordenadax, f => f.message.coordenaday, f => f.message.location
                    , f => f.message.valorIntensidadSenial, f => f.message.rangoIntensidadSenial, f => f.message.velocidadBajada, f => f.message.rangoVelocidadBajada
                    , f => f.message.velocidadSubida, f => f.message.rangoVelocidadSubida, f => f.message.latencia, f => f.message.rangoLatencia, f => f.message.timestamp_seconds)
                )
            );           

            oSearch.Scroll(sTimeoutScroll);

            var searchResponse = client.Search<CoberturaReport>(oSearch);

            if (searchResponse != null && searchResponse.IsValid) {
                while (searchResponse.Documents.Any())
                {
                    List<CoberturaReport> oListDocumentosRecibidos = searchResponse.Documents.ToList();
                    List<DatosCobertura> oListDatosDevolver = new List<DatosCobertura>();

                    if (oListDocumentosRecibidos.Count > 0)
                    {
                        foreach (CoberturaReport oReport in oListDocumentosRecibidos)
                        {
                            DatosCobertura oDatosReportesAPIPublica = new DatosCobertura();

                            oDatosReportesAPIPublica.fecha = Helper.EpochSecondsToDateTime(oReport.message.timestamp_seconds);

                            oDatosReportesAPIPublica.categoria = oReport.message.categoria;
                            oDatosReportesAPIPublica.calidad = oReport.Calidad;
                            oDatosReportesAPIPublica.municipio = oReport.message.municipio;
                            oDatosReportesAPIPublica.ine = oReport.message.ine;
                            oDatosReportesAPIPublica.modelo = oReport.message.modelo;
                            oDatosReportesAPIPublica.so = oReport.message.so;
                            oDatosReportesAPIPublica.tipoRed = oReport.message.tipoRed;
                            oDatosReportesAPIPublica.operador = oReport.message.operador;

                            oDatosReportesAPIPublica.coordenadaX = oReport.message.coordenadax;
                            oDatosReportesAPIPublica.coordenadaY = oReport.message.coordenaday;
                            oDatosReportesAPIPublica.latitud = oReport.message.location.lat;
                            oDatosReportesAPIPublica.longitud = oReport.message.location.lon;

                            oDatosReportesAPIPublica.valorIntensidadSenial = oReport.message.valorIntensidadSenial;
                            oDatosReportesAPIPublica.rangoIntensidadSenial = oReport.message.rangoIntensidadSenial;
                            oDatosReportesAPIPublica.velocidadBajada = oReport.message.velocidadBajada;
                            oDatosReportesAPIPublica.rangoVelocidadBajada = oReport.message.rangoVelocidadBajada;
                            oDatosReportesAPIPublica.velocidadSubida = oReport.message.velocidadSubida;
                            oDatosReportesAPIPublica.rangoVelocidadSubida = oReport.message.rangoVelocidadSubida;
                            oDatosReportesAPIPublica.latencia = oReport.message.latencia;
                            oDatosReportesAPIPublica.rangoLatencia = oReport.message.rangoLatencia;

                            //oDatosReportesAPIPublica.velocidadBajada = oReport.message.velocidadBajada.HasValue ? oReport.message.velocidadBajada.Value.ToString("0.##") + " Mbps" : string.Empty;

                            oListDatosDevolver.Add(oDatosReportesAPIPublica);
                        }

                        oResponse.documents.AddRange(oListDatosDevolver);
                    }

                    searchResponse = client.Scroll<CoberturaReport>(sTimeoutScroll, searchResponse.ScrollId);
                }
            }

            oResponse.documents = oResponse.documents.OrderByDescending(x => x.fecha).ToList();

            return oResponse;
        }

        
        public async Task<bool> AjustarTipoRed(string sMappingProperty = null)
        {

            string sIndexName = GetIndexNameForType("CoberturaReport");

            if (!string.IsNullOrEmpty(sMappingProperty))
            {
                sIndexName = GetIndexNameForType(sMappingProperty);
            }

            ElasticClient client = CreateClient();

            AggregationDictionary oAgregadoNested = new AggregationDictionary
            {
                {
                    "arrTipoRed",
                    new TermsAggregation("arrTipoRed")
                    {
                        Field = "message.tipoRed.keyword"
                    }
                }
            };

            AggregationDictionary oAgregadoPrincipal = null;
            Field oFieldKeyAgrupar = new Nest.Field("message.agrupado_cuadricula_fecha.keyword");

            oAgregadoPrincipal = new AggregationDictionary
                {
                    {
                        "agregado",
                        new TermsAggregation("agregado")
                        {
                            Field = oFieldKeyAgrupar,
                            Aggregations = oAgregadoNested,
                            Size = 150000
                        }
                    }
                };

            ISearchResponse<CoberturaReport> response = null;
            Field oFieldKeyCategoria = new Nest.Field("message.categoria.keyword");
            Field oFieldModelo = new Nest.Field("message.modelo.keyword");
            Field oFieldTipoRed = new Nest.Field("message.tipoRed.keyword");


            List<string> sListTiposRedModificar = new List<string>();
            sListTiposRedModificar.Add("2G");
            sListTiposRedModificar.Add("3G");
            sListTiposRedModificar.Add("4G");
            sListTiposRedModificar.Add("5G");

            response = await client.SearchAsync<CoberturaReport>(s => s.Index(sIndexName)
                 .Query(q => q
                    .Bool(b => b.MustNot(mt => mt.Exists(ex => ex.Field(oFieldModelo)))
                        .Must( m =>
                                new TermQuery()
                                {
                                    Field = oFieldKeyCategoria,
                                    Value = "RED MOVIL"
                                }, 
                                m=>
                                new TermsQuery()
                                {
                                    Field = oFieldTipoRed,
                                    Terms = sListTiposRedModificar.ToArray()
                                }
                            )
                        )
                    )
                .Take(0) //para traer solo los agregados
                .Aggregations(oAgregadoPrincipal)
            );

            if (response != null && response.IsValid)
            {
                Dictionary<string, List<string>> oDictionary = new Dictionary<string, List<string>>();

                if (response.Aggregations.Count > 0 && response.Aggregations["agregado"] != null)
                {
                    var responseAggs = response.Aggregations.Terms("agregado");

                    foreach (var childAgg in responseAggs.Buckets)
                    {
                        Nest.BucketAggregate vaArrTipoRed = (Nest.BucketAggregate)childAgg.Where(x => x.Key == "arrTipoRed").Select(x => x.Value).FirstOrDefault();

                        List<string> sListTiposRed = new List<string>();

                        if (vaArrTipoRed != null && vaArrTipoRed.Items.Count > 0)
                        {
                            foreach (var tipoRedbucket in vaArrTipoRed.Items)
                            {
                                string sTipoRed = ((KeyedBucket<object>)tipoRedbucket).Key.ToString();

                                if (!string.IsNullOrEmpty(sTipoRed))
                                {
                                    if (!sListTiposRed.Contains(sTipoRed))
                                    {
                                        sListTiposRed.Add(sTipoRed);
                                    }
                                }
                            }
                        }

                        sListTiposRed = sListTiposRed.OrderBy(x => x).ToList();

                        oDictionary.Add(childAgg.Key, sListTiposRed);
                    }
                }

                if (oDictionary.Count > 0) {

                    foreach (KeyValuePair<string, List<string>> oEntry in oDictionary)
                    {
                        if (!string.IsNullOrEmpty(oEntry.Key) && oEntry.Value != null && oEntry.Value.Count > 0) {
                            string key = oEntry.Key;
                            string sValue = String.Join(", ", oEntry.Value);

                            var updateResponse = await client.UpdateByQueryAsync<CoberturaReport>(uq => uq
                                .Index(sIndexName)
                                .Script(s => s
                                    .Source("ctx._source.message.tipoRedAux = '" + sValue + "'")
                                 )
                                .Query(q => q
                                    .Bool(b => b.MustNot(mt => mt.Exists(ex => ex.Field(oFieldModelo)))
                                        .Must(m =>
                                               new TermQuery()
                                               {
                                                   Field = oFieldKeyAgrupar,
                                                   Value = key
                                               },
                                               m =>
                                                new TermQuery()
                                                {
                                                    Field = oFieldKeyCategoria,
                                                    Value = "RED MOVIL"
                                                },
                                                m =>
                                                new TermsQuery()
                                                {
                                                    Field = oFieldTipoRed,
                                                    Terms = sListTiposRedModificar.ToArray()
                                                }
                                            )
                                        )
                                    )
                            );
                        }
                    }
                }
            }
            else {
                return false;
            }

            return true;
        }

        }
}
