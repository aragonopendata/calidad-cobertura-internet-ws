package es.ideariumConsultores.opendata.cobertura;

import static org.springframework.http.HttpStatus.OK;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.CrossOrigin;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;

import es.ideariumConsultores.opendata.cobertura.model.Medida;

@CrossOrigin(origins = {"http://localhost:8000","http://localhost", "https://opendataei2a.aragon.es/", "https://desopendataei2a.aragon.es/"})
@org.springframework.web.bind.annotation.RestController
public class RestController {

    @Value("${logging.path}")
    String logpath;

    
    @Autowired
    CoberturaService cobertura;
    
    public final static Logger log = LoggerFactory.getLogger("cobertura");
    
    @RequestMapping(value = "/logs", method = RequestMethod.GET, produces = "application/json")
    public ResponseEntity logs() throws Exception {
    	try{
    		byte[] encoded = Files.readAllBytes(Paths.get(logpath+"spring.log"));
    		String s = new String(encoded, StandardCharsets.US_ASCII);
    		return ResponseEntity.status(OK).body(s);
    		}
    		catch(Exception ex){
    			log.error("Error obteniendo log",ex);
    			return ResponseEntity.status(org.springframework.http.HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getLocalizedMessage());
    		}
    }

   
 
    
    @RequestMapping(value={"/api/obtenerMunicipioPorCoordenadas"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "text/plain")
    public ResponseEntity obtenerMunicipioPorCoordenadas(@RequestBody String latlon){
    	try{
    		
    		return ResponseEntity.status(OK).body(cobertura.obtenerMunicipioPorCoordenadas(latlon));
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Error");
    	}
    	
    }
    @RequestMapping(value={"/api/registrarDatosCobertura"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "text/plain")
    public ResponseEntity registrarDatosCobertura(@RequestBody Medida medida ){
    	try{
    		
    		
    		
    		return ResponseEntity.status(OK).body(cobertura.registrarDatosCobertura(medida));
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Error");
    	}
    	
    }
    @RequestMapping(value={"/api/testVelocidadSubida"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "application/json")
    public ResponseEntity testVelocidadSubida(@RequestBody String file ){
    	try{
    		log.debug("filelength " +file.length());
    		return ResponseEntity.status(OK).body( "{\"estadoRespuesta\":1}");
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Error");
    	}
    	
    }
 }
