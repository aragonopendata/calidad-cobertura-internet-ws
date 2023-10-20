package es.ideariumConsultores.opendata.cobertura;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.Calendar;

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
public class CoberturaService {

	public final static Logger log = LoggerFactory.getLogger("cobertura");
	@Autowired
	DataSource dataSource;

	@Autowired
	MedidaRepository medidaRepository;
	
	@Autowired
	private DataSourceConfiguration data;
	
	@Autowired
	RetryService retryService;
	
	public String obtenerMunicipioPorCoordenadas(double latitud,double longitud) throws Exception{
		Connection con = data.getConnection("geopub");
		try{
			Statement stmt =con.createStatement();
			log.debug("select c_muni_ine,d_muni_ine,provincia, st_x(punto) as x,st_y(punto) as y from (select st_transform(st_setsrid(st_point("+longitud+","+latitud+"),4326),25830) as punto) p, geopub.v101e_municipios where st_intersects(punto,shape)");
			ResultSet rs = stmt.executeQuery("select c_muni_ine,d_muni_ine,provincia, st_x(punto) as x,st_y(punto) as y from (select st_transform(st_setsrid(st_point("+longitud+","+latitud+"),4326),25830) as punto) p, geopub.v101e_municipios where st_intersects(punto,shape)");
			if (rs.next()){
				double coordenadaXUTM=rs.getDouble("x");
				double coordenadaYUTM=rs.getDouble("y");
				int[] coords500=anonimizarCoordenadasUTM(coordenadaXUTM,coordenadaYUTM, 500, 250);
				int[] coords5000=anonimizarCoordenadasUTM(coordenadaXUTM,coordenadaYUTM,  5000, 2500);
				int[] coords20000=anonimizarCoordenadasUTM(coordenadaXUTM,coordenadaYUTM, 20000, 10000);
				
				int	ine = rs.getInt("c_muni_ine");
				String	municipio = rs.getString("d_muni_ine");
				String	provincia = rs.getString("provincia");
					return "{\"ineMunicipio\":"+ine+",\"nombreMunicipio\":\""+municipio+"\",\"provincia\":\""+provincia+"\",\"coordenadax\":"+coords500[0]+",\"coordenaday\":"+coords500[1]+",\"coordenadax5000\":"+coords5000[0]+",\"coordenaday5000\":"+coords5000[1]+",\"coordenadax20000\":"+coords20000[0]+",\"coordenaday20000\":"+coords20000[1]+
							",\"estadoRespuesta\":1}";
							}
			else{
				return "{\"estadoRespuesta\":0,\"mensajeRespuesta\":\"No se encontraron coincidencias\"}";
			}
		}

		finally{
			con.close();
		
		}

	}
	
	public String obtenerMunicipioPorCoordenadas(String latlon) throws Exception{
		log.debug(latlon);
		JsonObject coords = new JsonParser().parse(latlon).getAsJsonObject();
		return obtenerMunicipioPorCoordenadas(coords.get("latitud").getAsDouble(),coords.get("longitud").getAsDouble());


	}
    public int[] anonimizarCoordenadasUTM(double coordenadaXUTM, double coordenadaYUTM, int iMultiploRedondeo, int iSumaRedondeo) {
                int iCoordenadaXUTMAnonimizada = ((int)(coordenadaXUTM / iMultiploRedondeo)) * iMultiploRedondeo;
        int iCoordenadaYUTMAnonimizada = ((int)(coordenadaYUTM / iMultiploRedondeo)) * iMultiploRedondeo;

        
        iCoordenadaXUTMAnonimizada = iCoordenadaXUTMAnonimizada + iSumaRedondeo;
        iCoordenadaYUTMAnonimizada = iCoordenadaYUTMAnonimizada + iSumaRedondeo;

        
        return new int[]{iCoordenadaXUTMAnonimizada,iCoordenadaYUTMAnonimizada};
    }
    
	protected Medida calcularValores(Medida medida){
		if ((medida.getCategoria()==null) || (medida.getCategoria().trim().length()==0)  || (medida.getCategoria() != Medida.RED_CABLEADA && medida.getCategoria() != Medida.RED_MOVIL))
        {
			medida.setCategoria(obtenerCategoria(medida.getSo(),medida.getModelo(),medida.getTipoRed()));
			
        }
		if ( (medida.getValorIntensidadSenial()!=null)&&  (medida.getValorIntensidadSenial() == 0) ){
            medida.setValorIntensidadSenial( null);
        }  
		 if ( (medida.getLatencia()!=null)&&(medida.getLatencia() == 0))
         {
             medida.setLatencia(null);
         }
	      
	    
	   
		
         return medida;
	}
	


	  private String obtenerCategoria(String sSO, String sModelo, String sTipoRed)
    {
        String sCategoria = Medida.RED_MOVIL;

        if ((sSO != null)&&(sSO.trim().length()>0)) {
            sSO = sSO.toLowerCase().trim();
        }

        if ((sModelo != null)&&(sModelo.trim().length()>0)) {
        
            sModelo = sModelo.toLowerCase().trim();
        }

        if (sTipoRed.equalsIgnoreCase( Medida.CONEXION_WIFI)) {
            sCategoria =  Medida.RED_CABLEADA;
        }
        else if (sSO.equalsIgnoreCase( Medida.SO_ANDROID.toLowerCase()))
        {
            sCategoria =  Medida.RED_MOVIL;
        }
        else if (sSO.equalsIgnoreCase( Medida.SO_IOS.toLowerCase()) || sModelo.equalsIgnoreCase( Medida.MODELO_IPHONE.toLowerCase()))
        {
            sCategoria =  Medida.RED_MOVIL;
        }
        else if (sSO.equalsIgnoreCase( Medida.SO_OSX.toLowerCase()) || sModelo.equalsIgnoreCase( Medida.MODELO_MAC.toLowerCase()))
        {
            sCategoria =  Medida.RED_CABLEADA;
        }
        else if (sSO.equalsIgnoreCase( Medida.SO_LINUX.toLowerCase()) || sModelo.equalsIgnoreCase( Medida.MODELO_LINUX.toLowerCase()))
        {
            sCategoria =  Medida.RED_CABLEADA;
        }
        else if (sSO.equalsIgnoreCase( Medida.SO_WINDOWS.toLowerCase()) || sModelo.equalsIgnoreCase( Medida.MODELO_PC.toLowerCase()))
        {
            sCategoria =  Medida.RED_CABLEADA;
        }
        else {
            if (sTipoRed.equalsIgnoreCase( Medida.CONEXION_2G) || sTipoRed.equalsIgnoreCase( Medida.CONEXION_3G) || sTipoRed.equalsIgnoreCase( Medida.CONEXION_4G) || sTipoRed.equalsIgnoreCase( Medida.CONEXION_5G) || sTipoRed.equalsIgnoreCase( Medida.CONEXION_MOBILE) || sTipoRed.equalsIgnoreCase( Medida.CONEXION_CELLULAR)) {
                sCategoria =  Medida.RED_MOVIL;
            } else if (sTipoRed.equalsIgnoreCase( Medida.CONEXION_ETH)) {
                sCategoria =  Medida.RED_CABLEADA;
            }
        }

        return sCategoria;
    }
	  
	 
	public String registrarDatosCobertura(Medida medida) throws Exception{
		medida = this.calcularValores(medida);
	
		
		try{
			retryService.saveMedida(medida); 
		
		}
		catch(Exception ex){
			  log.debug("devuelvo error");
			return "{\"estadoRespuesta\":0,\"mensajeRespuesta\":\"Faltan datos "+
		(ex.getLocalizedMessage()!=null ? ex.getLocalizedMessage().replaceAll("\n"," ").replaceAll("\r"," ").replaceAll("\"", "\\\\\""):"")+"\"}";
			 
		}
		log.debug("devuelvo correcto");
		updateBest(medida);
		updateCell(medida);
		return "{\"estadoRespuesta\":1}";
	}
	
	@Async
	void updateBest(Medida medida) {
		
		try{
			if (medida.getVelocidadBajada()!=null){
				String anio = new Integer(medida.getTimestamp().get(Calendar.YEAR)).toString();
				log.debug(anio+","+ medida.getIne()+","+ medida.getCategoria());
				String best = medidaRepository.getTheBest(anio, medida.getIne(), medida.getCategoria());
				String calidad = medidaRepository.getCalidad(medida.getVelocidadBajada(), medida.getCategoria());
				if (best==null){
					medidaRepository.insertBest(anio, medida.getIne(), medida.getCategoria(), calidad);
				}
				else if ( calidad.compareTo(best) > 0){
					medidaRepository.updateBest(anio, medida.getIne(), medida.getCategoria(), calidad);
				}
				
			}
			}
			catch(Exception ex){
			log.error("No se ha podido actualizar la mejor medida por año, municipio y categoria ",ex);
				 
			}
	}
	@Async
	void updateCell(Medida medida) {
		try{
			 if (medida.getCategoria().equalsIgnoreCase(Medida.RED_CABLEADA)){
			 int upd = medidaRepository.updateCellRedFija(medida.getCoordenadax(),medida.getCoordenaday());
			 if (upd==0){
				 medidaRepository.insertCellRedFija(medida.getCoordenadax(),medida.getCoordenaday());
			 }
			 }
			 else if (medida.getCategoria().equalsIgnoreCase(Medida.RED_MOVIL)) {
				 int upd = medidaRepository.updateCellRedMovil(medida.getCoordenadax(),medida.getCoordenaday());
				 if (upd==0){
					 medidaRepository.insertCellRedMovil(medida.getCoordenadax(),medida.getCoordenaday());
				 }
			 }
			}
			catch(Exception ex){
			log.error("No se ha podido actualizar la celda "+medida.getCoordenadax()+", "+medida.getCoordenaday(),ex);
				 
			}
	}
}
