using Messaging.ElasticSearch;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<bool> AddorEditListCoberturaReportAgrupadoCuadriculaTecnologia(List<CoberturaReportAgrupadoCuadriculaTecnologia> oListReports)
        {
            Nest.UpdateResponse<CoberturaReportAgrupadoCuadriculaTecnologia> response = null;
            try
            {
                response = await _esCoberturaHelper.AddListCoberturaReportAgrupadoCuadriculaTecnologiaToIndex(oListReports);
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "AddorEditListCoberturaReportAgrupadoCuadriculaTecnologia", ex.Message, true);
            }

            return response != null && response.IsValid;
        }

        public async Task<bool> DeleteIndexCoberturaReportAgrupadoCuadriculaTecnologia()
        {
            Nest.DeleteIndexResponse response = null;
            try
            {
                response = await _esCoberturaHelper.DeleteIndexCoberturaReportAgrupadoCuadriculaTecnologia();
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("CoberturaReportRepository", "DeleteIndexCoberturaReportAgrupadoCuadriculaTecnologia", ex.Message, true);
            }

            return response != null && response.IsValid;
        }

        public async Task<bool> GroupCoberturaReports()
        {
            bool bResult = false;
            try
            {
                
                DateTimeOffset? dUltimaFechaEjecucion = await _esCoberturaHelper.ObtenerUltimaEjecucionCorrectaCronPorIndexMapping("CoberturaReportAgrupadoCuadriculaTecnologia");

                List<CoberturaReportAgrupadoCuadriculaTecnologia> oListReportes = await _esCoberturaHelper.ObtenerReportesAgrupadosPorCuadriculaYTecnologia(dUltimaFechaEjecucion);

                if (oListReportes.Count > 0)
                {
                    bResult = await AddorEditListCoberturaReportAgrupadoCuadriculaTecnologia(oListReportes);
                }
                else {
                    bResult = true;
                }

                if (bResult == true)
                {
                    string sIndexName = _esCoberturaHelper.GetIndexNameForType("CoberturaReportAgrupadoCuadriculaTecnologia");

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
                Helper.VolcarAConsola("CoberturaReportRepository", "GroupCoberturaReports", ex.Message, true);
            }
          

            return bResult;
        }
    }
}
