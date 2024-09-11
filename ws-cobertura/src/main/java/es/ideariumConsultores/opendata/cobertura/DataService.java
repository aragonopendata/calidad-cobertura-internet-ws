package es.ideariumConsultores.opendata.cobertura;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.Statement;

import javax.sql.DataSource;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;



@Service
public class DataService {

	public final static Logger log = LoggerFactory.getLogger("cobertura");
	@Autowired
	DataSource dataSource;

	public String getData(String capa, Integer municipio, Integer anyo) throws Exception{
		StringBuilder out=new StringBuilder();
		Connection conn = dataSource.getConnection();
		try{
			PreparedStatement stmt=conn.prepareStatement("SELECT * FROM cyt_datos_origen where capa=?");
			stmt.setString(1,capa);
			ResultSet rs = stmt.executeQuery();
			if(rs.next()){
				String where="";
				if(anyo!=null){
					if (rs.getString("campo_anyo")==null){
						rs.close();
						stmt.close();
						throw new Exception("La capa "+capa+" no admite filtro por año");
					}
					where+=rs.getString("campo_anyo")+"="+anyo;
				}
				if(municipio!=null){
					if (rs.getString("campo_municipio")==null){
						rs.close();
						stmt.close();
						throw new Exception("La capa "+capa+" no admite filtro por municipio");
					}
					if (where.length()>0){
						where+=" AND ";
					}
					where+=rs.getString("campo_municipio")+"="+municipio;
				}
				Statement stmt_data=conn.createStatement();
				log.debug("SELECT "+rs.getString("campos")+" FROM "+rs.getString("tabla")+(where.length()>0  ? " WHERE "+where:"")+(rs.getString("orden")!=null ? " ORDER BY "+rs.getString("orden"):""));
				ResultSet rs_data=stmt_data.executeQuery("SELECT "+rs.getString("campos")+" FROM "+rs.getString("tabla")+(where.length()>0 ? " WHERE "+where:"")+(rs.getString("orden")!=null ? " ORDER BY "+rs.getString("orden"):""));
				out.append("[");
				while (rs_data.next()){
					out.append("{"+getAttributes(rs_data)+"}");
					if (!rs_data.isLast()){
						out.append(",");
					}
				}
				out.append("]");
			}
			else{
				throw new Exception("La capa "+capa+" no está configurada para obtener sus datos en cyt_datos_origen");
			}
			return out.toString();
		}
		finally{
			conn.close();
		}
	}

	public String getSummary( Integer municipio) throws Exception{
		StringBuilder out=new StringBuilder();
		Connection conn = dataSource.getConnection();
		try{
			Statement stmt=conn.createStatement();
			ResultSet rs;
			if (municipio!=null){
				rs = stmt.executeQuery("SELECT label FROM v_cyt_resumen_municipios where c_muni_ine="+municipio+" order by orden");
			}
			else{
				rs = stmt.executeQuery("SELECT label FROM v_cyt_resumen_aragon order by orden");
			}
				out.append("[");
				while (rs.next()){
					out.append("\""+rs.getString("label").replaceAll("\"", "\\\\\"")+"\"");
					if (!rs.isLast()){
						out.append(",");
					}
				}
				out.append("]");
			return out.toString();
		}
		finally{
			conn.close();
		}
	}

	protected String getAttributes(ResultSet rs) throws Exception{
		String datos="";

		ResultSetMetaData md = rs.getMetaData();
		int columnCount = md.getColumnCount();
		boolean primero=true;
		for (int i=1; i<=columnCount;i++) {
			String val=rs.getString(i);
			if (val!=null){
				if (!primero){
					datos+=",";
				}

				datos+="\""+md.getColumnName(i)+"\":\""+val.replaceAll("\r\n"," ").replaceAll("\n"," ").replaceAll("\r"," ").replaceAll("\\\\","\\\\\\\\").replaceAll("\"","\\\\\"").trim()+"\"";
				primero=false;
			}
		}			
		return datos;

	}
}
