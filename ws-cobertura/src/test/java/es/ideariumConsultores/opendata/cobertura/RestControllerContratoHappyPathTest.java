package es.ideariumConsultores.opendata.cobertura;

import es.ideariumConsultores.opendata.cobertura.model.Medida;
import org.junit.Test;
import org.junit.experimental.categories.Category;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.context.annotation.Import;
import org.springframework.http.MediaType;
import org.springframework.test.context.junit4.SpringRunner;
import org.springframework.test.web.servlet.MockMvc;

import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

/**
 * Capa 2 — happy path: fija status 200, pass-through exacto y Content-Type de los 9 endpoints.
 * Todos los servicios están mockeados — los stubs son centinelas, no respuestas reales.
 * Ver guia-tests-capa2.md §4 y §8.
 */
@RunWith(SpringRunner.class)
@WebMvcTest(RestController.class)
@Import(StubDataSourceTestConfig.class)
@Category(ContratoConMocks.class)
public class RestControllerContratoHappyPathTest {

    @Autowired
    MockMvc mvc;

    @MockBean
    CoberturaService cobertura;

    @MockBean
    VisorService visor;

    @MockBean
    DataService dataService;

    // ─── Endpoint 1: /api/obtenerMunicipioPorCoordenadas ───────────────────────

    @Test
    public void obtenerMunicipioPorCoordenadas_post_happyPath() throws Exception {
        when(cobertura.obtenerMunicipioPorCoordenadas(anyString())).thenReturn("MOCK::municipio"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(post("/api/obtenerMunicipioPorCoordenadas")
                .content("{\"latitud\":41.65,\"longitud\":-0.88}"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::municipio"))
           .andExpect(header().string("Content-Type", "text/plain;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(cobertura).obtenerMunicipioPorCoordenadas("{\"latitud\":41.65,\"longitud\":-0.88}");
    }

    // ─── Endpoint 2: /api/obtenerDatosPorCoordenadas ───────────────────────────

    @Test
    public void obtenerDatosPorCoordenadas_post_happyPath() throws Exception {
        when(cobertura.obtenerDatosPorCoordenadas(anyString())).thenReturn("MOCK::datos"); // MOCKED: el valor real lo fija la Capa 3
        String body = "{\"latitud\":41.65,\"longitud\":-0.88,\"sSO\":\"Android\",\"sModelo\":\"Pixel\",\"sTipoRed\":\"LTE\"}";
        mvc.perform(post("/api/obtenerDatosPorCoordenadas")
                .content(body))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::datos"))
           .andExpect(header().string("Content-Type", "text/plain;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(cobertura).obtenerDatosPorCoordenadas(body);
    }

    // ─── Endpoint 3: /api/obtenerCalidadCobertura ──────────────────────────────

    @Test
    public void obtenerCalidadCobertura_happyPath() throws Exception {
        when(cobertura.obtenerCalidadCobertura(anyString(), any(Double.class))).thenReturn("MOCK::calidad"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(get("/api/obtenerCalidadCobertura")
                .param("categoria", "RED MOVIL")
                .param("velBajada", "12.5"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::calidad"))
           .andExpect(header().string("Content-Type", "text/plain;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(cobertura).obtenerCalidadCobertura("RED MOVIL", 12.5);
    }

    // ─── Endpoint 4: /api/registrarDatosCobertura (happy path básico; binding completo en RestControllerContratoMedidaJsonTest) ───

    @Test
    public void registrarDatosCobertura_post_happyPath() throws Exception {
        when(cobertura.registrarDatosCobertura(any(Medida.class))).thenReturn("MOCK::registrado"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{\"so\":\"Android\",\"municipio\":\"Zaragoza\"}"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::registrado"))
           .andExpect(header().string("Content-Type", "text/plain;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
    }

    // ─── Endpoint 5: /api/testVelocidadSubida (sin servicio — devuelve literal hardcoded) ───

    @Test
    public void testVelocidadSubida_post_happyPath() throws Exception {
        mvc.perform(post("/api/testVelocidadSubida")
                .content("dummy-upload-data"))
           .andExpect(status().isOk())
           .andExpect(content().string("{\"estadoRespuesta\":1}"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
    }

    // ─── Endpoint 6: /api/getData/{capa} — con parámetros opcionales ───────────

    @Test
    public void getData_conParams_happyPath() throws Exception {
        when(dataService.getData("velocidad", 50297, 2024)).thenReturn("MOCK::getData-params"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(get("/api/getData/velocidad")
                .param("municipio", "50297")
                .param("anyo", "2024"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::getData-params"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(dataService).getData("velocidad", 50297, 2024);
    }

    // ─── Endpoint 6: /api/getData/{capa} — sin parámetros opcionales (llegan null) ───

    @Test
    public void getData_sinParams_happyPath() throws Exception {
        when(dataService.getData("velocidad", null, null)).thenReturn("MOCK::getData-noparams"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(get("/api/getData/velocidad"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::getData-noparams"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(dataService).getData("velocidad", null, null);
    }

    // ─── Endpoint 7: /data — con parámetro optional ────────────────────────────

    @Test
    public void getSummary_conParam_happyPath() throws Exception {
        when(dataService.getSummary(50297)).thenReturn("MOCK::summary-param"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(get("/data").param("municipio", "50297"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::summary-param"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(dataService).getSummary(50297);
    }

    // ─── Endpoint 7: /data — sin parámetro (llega null) ───────────────────────

    @Test
    public void getSummary_sinParam_happyPath() throws Exception {
        when(dataService.getSummary(null)).thenReturn("MOCK::summary-noparam"); // MOCKED: el valor real lo fija la Capa 3
        mvc.perform(get("/data"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::summary-noparam"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(dataService).getSummary(null);
    }

    // ─── Endpoint 8: /config/toc ───────────────────────────────────────────────

    @Test
    public void getToc_happyPath() throws Exception {
        when(visor.getToc()).thenReturn("MOCK::toc"); // MOCKED: el JSON real lo fija la Capa 3
        mvc.perform(get("/config/toc"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::toc"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(visor).getToc();
    }

    // ─── Endpoint 9: /config/queryableLayers ───────────────────────────────────

    @Test
    public void getQueryableLayers_happyPath() throws Exception {
        when(visor.getQueryableLayers()).thenReturn("MOCK::queryableLayers"); // MOCKED: el JSON real lo fija la Capa 3
        mvc.perform(get("/config/queryableLayers"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::queryableLayers"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: charset observado en Boot 1.4.1
        verify(visor).getQueryableLayers();
    }

    // ─── § 8: CORS — preflight con origen permitido ───────────────────────────

    @Test
    public void cors_preflightOrigenPermitido() throws Exception {
        // cors.origins default incluye http://localhost:4200 (ver application.properties:33)
        mvc.perform(options("/config/toc")
                .header("Origin", "http://localhost:4200")
                .header("Access-Control-Request-Method", "GET"))
           .andExpect(status().isOk())
           .andExpect(header().string("Access-Control-Allow-Origin", "http://localhost:4200"));
    }

    // ─── § 8: CORS — origen no permitido → 403 ────────────────────────────────

    @Test
    public void cors_origenNoPermitido() throws Exception {
        mvc.perform(get("/config/toc")
                .header("Origin", "http://evil.example"))
           .andExpect(status().isForbidden())
           .andExpect(content().string("Invalid CORS request")); // CHARACTERIZATION: comportamiento actual en Spring 4 — ver guía §8
    }
}
