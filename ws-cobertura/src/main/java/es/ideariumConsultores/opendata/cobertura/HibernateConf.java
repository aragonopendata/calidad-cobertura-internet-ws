package es.ideariumConsultores.opendata.cobertura;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.jdbc.DataSourceBuilder;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.annotation.Primary;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;
import org.springframework.orm.hibernate4.HibernateTransactionManager;
import org.springframework.orm.hibernate4.LocalSessionFactoryBean;
import org.springframework.orm.jpa.JpaTransactionManager;
import org.springframework.transaction.PlatformTransactionManager;
import org.springframework.transaction.annotation.EnableTransactionManagement;

import javax.sql.DataSource;
import java.util.Properties;

@Configuration
@EnableTransactionManagement
@EnableJpaRepositories(basePackages = "es.ideariumConsultores.opendata.cobertura.repository",
        transactionManagerRef = "dbTransactionManager")
public class HibernateConf {


    @Value("${spring.datasource.username}")
    String user;
    @Value("${spring.datasource.password}")
    String pass;
    @Value("${spring.datasource.url}")
    String url;
    @Value("${spring.datasource.driverClassName}")
    String driver;



    @Value("${ddl-auto}")
    String ddl;

    @Value("${spring.jpa.properties.hibernate.temp.use_jdbc_metadata_defaults}")
    String use_jdbc_metadata_defaults;



    @Bean
    public LocalSessionFactoryBean sessionFactory() {
        LocalSessionFactoryBean sessionFactory = new LocalSessionFactoryBean();
        sessionFactory.setDataSource(dataSource());
        sessionFactory.setPackagesToScan(
                "es.ideariumConsultores.opendata.cobertura.model" );
        sessionFactory.setHibernateProperties(hibernateProperties());

        return sessionFactory;
    }

    @ConfigurationProperties("spring.datasource")
    @Bean
    @Primary
    public DataSource dataSource() {
        DataSource dataSource = DataSourceBuilder.create()
                .build();
        return dataSource;
    }


    @Bean(name="dbTransactionManager")
    public PlatformTransactionManager hibernateTransactionManager() {
        HibernateTransactionManager transactionManager
                = new HibernateTransactionManager();
        transactionManager.setSessionFactory(sessionFactory().getObject());
        return transactionManager;
    }

    private final Properties hibernateProperties() {
        Properties hibernateProperties = new Properties();
        hibernateProperties.setProperty(
                "hibernate.hbm2ddl.auto", ddl);
        hibernateProperties.setProperty(
                "hibernate.dialect", "org.hibernate.dialect.PostgreSQLDialect");
        hibernateProperties.setProperty("hibernate.temp.use_jdbc_metadata_defaults", use_jdbc_metadata_defaults);



        return hibernateProperties;
    }
}
