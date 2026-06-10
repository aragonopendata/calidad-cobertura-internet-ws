package es.ideariumConsultores.opendata.cobertura;

import javax.sql.DataSource;
import org.slf4j.LoggerFactory;
import org.springframework.boot.ApplicationRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.retry.annotation.EnableRetry;
import org.springframework.scheduling.annotation.EnableAsync;

@EnableAsync
@EnableRetry
@SpringBootApplication
public class CoberturaApplication {

	public static void main(String[] args) {
		SpringApplication.run(CoberturaApplication.class, args);
	}

	@Bean
	public ApplicationRunner checkDatabaseConnectivity(DataSource dataSource) {
		return args -> {
			try {
				dataSource.getConnection().close();
			} catch (Exception e) {
				LoggerFactory.getLogger("cobertura").error(
					"Cannot connect to database — check DB_URL, DB_USER, DB_PASSWORD. Exiting. Cause: {}",
					e.getMessage());
				System.exit(1);
			}
		};
	}

}
