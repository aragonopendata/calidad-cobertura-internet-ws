package es.ideariumConsultores.opendata.cobertura;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.web.support.SpringBootServletInitializer;
import org.springframework.retry.annotation.EnableRetry;

@EnableRetry
@SpringBootApplication
public class CoberturaApplication extends SpringBootServletInitializer{


	public static void main(String[] args) {
		SpringApplication.run(CoberturaApplication.class, args);
	}

}
