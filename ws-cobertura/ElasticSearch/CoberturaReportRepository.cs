using Messaging.ElasticSearch;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ws_cobertura.httpapi.Model.Response;

namespace ws_cobertura.ElasticSearch
{
    public class CoberturaReportRepository
    {
        protected readonly CoberturaESHelper _esCoberturaHelper;

        public CoberturaReportRepository(CoberturaESHelper esCoberturaHelper)
        {
            _esCoberturaHelper = esCoberturaHelper;
        }

        public async Task<bool> AddorEditCoberturaReport(CoberturaReport coberturaReport)
        {
            Nest.IndexResponse response = null;
            try
            {
                response = await _esCoberturaHelper.AddToIndex<CoberturaReport>(coberturaReport);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "AddorEditCoberturaReport", ex.Message, true);
            }
            return response != null && response.IsValid;
        }
        public async Task<bool> AddCoberturaReport(CoberturaReport oCoberturaReport)
        {
            Nest.IndexResponse response = null;
            try
            {
                response = await _esCoberturaHelper.AddCoberturaReport(oCoberturaReport);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "AddCoberturaReporte", ex.Message, true);
            }

            return response != null && response.IsValid;
        }
        public async Task<bool> AddUltimaEjecucionCron(CoberturaCronUltimaEjecucion oCoberturaCronUltimaEjecucion)
        {
            Nest.IndexResponse response = null;
            try
            {
                response = await _esCoberturaHelper.AddUltimaEjecucionCron(oCoberturaCronUltimaEjecucion);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "AddUltimaEjecucionCron", ex.Message, true);
            }

            return response != null && response.IsValid;
        }

        public async Task<bool> AddorEditListCoberturaReportAgrupadoCuadricula(List<CoberturaReportAgrupadoCuadricula> oListReports, string sIndexMapping)
        {
            Nest.UpdateResponse<CoberturaReportAgrupadoCuadricula> response = null;
            try
            {
                response = await _esCoberturaHelper.AddListCoberturaReportAgrupadoCuadriculaToIndex(oListReports, sIndexMapping);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "AddorEditListCoberturaReportAgrupadoCuadricula", ex.Message, true);
            }

            return response != null && response.IsValid;
        }

        public async Task<bool> AddListCoberturaReport(List<CoberturaReport> oListReports, string sIndexMapping)
        {
            Nest.UpdateResponse<CoberturaReport> response = null;
            try
            {
                response = await _esCoberturaHelper.AddListCoberturaReportToIndex(oListReports, sIndexMapping);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "AddListCoberturaReport", ex.Message, true);
            }

            return response != null && response.IsValid;
        }
        
        public async Task<bool> DeleteIndexCoberturaReportAgrupadoCuadricula(string sIndexMapping)
        {
            Nest.DeleteIndexResponse response = null;
            try
            {
                response = await _esCoberturaHelper.DeleteIndexCoberturaReportAgrupadoCuadricula(sIndexMapping);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "DeleteIndexCoberturaReportAgrupadoCuadricula", ex.Message, true);
            }

            return response != null && response.IsValid;
        }

        public async Task<bool> GroupCoberturaReports()
        {
            bool bResult = false;

            try
            {
                
                DateTimeOffset? dUltimaFechaEjecucion500 = await _esCoberturaHelper.ObtenerUltimaEjecucionCorrectaCronPorIndexMapping("CoberturaReportAgrupadoCuadricula500m");

                List<CoberturaReportAgrupadoCuadricula> oListReportes = await _esCoberturaHelper.ObtenerReportesAgrupadosPorCuadricula(dUltimaFechaEjecucion500, 500);

                if (oListReportes != null)
                {
                    if (oListReportes.Count > 0)
                    {
                        bResult = await AddorEditListCoberturaReportAgrupadoCuadricula(oListReportes, "CoberturaReportAgrupadoCuadricula500m");
                    }
                    else
                    {
                        bResult = true;
                    }

                    if (bResult == true)
                    {
                        string sIndexName = _esCoberturaHelper.GetIndexNameForType("CoberturaReportAgrupadoCuadricula500m");

                        CoberturaCronUltimaEjecucion oCoberturaCronUltimaEjecucion = new CoberturaCronUltimaEjecucion();

                        oCoberturaCronUltimaEjecucion.reportid = Guid.NewGuid().ToString();
                        oCoberturaCronUltimaEjecucion.fecha_ultima_ejecucion = DateTimeOffset.Now;
                        oCoberturaCronUltimaEjecucion.nombre_index = sIndexName;

                        await AddUltimaEjecucionCron(oCoberturaCronUltimaEjecucion);
                    }
                }
                else {
                    Helper.VolcarAConsola("CoberturaReportRepository", "CoberturaReportAgrupadoCuadricula500m", "No ejecutado", true);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                Helper.VolcarAConsola("CoberturaReportRepository", "CoberturaReportAgrupadoCuadricula500m", ex.Message, true);
            }

            /*try
            {

                DateTimeOffset? dUltimaFechaEjecucion5000 = await _esCoberturaHelper.ObtenerUltimaEjecucionCorrectaCronPorIndexMapping("CoberturaReportAgrupadoCuadricula5000m");

                List<CoberturaReportAgrupadoCuadricula> oListReportes = await _esCoberturaHelper.ObtenerReportesAgrupadosPorCuadricula(dUltimaFechaEjecucion5000, 5000);

                if (oListReportes.Count > 0)
                {
                    bResult = await AddorEditListCoberturaReportAgrupadoCuadricula(oListReportes, "CoberturaReportAgrupadoCuadricula5000m");
                }
                else
                {
                    bResult = true;
                }

                if (bResult == true)
                {
                    string sIndexName = _esCoberturaHelper.GetIndexNameForType("CoberturaReportAgrupadoCuadricula5000m");

                    CoberturaCronUltimaEjecucion oCoberturaCronUltimaEjecucion = new CoberturaCronUltimaEjecucion();

                    oCoberturaCronUltimaEjecucion.reportid = Guid.NewGuid().ToString();
                    oCoberturaCronUltimaEjecucion.fecha_ultima_ejecucion = DateTimeOffset.Now;
                    oCoberturaCronUltimaEjecucion.nombre_index = sIndexName;

                    await AddUltimaEjecucionCron(oCoberturaCronUltimaEjecucion);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                Helper.VolcarAConsola("CoberturaReportRepository", "CoberturaReportAgrupadoCuadricula5000m", ex.Message, true);
            }

            try
            {
                DateTimeOffset? dUltimaFechaEjecucion20000 = await _esCoberturaHelper.ObtenerUltimaEjecucionCorrectaCronPorIndexMapping("CoberturaReportAgrupadoCuadricula20000m");

                List<CoberturaReportAgrupadoCuadricula> oListReportes = await _esCoberturaHelper.ObtenerReportesAgrupadosPorCuadricula(dUltimaFechaEjecucion20000, 20000);

                if (oListReportes.Count > 0)
                {
                    bResult = await AddorEditListCoberturaReportAgrupadoCuadricula(oListReportes, "CoberturaReportAgrupadoCuadricula20000m");
                }
                else
                {
                    bResult = true;
                }

                if (bResult == true)
                {
                    string sIndexName = _esCoberturaHelper.GetIndexNameForType("CoberturaReportAgrupadoCuadricula20000m");

                    CoberturaCronUltimaEjecucion oCoberturaCronUltimaEjecucion = new CoberturaCronUltimaEjecucion();

                    oCoberturaCronUltimaEjecucion.reportid = Guid.NewGuid().ToString();
                    oCoberturaCronUltimaEjecucion.fecha_ultima_ejecucion = DateTimeOffset.Now;
                    oCoberturaCronUltimaEjecucion.nombre_index = sIndexName;

                    await AddUltimaEjecucionCron(oCoberturaCronUltimaEjecucion);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                Helper.VolcarAConsola("CoberturaReportRepository", "CoberturaReportAgrupadoCuadricula20000m", ex.Message, true);
            }*/

            return bResult;
        }

        public async Task<bool> AjustarTipoRed()
        {
            bool bResult = false;

            try
            {
                bResult = await _esCoberturaHelper.AjustarTipoRed();

                if (bResult == false) {
                    Helper.VolcarAConsola("CoberturaReportRepository", "AjustarTipoRed", "No ejecutado", true);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                Helper.VolcarAConsola("CoberturaReportRepository", "AjustarTipoRed", ex.Message, true);
            }

            return bResult;
        }

        public async Task<ReportesFiltradosResponse> ObtenerReportesFiltrados(DateTime? dFechaDesde, DateTime? dFechaHasta, string sMunicipio, string sCodigoINE) {
            ReportesFiltradosResponse oResponse = new ReportesFiltradosResponse();

            string sIndexMapping = "CoberturaReport";

            try
            {

                oResponse = _esCoberturaHelper.ObtenerReportesFiltrados(sIndexMapping, dFechaDesde, dFechaHasta, sMunicipio, sCodigoINE);

            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "ObtenerTodosReportes", ex.Message, true);
            }

            return oResponse;
        }

        public async Task<ReportesAccesibilidadResponse> ObtenerReportesPaginados(string sIndexMapping,int iSkip, int iTake, string sortBy, string sortOrder, List<string> municipios, List<string> categorias, List<string> calidades)
        {
            ReportesAccesibilidadResponse oResponse = new ReportesAccesibilidadResponse();

            try
            {

                oResponse = await _esCoberturaHelper.ObtenerReportesPaginado(sIndexMapping, iSkip, iTake, sortBy, sortOrder, municipios, categorias, calidades);

            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "ObtenerReportesPaginados", ex.Message, true);
            }

            return oResponse;
        }

    }
}
