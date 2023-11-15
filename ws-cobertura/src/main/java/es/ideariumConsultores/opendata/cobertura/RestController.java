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
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;

import es.ideariumConsultores.opendata.cobertura.model.Medida;

@CrossOrigin(origins = {"http://localhost:8000","http://localhost:4200","http://localhost","app://localhost", "https://opendataei2a.aragon.es", "https://desopendataei2a.aragon.es",
		 "https://preopendataei2a.aragon.es","https://opendata.aragon.es", "https://desopendata.aragon.es", "https://preopendata.aragon.es"})
@org.springframework.web.bind.annotation.RestController
public class RestController {

    @Value("${logging.path}")
    String logpath;

    
    @Autowired
    CoberturaService cobertura;
    
    @Autowired
   VisorService visor;
    
    @Autowired
   DataService dataService;
    
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
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
    	}
    	
    }
    @RequestMapping(value={"/api/registrarDatosCobertura"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "text/plain")
    public ResponseEntity registrarDatosCobertura(@RequestBody Medida medida ){
    	try{
    		
    		
    		
    		return ResponseEntity.status(OK).body(cobertura.registrarDatosCobertura(medida));
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
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
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
    	}
    	
    }
    
    @RequestMapping(value={"/api/getData/{capa}"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "application/json")
    public ResponseEntity getData(@PathVariable(required = false) String capa, @RequestParam(value = "municipio",required = false) Integer municipio , @RequestParam(value = "anyo",required = false) Integer anyo ){
    	try{
    		
    		return ResponseEntity.status(OK).body( dataService.getData(capa, municipio,anyo));
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
    	}
    	
    }
    
    @RequestMapping(value={"/data"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "application/json")
    public ResponseEntity getData( @RequestParam(value = "municipio",required = false) Integer municipio){
    	try{
    		
    		return ResponseEntity.status(OK).body( dataService.getSummary( municipio));
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
    	}
    	
    }
    
    @RequestMapping(value={"/config/toc"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "application/json")
    public ResponseEntity getToc( ){
    	try{
    		
    		return ResponseEntity.status(OK).body( visor.getToc());
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
    	}
    	
    }
    
   
    
    @RequestMapping(value={"/config/queryableLayers"}, method = { RequestMethod.GET, RequestMethod.POST }, produces = "application/json")
    public ResponseEntity getQuerayableLayers( ){
    	try{
    		
    		return ResponseEntity.status(OK).body( visor.getQueryableLayers());
    	}
    	catch(Exception ex){
    		log.info("error", ex);
    		return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(ex.getMessage());
    	}
    	
    }
 }
