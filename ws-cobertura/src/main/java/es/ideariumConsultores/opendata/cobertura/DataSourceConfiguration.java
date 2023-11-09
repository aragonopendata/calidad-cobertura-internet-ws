package es.ideariumConsultores.opendata.cobertura;


import java.sql.Connection;
import java.sql.SQLException;
import java.util.HashMap;

import javax.sql.DataSource;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.core.env.Environment;
import org.springframework.stereotype.Service;

@Service
public class DataSourceConfiguration {

    private HashMap<String, DataSource> hash = new HashMap();

    @Autowired
    private Environment env;


    
    public DataSource getDataSource(String conex){
    	org.apache.tomcat.jdbc.pool.DataSource ds = new org.apache.tomcat.jdbc.pool.DataSource();
    	ds.setDriverClassName(env.getProperty("driver.postgresql"));
    	ds.setUsername(env.getProperty(conex+".username"));
    	ds.setPassword(env.getProperty(conex+".password"));
    	ds.setUrl(env.getProperty(conex+".url"));
    	ds.setTestOnBorrow(true);
    	ds.setValidationQuery("select 1;");
        return ds;
    }



    public Connection getConnection(String con) throws SQLException {

    	DataSource d = this.hash.get(con);
      if (d==null){
    	  d = getDataSource(con);
    	  this.hash.put(con,d);
      }
        
            
        Connection    c = d.getConnection();
           
            return c;
        
    }
}
