package es.ideariumConsultores.opendata.cobertura;

import java.sql.Connection;
import java.sql.SQLException;
import javax.sql.DataSource;
import org.mockito.Mockito;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.context.annotation.Bean;

/**
 * Provee un DataSource stub pre-stubeado para satisfacer la dependencia de
 * ApplicationRunner checkDatabaseConnectivity en CoberturaApplication.
 * Sin esto, @WebMvcTest dispararía System.exit(1) durante el arranque del contexto.
 * Ver guia-tests-capa2.md §2.3.
 */
@TestConfiguration
public class StubDataSourceTestConfig {

    // Satisface la dependencia del ApplicationRunner checkDatabaseConnectivity
    // y hace que su getConnection().close() sea inocuo (evita System.exit(1)).
    @Bean
    public DataSource dataSource() throws SQLException {
        DataSource ds = Mockito.mock(DataSource.class);
        Mockito.when(ds.getConnection()).thenReturn(Mockito.mock(Connection.class));
        return ds;
    }
}
