package es.ideariumConsultores.opendata.cobertura;

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

import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

/**
 * Capa 2 — sobre de error 500 y errores generados por Spring MVC (400/405).
 * Lo que más cambia entre versiones mayores de Spring Boot.
 * Ver guia-tests-capa2.md §6 y §7.
 */
@RunWith(SpringRunner.class)
@WebMvcTest(RestController.class)
@Import(StubDataSourceTestConfig.class)
@Category(ContratoConMocks.class)
public class RestControllerContratoErroresTest {

    @Autowired
    MockMvc mvc;

    @MockBean
    CoberturaService cobertura;

    @MockBean
    VisorService visor;

    @MockBean
    DataService dataService;

    // ─── § 6: Sobre de error — excepción con mensaje ────────────────────────────
    // El body del 500 es exactamente ex.getMessage(); los clientes pueden estar parseándolo.

    @Test
    public void getToc_excepcionConMensaje_devuelve500() throws Exception {
        when(visor.getToc()).thenThrow(new Exception("boom")); // MOCKED
        mvc.perform(get("/config/toc"))
           .andExpect(status().isInternalServerError())
           .andExpect(content().string("boom"))
           .andExpect(header().string("Content-Type", "application/json;charset=UTF-8")); // CHARACTERIZATION: produces sigue aplicando en el 500
    }

    // ─── § 6: Sobre de error — excepción sin mensaje (getMessage() == null) ─────

    @Test
    public void getToc_excepcionSinMensaje_devuelve500BodyVacio() throws Exception {
        // CHARACTERIZATION: ex.getMessage() == null → body(null) → body vacío en la respuesta
        when(visor.getToc()).thenThrow(new Exception()); // MOCKED
        mvc.perform(get("/config/toc"))
           .andExpect(status().isInternalServerError())
           .andExpect(content().string("")); // CHARACTERIZATION: null getMessage() → body vacío
    }

    // ─── § 6: Sobre de error en endpoint text/plain ──────────────────────────────

    @Test
    public void obtenerMunicipioPorCoordenadas_excepcion_devuelve500TextPlain() throws Exception {
        when(cobertura.obtenerMunicipioPorCoordenadas(anyString())).thenThrow(new Exception("fallo de geo")); // MOCKED
        mvc.perform(post("/api/obtenerMunicipioPorCoordenadas")
                .content("{}"))
           .andExpect(status().isInternalServerError())
           .andExpect(content().string("fallo de geo"))
           .andExpect(header().string("Content-Type", "text/plain;charset=UTF-8")); // CHARACTERIZATION: text/plain sigue aplicando en el 500
    }

    // ─── § 7: Errores de Spring MVC — parámetro obligatorio ausente ─────────────

    @Test
    public void obtenerCalidadCobertura_sinVelBajada_devuelve400() throws Exception {
        mvc.perform(get("/api/obtenerCalidadCobertura").param("categoria", "RED MOVIL"))
           .andExpect(status().isBadRequest()); // MissingServletRequestParameterException
        verifyZeroInteractions(cobertura, visor, dataService);
    }

    // ─── § 7: Errores de Spring MVC — parámetro con tipo inválido ───────────────

    @Test
    public void obtenerCalidadCobertura_velBajadaInvalida_devuelve400() throws Exception {
        mvc.perform(get("/api/obtenerCalidadCobertura")
                .param("categoria", "RED MOVIL")
                .param("velBajada", "abc"))
           .andExpect(status().isBadRequest()); // MethodArgumentTypeMismatchException
        verifyZeroInteractions(cobertura, visor, dataService);
    }

    // ─── § 7: Errores de Spring MVC — parámetro opcional con tipo inválido ──────

    @Test
    public void getSummary_municipioInvalido_devuelve400() throws Exception {
        // CHARACTERIZATION: opcional no significa laxo — tipo inválido sigue siendo 400
        mvc.perform(get("/data").param("municipio", "abc"))
           .andExpect(status().isBadRequest());
        verifyZeroInteractions(cobertura, visor, dataService);
    }

    // ─── § 7: Errores de Spring MVC — JSON malformado ───────────────────────────

    @Test
    public void registrarDatosCobertura_jsonMalformado_devuelve400() throws Exception {
        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{\"so\":"))
           .andExpect(status().isBadRequest()); // HttpMessageNotReadableException
        verifyZeroInteractions(cobertura, visor, dataService);
    }

    // ─── § 7: Errores de Spring MVC — campo con tipo inválido en JSON ───────────

    @Test
    public void registrarDatosCobertura_campoTipoInvalido_devuelve400() throws Exception {
        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{\"ine\":\"no-es-numero\"}"))
           .andExpect(status().isBadRequest()); // InvalidFormatException dentro de HttpMessageNotReadable
        verifyZeroInteractions(cobertura, visor, dataService);
    }

    // ─── § 7: Rareza GET+@RequestBody — GET sin body → 400 ──────────────────────

    @Test
    public void obtenerMunicipioPorCoordenadas_get_sinBody_devuelve400() throws Exception {
        // CHARACTERIZATION: @RequestBody obligatorio en GET sin body = 400 — rareza documentada del proyecto
        // Ver guia-tests-capa2.md §7
        mvc.perform(get("/api/obtenerMunicipioPorCoordenadas"))
           .andExpect(status().isBadRequest());
        verifyZeroInteractions(cobertura, visor, dataService);
    }

    // ─── § 7: Rareza GET+@RequestBody — GET con body → 200 ──────────────────────

    @Test
    public void obtenerMunicipioPorCoordenadas_get_conBody_devuelve200() throws Exception {
        // CHARACTERIZATION: MockMvc no rechaza body en GET — funciona igual que POST
        // Ver guia-tests-capa2.md §7
        when(cobertura.obtenerMunicipioPorCoordenadas(anyString())).thenReturn("MOCK::municipio"); // MOCKED
        mvc.perform(get("/api/obtenerMunicipioPorCoordenadas")
                .content("{\"latitud\":41.65,\"longitud\":-0.88}"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::municipio"));
    }

    // ─── § 7: Método no soportado → 405 ─────────────────────────────────────────

    @Test
    public void getToc_delete_devuelve405() throws Exception {
        mvc.perform(delete("/config/toc"))
           .andExpect(status().isMethodNotAllowed());
        verifyZeroInteractions(cobertura, visor, dataService);
    }
}
