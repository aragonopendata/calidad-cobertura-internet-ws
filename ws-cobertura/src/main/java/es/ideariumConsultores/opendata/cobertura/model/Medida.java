package es.ideariumConsultores.opendata.cobertura.model;

import java.util.Calendar;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.google.gson.annotations.Expose;

@Entity
@Table(name = "t_calidad_cobertura_medidas", schema = "opendata_usr")
public class Medida {
	  public static String CAMPO_SIN_DATOS = "Sin datos";
      public static String CONEXION_2G = "2G";
      public static String CONEXION_GSM = "GSM";
      public static String CONEXION_GPRS = "GPRS";
      public static String CONEXION_EDGE = "EDGE";

      public static String CONEXION_3G = "3G";
      public static String CONEXION_CDMA = "CDMA";
      public static String CONEXION_UMTS = "UMTS";
      public static String CONEXION_HSPA  = "HSPA";
      public static String CONEXION_HSUPA = "HSUPA";
      public static String CONEXION_HSDPA = "HSDPA";
      public static String CONEXION_1XRTT = "1XRTT";
      public static String CONEXION_EHRPD = "EHRPD";
   
      public static String CONEXION_4G = "4G";
      public static String CONEXION_LTE = "LTE";
      public static String CONEXION_UMB = "UMB";
      public static String CONEXION_HSPA_PLUS = "HSPA+";

      public static String CONEXION_5G = "5G";
      public static String CONEXION_MOBILE = "MOBILE";
      public static String CONEXION_CELLULAR = "CELLULAR";
      public static String CONEXION_WIFI = "WIFI";
      public static String CONEXION_WIMAX = "WIMAX";
      public static String CONEXION_ETH = "ETH";

      public static String RED_MOVIL = "RED MOVIL";
      public static String RED_CABLEADA = "RED FIJA";
      public static String RED_DESCONOCIDA = "DESCONOCIDO";

      public static String SO_IOS = "iOS";
      public static String SO_ANDROID = "Android";
      public static String SO_WINDOWS = "Windows";
      public static String SO_OSX = "OSX";
      public static String SO_LINUX = "Linux";

      public static String MODELO_PC = "PC";
      public static String MODELO_IPHONE = "iPhone";
      public static String MODELO_MAC = "Mac";
      public static String MODELO_LINUX = "Linux";

      public static String ZONA_SIN_DATOS = "Zona sin datos";
 
	 @Expose
	    @Id
	    @SequenceGenerator(name="medidas_id_seq",
	            sequenceName="opendata_usr.medidas_id_seq",
	            allocationSize=1)
	    @GeneratedValue(strategy=GenerationType.SEQUENCE, generator="medidas_id_seq")
	    int id;
	 
	 @JsonFormat(pattern="yyyy-MM-dd HH:mm:ss'Z'")
	 @Column(name="fecha")
       Calendar timestamp;// Fecha y hora(en la franja horaria UTC) en la que se ha realizado la captura de datos de cobertura. En formato “YYYY-MM-dd HH:mm:ssZ”.
            
            Integer coordenadax;// Coordenada X en el estándar EPSG:25830 de la posición geográfica anonimizada del usuario que ha realizado la captura de datos de cobertura. Rango 500m
            Integer coordenaday;// Coordenada Y en el estándar EPSG:25830 de la posición geográfica anonimizada del usuario que ha realizado la captura de datos de cobertura. Rango 500m
        
            String municipio;// Nombre del municipio en el que se encuentra el usuario que ha realizado la captura de datos de cobertura.
           Integer ine;// INE del municipio en el que se encuentra el usuario que ha realizado la captura de datos de cobertura.
           
           String modelo;// Modelo del dispositivo del usuario que está realizando la captura.
          
           String so;// Sistema Operativo del dispositivo del usuario que está realizando la captura. F
           
           @Column(name="tipored")
           String tipoRed;// Tipo de Red (2G, 3G, 4G, 5G, WiFI, etc.) a la que está conectado dispositivo del usuario que está realizando la captura.
           
           String operador;// Operador(Movistar, Vodafone, Orange, etc.) al que está conectado el dispositivo del usuario que está realizando la captura.
           
           @Column(name="valorintensidadsenial")
           Double valorIntensidadSenial;// Intensidad de la señal de la red a la que está conectado el dispositivo del usuario que está realizando la captura expresada en dBm.
           
           @Column(name="velocidadbajada")
           Double velocidadBajada;
             @Column(name="velocidadsubida")
           Double velocidadSubida;
                Double latencia;
           String categoria; // indica si la red es tipo CABLEADA o RED MOVIL


    public Medida() {
 
    }


	public int getId() {
		return id;
	}


	public void setId(int id) {
		this.id = id;
	}


	public Calendar getTimestamp() {
		return timestamp;
	}


	public void setTimestamp(Calendar timestamp) {
		this.timestamp = timestamp;
	}


	public Integer getCoordenadax() {
		return coordenadax;
	}


	public void setCoordenadax(Integer coordenadax) {
		this.coordenadax = coordenadax;
	}


	public Integer getCoordenaday() {
		return coordenaday;
	}


	public void setCoordenaday(Integer coordenaday) {
		this.coordenaday = coordenaday;
	}




	public String getMunicipio() {
		return municipio;
	}


	public void setMunicipio(String municipio) {
		this.municipio = municipio;
	}


	
	public Integer getIne() {
		return ine;
	}


	public void setIne(Integer ine) {
		this.ine = ine;
	}


	public String getModelo() {
		return modelo;
	}


	public void setModelo(String modelo) {
		this.modelo = modelo;
	}


	public String getSo() {
		return so;
	}


	public void setSo(String so) {
		this.so = so;
	}


	public String getTipoRed() {
		return tipoRed;
	}


	public void setTipoRed(String tipoRed) {
		this.tipoRed = tipoRed;
	}


	public String getOperador() {
		return operador;
	}


	public void setOperador(String operador) {
		this.operador = operador;
	}


	public Double getValorIntensidadSenial() {
		return valorIntensidadSenial;
	}


	public void setValorIntensidadSenial(Double valorIntensidadSenial) {
		this.valorIntensidadSenial = valorIntensidadSenial;
	}




	public Double getVelocidadBajada() {
		return velocidadBajada;
	}


	public void setVelocidadBajada(Double velocidadBajada) {
		this.velocidadBajada = velocidadBajada;
	}


	

	public Double getVelocidadSubida() {
		return velocidadSubida;
	}


	public void setVelocidadSubida(Double velocidadSubida) {
		this.velocidadSubida = velocidadSubida;
	}



	public Double getLatencia() {
		return latencia;
	}


	public void setLatencia(Double latencia) {
		this.latencia = latencia;
	}


	public String getCategoria() {
		
        return this.categoria;
	}


	public void setCategoria(String categoria) {
		
        	this.categoria = categoria;
        
		
	}
	

}
