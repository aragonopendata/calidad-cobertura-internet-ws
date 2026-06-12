package es.ideariumConsultores.opendata.cobertura;

import es.ideariumConsultores.opendata.cobertura.model.Medida;
import org.junit.Test;
import org.junit.experimental.categories.Category;
import org.junit.runner.RunWith;
import org.mockito.ArgumentCaptor;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.context.annotation.Import;
import org.springframework.http.MediaType;
import org.springframework.test.context.junit4.SpringRunner;
import org.springframework.test.web.servlet.MockMvc;

import java.util.Calendar;
import java.util.TimeZone;

import static org.junit.Assert.*;
import static org.mockito.Matchers.any;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.content;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

/**
 * Capa 2 — binding Jackson de Medida en /api/registrarDatosCobertura.
 * Fija cómo Boot 1.4.1 deserializa el JSON antes de pasarlo al servicio.
 * Ver guia-tests-capa2.md §5.
 */
@RunWith(SpringRunner.class)
@WebMvcTest(RestController.class)
@Import(StubDataSourceTestConfig.class)
@Category(ContratoConMocks.class)
public class RestControllerContratoMedidaJsonTest {

    @Autowired
    MockMvc mvc;

    @MockBean
    CoberturaService cobertura;

    @MockBean
    VisorService visor;

    @MockBean
    DataService dataService;

    // ─── § 5.1 — Caso completo: todos los campos conocidos se deserializan correctamente ───

    @Test
    public void registrarDatosCobertura_casoCompleto() throws Exception {
        when(cobertura.registrarDatosCobertura(any(Medida.class))).thenReturn("MOCK::registrado"); // MOCKED: el valor real lo fija la Capa 3

        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{" +
                    "\"timestamp\":\"2026-06-01 10:30:00Z\"," +
                    "\"coordenadax\":681250,\"coordenaday\":4612750," +
                    "\"municipio\":\"Zaragoza\",\"ine\":50297," +
                    "\"modelo\":\"Pixel 7\",\"so\":\"Android\",\"tipoRed\":\"LTE\"," +
                    "\"operador\":\"Movistar\",\"valorIntensidadSenial\":-95.0," +
                    "\"velocidadBajada\":50.5,\"velocidadSubida\":10.25," +
                    "\"latencia\":23.0,\"categoria\":\"RED MOVIL\"}"))
           .andExpect(status().isOk())
           .andExpect(content().string("MOCK::registrado"));

        ArgumentCaptor<Medida> captor = ArgumentCaptor.forClass(Medida.class);
        verify(cobertura).registrarDatosCobertura(captor.capture());
        Medida m = captor.getValue();

        assertEquals("municipio", "Zaragoza", m.getMunicipio());
        assertEquals("ine", Integer.valueOf(50297), m.getIne());
        assertEquals("so", "Android", m.getSo());
        assertEquals("modelo", "Pixel 7", m.getModelo());
        assertEquals("tipoRed", "LTE", m.getTipoRed());
        assertEquals("operador", "Movistar", m.getOperador());
        assertEquals("valorIntensidadSenial", Double.valueOf(-95.0), m.getValorIntensidadSenial());
        assertEquals("velocidadBajada", Double.valueOf(50.5), m.getVelocidadBajada());
        assertEquals("velocidadSubida", Double.valueOf(10.25), m.getVelocidadSubida());
        assertEquals("latencia", Double.valueOf(23.0), m.getLatencia());
        assertEquals("categoria", "RED MOVIL", m.getCategoria());
        assertEquals("coordenadax", Integer.valueOf(681250), m.getCoordenadax());
        assertEquals("coordenaday", Integer.valueOf(4612750), m.getCoordenaday());

        // § 5.2 — Campo fecha: propiedad JSON "timestamp" con @JsonFormat parseado en UTC
        // CHARACTERIZATION: @JsonFormat(pattern="yyyy-MM-dd HH:mm:ss'Z'") sin timezone → Jackson usa UTC
        // La 'Z' del pattern es literal (no timezone designator), la comparación es por getTimeInMillis()
        assertNotNull("timestamp no debe ser null", m.getTimestamp());
        Calendar esperado = Calendar.getInstance(TimeZone.getTimeZone("UTC"));
        esperado.clear();
        esperado.set(2026, Calendar.JUNE, 1, 10, 30, 0);
        assertEquals("timestamp en UTC por getTimeInMillis()", esperado.getTimeInMillis(), m.getTimestamp().getTimeInMillis());
    }

    // ─── § 5.2 — Campo "fecha" como propiedad JSON → ignorado (el campo Java se llama "timestamp") ───

    @Test
    public void registrarDatosCobertura_campoFecha_seIgnora() throws Exception {
        when(cobertura.registrarDatosCobertura(any(Medida.class))).thenReturn("MOCK::registrado"); // MOCKED

        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{\"fecha\":\"2026-06-01 10:30:00Z\",\"so\":\"Android\"}"))
           .andExpect(status().isOk());

        ArgumentCaptor<Medida> captor = ArgumentCaptor.forClass(Medida.class);
        verify(cobertura).registrarDatosCobertura(captor.capture());
        // "fecha" no tiene mapeo Jackson — se ignora, timestamp queda null
        assertNull("timestamp debe ser null cuando se envía 'fecha' en lugar de 'timestamp'",
                captor.getValue().getTimestamp());
        assertEquals("so sí se deserializa", "Android", captor.getValue().getSo());
    }

    // ─── § 5.3 — Propiedad desconocida → se ignora (FAIL_ON_UNKNOWN_PROPERTIES=false en Boot 1.4) ───

    @Test
    public void registrarDatosCobertura_propiedadDesconocida_seIgnora() throws Exception {
        // CHARACTERIZATION: Boot 1.4 configura Jackson con FAIL_ON_UNKNOWN_PROPERTIES=false
        // Si una subida de Boot cambia este default, este test lo detecta (devolvería 400)
        when(cobertura.registrarDatosCobertura(any(Medida.class))).thenReturn("MOCK::registrado"); // MOCKED

        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{\"so\":\"Android\",\"foo\":\"bar\"}"))
           .andExpect(status().isOk());

        ArgumentCaptor<Medida> captor = ArgumentCaptor.forClass(Medida.class);
        verify(cobertura).registrarDatosCobertura(captor.capture());
        assertEquals("so se deserializa correctamente", "Android", captor.getValue().getSo());
        assertNull("municipio no enviado queda null", captor.getValue().getMunicipio());
    }

    // ─── § 5.3 — Body {} vacío → 200, todos los campos null (id=0 por primitivo) ───

    @Test
    public void registrarDatosCobertura_bodyVacio_camposNulos() throws Exception {
        // CHARACTERIZATION: el controller no valida Medida — si se añade @Valid, este test lo detecta
        when(cobertura.registrarDatosCobertura(any(Medida.class))).thenReturn("MOCK::registrado"); // MOCKED

        mvc.perform(post("/api/registrarDatosCobertura")
                .contentType(MediaType.APPLICATION_JSON)
                .content("{}"))
           .andExpect(status().isOk());

        ArgumentCaptor<Medida> captor = ArgumentCaptor.forClass(Medida.class);
        verify(cobertura).registrarDatosCobertura(captor.capture());
        Medida m = captor.getValue();
        assertEquals("id es primitivo int, queda 0", 0, m.getId());
        assertNull("timestamp", m.getTimestamp());
        assertNull("coordenadax", m.getCoordenadax());
        assertNull("coordenaday", m.getCoordenaday());
        assertNull("municipio", m.getMunicipio());
        assertNull("ine", m.getIne());
        assertNull("modelo", m.getModelo());
        assertNull("so", m.getSo());
        assertNull("tipoRed", m.getTipoRed());
        assertNull("operador", m.getOperador());
        assertNull("valorIntensidadSenial", m.getValorIntensidadSenial());
        assertNull("velocidadBajada", m.getVelocidadBajada());
        assertNull("velocidadSubida", m.getVelocidadSubida());
        assertNull("latencia", m.getLatencia());
        assertNull("categoria", m.getCategoria());
    }
}
