package es.ideariumConsultores.opendata.cobertura;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.Statement;

import javax.sql.DataSource;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.retry.annotation.Backoff;
import org.springframework.retry.annotation.Retryable;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import es.ideariumConsultores.opendata.cobertura.model.Medida;
import es.ideariumConsultores.opendata.cobertura.repository.MedidaRepository;



@Service
public class RetryService {

	public final static Logger log = LoggerFactory.getLogger("cobertura");
	@Autowired
	MedidaRepository medidaRepository;

	  @Retryable( maxAttempts = 5, backoff = @Backoff(delay =1000))   
	  public void saveMedida(Medida medida) throws Exception{
		  log.debug("intento save");
		  try{
		  medidaRepository.save(medida);
		  }
		  catch(Exception ex){
			  log.warn("No se ha podido guardar la medida", ex);
			  throw ex;
		  }
	  }
	
}
