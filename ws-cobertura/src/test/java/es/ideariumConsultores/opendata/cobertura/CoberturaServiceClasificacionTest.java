package es.ideariumConsultores.opendata.cobertura;

import es.ideariumConsultores.opendata.cobertura.model.Medida;
import org.junit.Test;

import static org.junit.Assert.assertEquals;

public class CoberturaServiceClasificacionTest {

    // ── normalizarTipoRed (vía calcularValores) ──────────────────────────────

    private String normalizar(String tipoRed) {
        Medida m = new Medida();
        m.setTipoRed(tipoRed);
        m.setCategoria(Medida.RED_MOVIL); // evita que se invoque obtenerCategoria
        new CoberturaService().calcularValores(m);
        return m.getTipoRed();
    }

    @Test
    public void normalizarTipoRedTablado() {
        String[][] casos = {
            // { entrada, esperado }
            // Grupo 2G
            { "2G",    "2G" }, { "GSM",   "2G" }, { "GPRS",  "2G" }, { "EDGE",  "2G" },
            // Grupo 3G
            { "3G",    "3G" }, { "CDMA",  "3G" }, { "UMTS",  "3G" }, { "HSPA",  "3G" },
            { "HSUPA", "3G" }, { "HSDPA", "3G" }, { "1XRTT", "3G" }, { "EHRPD", "3G" },
            // Grupo 4G
            { "4G",    "4G" }, { "LTE",   "4G" }, { "UMB",   "4G" }, { "HSPA+", "4G" },
            // Grupo 5G
            { "5G",    "5G" },
            // MOBILE / CELLULAR→MOBILE
            { "MOBILE",   "MOBILE" },
            { "CELLULAR", "MOBILE" },
            // Otros
            { "WIFI",  "WIFI"  },
            { "WIMAX", "WIMAX" },
            { "ETH",   "ETH"   },
        };

        for (String[] c : casos) {
            assertEquals("normalizar(\"" + c[0] + "\")", c[1], normalizar(c[0]));
        }
    }

    @Test
    public void normalizarTipoRedCaseInsensitive() {
        assertEquals("2G", normalizar("gprs"));
        assertEquals("4G", normalizar("lte"));
        assertEquals("4G", normalizar("HsPa+"));
    }

    @Test
    public void normalizarTipoRedHspaPlusVsHspa() {
        // HSPA→3G, HSPA+→4G: el símbolo + desvía a la rama 4G, no a la 3G
        assertEquals("3G", normalizar("HSPA"));
        assertEquals("4G", normalizar("HSPA+"));
    }

    @Test
    public void normalizarTipoRedPassthrough() {
        // Valor sin match → se devuelve tal cual (no hay fallback)
        assertEquals("FOO", normalizar("FOO"));
    }

    // ── obtenerCategoria (vía calcularValores) ────────────────────────────────

    private String categoria(String so, String modelo, String tipoRed) {
        Medida m = new Medida();
        m.setSo(so);
        m.setModelo(modelo);
        m.setTipoRed(tipoRed);
        m.setCategoria(null); // fuerza recálculo
        new CoberturaService().calcularValores(m);
        return m.getCategoria();
    }

    @Test
    public void obtenerCategoriaTablado() {
        Object[][] casos = {
            // { so, modelo, tipoRed, expectedCategoria }
            // Regla #1: WIFI gana sobre cualquier SO (incluido Android)
            { "Android",     "Pixel",     "WIFI", Medida.RED_CABLEADA },
            // Regla #2: Android → RED MOVIL
            { "Android",     "Pixel",     "LTE",  Medida.RED_MOVIL    },
            // Regla #3: iOS/iPhone
            { "iOS",         "iPhone",    "4G",   Medida.RED_MOVIL    },
            { "",            "iPhone",    "3G",   Medida.RED_MOVIL    }, // match por modelo
            // Regla #4: OSX/Mac → RED FIJA
            { "OSX",         "Mac",       "ETH",  Medida.RED_CABLEADA },
            // Regla #5: Linux → RED FIJA
            { "Linux",       "Linux",     "ETH",  Medida.RED_CABLEADA },
            // Regla #6: Windows/PC → RED FIJA
            { "Windows",     "PC",        "ETH",  Medida.RED_CABLEADA },
            // Regla #7: fallback por tipoRed normalizado
            { "SymbianRaro", "NokiaRaro", "GPRS",  Medida.RED_MOVIL    }, // GPRS→2G→móvil
            { "SymbianRaro", "NokiaRaro", "ETH",   Medida.RED_CABLEADA }, // ETH→fija
            { "SymbianRaro", "NokiaRaro", "WIMAX", Medida.RED_MOVIL    }, // WIMAX→default→móvil
        };

        for (Object[] c : casos) {
            String so      = (String) c[0];
            String modelo  = (String) c[1];
            String tipoRed = (String) c[2];
            String expected = (String) c[3];
            assertEquals(
                String.format("categoria(so=%s, modelo=%s, tipoRed=%s)", so, modelo, tipoRed),
                expected,
                categoria(so, modelo, tipoRed));
        }
    }

    @Test
    public void categoriaOverrideValidoConservado() {
        // Si categoria ya es "RED FIJA", no se recalcula aunque so/tipoRed sugieran RED MOVIL
        Medida m = new Medida();
        m.setSo("Android");
        m.setModelo("Pixel");
        m.setTipoRed("LTE");
        m.setCategoria(Medida.RED_CABLEADA);
        new CoberturaService().calcularValores(m);
        assertEquals(Medida.RED_CABLEADA, m.getCategoria());
    }

    @Test
    public void categoriaSinValidezSeRecalcula() {
        // Si categoria es un valor no reconocido ("foo"), se recalcula
        Medida m = new Medida();
        m.setSo("Android");
        m.setModelo("Pixel");
        m.setTipoRed("LTE");
        m.setCategoria("foo");
        new CoberturaService().calcularValores(m);
        assertEquals(Medida.RED_MOVIL, m.getCategoria());
    }
}
