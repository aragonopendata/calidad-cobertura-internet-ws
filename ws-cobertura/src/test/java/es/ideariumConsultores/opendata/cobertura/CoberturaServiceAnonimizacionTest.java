package es.ideariumConsultores.opendata.cobertura;

import org.junit.Test;

import static org.junit.Assert.assertArrayEquals;

public class CoberturaServiceAnonimizacionTest {

    private final CoberturaService service = new CoberturaService();

    @Test
    public void anonimizarCasosTablados() {
        Object[][] casos = {
            // { x, y, multiplo, suma, expX, expY }
            // Los tres múltiplos reales usados en obtenerMunicipioPorCoordenadas:
            { 681234.0, 4612678.0,   500,   250,  681250, 4612750 },
            { 681234.0, 4612678.0,  5000,  2500,  682500, 4612500 },
            { 681234.0, 4612678.0, 20000, 10000,  690000, 4610000 },
            // Coordenadas en múltiplo exacto (el +suma siempre se añade)
            { 500000.0, 4600000.0,   500,   250,  500250, 4600250 },
            // Coordenadas cero
            { 0.0,      0.0,         500,   250,     250,     250 },
        };

        for (Object[] c : casos) {
            double x       = (Double)  c[0];
            double y       = (Double)  c[1];
            int multiplo   = (Integer) c[2];
            int suma       = (Integer) c[3];
            int[] expected = { (Integer) c[4], (Integer) c[5] };

            int[] actual = service.anonimizarCoordenadasUTM(x, y, multiplo, suma);
            assertArrayEquals(
                String.format("anonimizar(%.0f, %.0f, %d, %d)", x, y, multiplo, suma),
                expected, actual);
        }
    }
}
