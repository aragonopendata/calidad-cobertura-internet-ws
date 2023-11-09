package es.ideariumConsultores.opendata.cobertura;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.Statement;

import javax.sql.DataSource;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;



@Service
public class VisorService {

	public final static Logger log = LoggerFactory.getLogger("cobertura");
	@Autowired
	DataSource dataSource;

	@Autowired
	DataService dataService;

	public String getToc() throws Exception{
		StringBuilder out=new StringBuilder();
		Connection conn = dataSource.getConnection();
		try{
			Statement stmt=conn.createStatement(ResultSet.TYPE_SCROLL_SENSITIVE,ResultSet.CONCUR_READ_ONLY);
			ResultSet rs = stmt.executeQuery("SELECT * FROM cyt_toc_grupos where padre is null order by orden");
			out.append("{\"grupos\":[");

			boolean primero=true;

			while (rs.next()) {
				if (!primero){
					out.append(",");
				}
				primero=false;
				out.append("{");
				out.append(dataService.getAttributes(rs)+","+getChildren(conn,rs.getInt("pk")));
				out.append("}");
			}

			out.append("]}");	
			rs.close();
			stmt.close();
			return out.toString();
		}
		finally{
			conn.close();
		}
	}

	public String getQueryableLayers() throws Exception{
		StringBuilder out=new StringBuilder();
		Connection conn = dataSource.getConnection();
		try{
			Statement stmt=conn.createStatement(ResultSet.TYPE_SCROLL_SENSITIVE,ResultSet.CONCUR_READ_ONLY);
			ResultSet rs = stmt.executeQuery("SELECT * FROM  cyt_capas_consulta WHERE consultable");
			out.append("{\"capas\":[");

			boolean primero=true;

			while (rs.next()) {
				if (!primero){
					out.append(",");
				}
				primero=false;
				out.append("{");


				out.append(dataService.getAttributes(rs)+","+getAtributosCapas(conn,rs.getString("pk")));
				out.append("}");
			}
			rs.close();
			stmt.close();
			out.append("]}");
			return out.toString();
		}
		finally{
			conn.close();
		}
	}

	public String getAtributosCapas(Connection conn, String pk) throws Exception {
		Statement stmt = null;//Objeto para la ejecucion de sentencias
		ResultSet rs=null;
		String resultado="";

		stmt = conn.createStatement(ResultSet.TYPE_SCROLL_SENSITIVE,
				ResultSet.CONCUR_UPDATABLE);
		rs = stmt.executeQuery("SELECT campo, etiqueta,\"mainField\" FROM  cyt_capas_atributos where pk_capa='" + pk + "' order by orden");
		resultado="\"campos\":[";
		while (rs.next()){
			resultado+="{"+dataService.getAttributes(rs)+"}";
			if (!rs.isLast()){
				resultado+=",";
			}
		}


		resultado+="]";			
		rs.close();
		stmt.close();

		return resultado;


	}
	protected String getChildren(Connection conn,int grupo) throws Exception{
		Statement stmt = null;//Objeto para la ejecucion de sentencias
		ResultSet rs=null;
		String resultado="";

		stmt = conn.createStatement(ResultSet.TYPE_SCROLL_SENSITIVE,
				ResultSet.CONCUR_UPDATABLE);
		rs = stmt.executeQuery("SELECT * FROM cyt_toc_grupos where padre ="+grupo+" order by orden");
		resultado="\"grupos\":[";
		while (rs.next()){
			resultado+="{"+dataService.getAttributes(rs)+","+getChildren(conn,rs.getInt("pk"))+"}";
			if (!rs.isLast()){
				resultado+=",";
			}
		}
		resultado+="]";
		rs = stmt.executeQuery("SELECT * FROM v_cyt_toc_wms where grupo ="+grupo+" order by orden");
		resultado+=",\"capasWMS\":[";
		while (rs.next()){
			resultado+="{"+dataService.getAttributes(rs);

			rs.getInt("anyo_defecto");
			Statement stmt_datos = conn.createStatement(ResultSet.TYPE_SCROLL_SENSITIVE,
					ResultSet.CONCUR_UPDATABLE);
			ResultSet rs_datos=null;
			if (!rs.wasNull()){

				rs_datos = stmt_datos.executeQuery("select anyo from cyt_capas_anyos where capa='"+rs.getString("layers")+"' order by anyo");
				resultado+=",\"anyos\":[";
				while (rs_datos.next()){
					resultado+=rs_datos.getInt("anyo");
					if (!rs_datos.isLast()){
						resultado+=",";
					}
				}
				resultado+="]";
			}
			rs_datos = stmt_datos.executeQuery("select campo_municipio, campo_anyo from cyt_datos_origen where  capa='"+rs.getString("layers")+"'");
			rs_datos.next();
			String campo_muni = rs_datos.getString("campo_municipio");
			if (rs_datos.getString("campo_anyo")!=null){
				resultado+=",\"campo_anyo\":\""+rs_datos.getString("campo_anyo")+"\"";
			}
			resultado+=",\"filtro_muni\":";
			if (campo_muni!=null){
				resultado+="true";

			}
			else{
				resultado+="false";
			}


			rs_datos.close();
			stmt_datos.close();
			resultado+="}";
			if (!rs.isLast()){
				resultado+=",";
			}
		}
		resultado+="]";




		rs.close();
		stmt.close();
		return resultado;


	}



}
