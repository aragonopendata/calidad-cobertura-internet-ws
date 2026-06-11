package es.ideariumConsultores.opendata.cobertura;

import es.ideariumConsultores.opendata.cobertura.model.Medida;
import org.junit.Test;

import static org.junit.Assert.*;

public class CoberturaServiceCalcularValoresTest {

    // ── NPE characterization (§6) ─────────────────────────────────────────────

    @Test(expected = NullPointerException.class)
    public void tipoRedNullLanzaNpe() {
        // CHARACTERIZATION: comportamiento actual, posible bug — ver guía §6
        // normalizarTipoRed llama a null.equalsIgnoreCase() al ser tipoRed null
        Medida m = new Medida();
        m.setTipoRed(null);
        m.setCategoria(Medida.RED_MOVIL);
        new CoberturaService().calcularValores(m);
    }

    @Test(expected = NullPointerException.class)
    public void soNullConTipoRedNoWifiLanzaNpe() {
        // CHARACTERIZATION: comportamiento actual, posible bug — ver guía §6
        // obtenerCategoria: sSO queda null y null.equalsIgnoreCase("android") explota
        Medida m = new Medida();
        m.setTipoRed("3G");
        m.setSo(null);
        m.setModelo("SomeModelo");
        m.setCategoria(null);
        new CoberturaService().calcularValores(m);
    }

    @Test(expected = NullPointerException.class)
    public void modeloNullConSoNoAndroidNiIosLanzaNpe() {
        // CHARACTERIZATION: comportamiento actual, posible bug — ver guía §6
        // obtenerCategoria: la rama "|| sModelo.equalsIgnoreCase(iPhone)" evalúa null
        // cuando so no es iOS y cortocircuita antes de su rama
        Medida m = new Medida();
        m.setTipoRed("ETH");
        m.setSo("Windows"); // pasa los checks Android/iOS-SO, llega al || con modelo null
        m.setModelo(null);
        m.setCategoria(null);
        new CoberturaService().calcularValores(m);
    }

    // ── Sanitización de ceros (§6) ────────────────────────────────────────────

    @Test
    public void intensidadCeroSeConvierteANull() {
        Medida m = medidaWifi();
        m.setValorIntensidadSenial(0.0);
        new CoberturaService().calcularValores(m);
        assertNull(m.getValorIntensidadSenial());
    }

    @Test
    public void intensidadNoNulaSeConserva() {
        Medida m = medidaWifi();
        m.setValorIntensidadSenial(-95.0);
        new CoberturaService().calcularValores(m);
        assertEquals(Double.valueOf(-95.0), m.getValorIntensidadSenial());
    }

    @Test
    public void intensidadNullSeConservaNull() {
        Medida m = medidaWifi();
        m.setValorIntensidadSenial(null);
        new CoberturaService().calcularValores(m);
        assertNull(m.getValorIntensidadSenial());
    }

    @Test
    public void latenciaCeroSeConvierteANull() {
        Medida m = medidaWifi();
        m.setLatencia(0.0);
        new CoberturaService().calcularValores(m);
        assertNull(m.getLatencia());
    }

    @Test
    public void latenciaNoNulaSeConserva() {
        Medida m = medidaWifi();
        m.setLatencia(25.0);
        new CoberturaService().calcularValores(m);
        assertEquals(Double.valueOf(25.0), m.getLatencia());
    }

    @Test
    public void latenciaNullSeConservaNull() {
        Medida m = medidaWifi();
        m.setLatencia(null);
        new CoberturaService().calcularValores(m);
        assertNull(m.getLatencia());
    }

    // ── Test integrador end-to-end (§7) ──────────────────────────────────────

    @Test
    public void calcularValoresEndToEnd() {
        Medida m = new Medida();
        m.setSo("Android");
        m.setModelo("Pixel 7");
        m.setTipoRed("LTE");           // → normaliza a 4G
        m.setCategoria(null);           // → recalcula → RED MOVIL
        m.setValorIntensidadSenial(0.0); // → null
        m.setLatencia(0.0);             // → null
        m.setVelocidadBajada(50.0);     // no lo toca calcularValores

        Medida r = new CoberturaService().calcularValores(m);

        assertEquals("4G", r.getTipoRed());
        assertEquals(Medida.RED_MOVIL, r.getCategoria());
        assertNull(r.getValorIntensidadSenial());
        assertNull(r.getLatencia());
        assertEquals(Double.valueOf(50.0), r.getVelocidadBajada());
        assertSame(m, r); // muta y devuelve la misma instancia
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    /** Medida con tipoRed=WIFI y categoria ya válida, para testear sólo sanitización. */
    private Medida medidaWifi() {
        Medida m = new Medida();
        m.setTipoRed("WIFI");
        m.setCategoria(Medida.RED_CABLEADA);
        return m;
    }
}
