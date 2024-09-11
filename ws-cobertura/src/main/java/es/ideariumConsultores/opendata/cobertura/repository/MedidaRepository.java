package es.ideariumConsultores.opendata.cobertura.repository;

import java.util.Calendar;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.transaction.annotation.Transactional;

import es.ideariumConsultores.opendata.cobertura.model.Medida;

public interface MedidaRepository extends JpaRepository<Medida,Integer> {
	  @Modifying
	    @Query(value = "update opendata_usr.vm_calidad_cobertura_red_fija vm set municipios=v.municipios,categoria=v.categoria,num_medidas=v.num_medidas,desde=v.desde,hasta=v.hasta,cuadricula=v.cuadricula,geom_25830=v.geom_25830,latencia=v.latencia,valorintensidadsenial=v.valorintensidadsenial,velocidadbajada=v.velocidadbajada,velocidadsubida=v.velocidadsubida,rangolatencia=v.rangolatencia,rangointensidadsenial=v.rangointensidadsenial,rangovelocidadbajada=v.rangovelocidadbajada,rangovelocidadsubida=v.rangovelocidadsubida from opendata_usr.v_calidad_cobertura_red_fija v where  vm.coordenadax=v.coordenadax and vm.coordenaday=v.coordenaday and vm.coordenadax=:coordenadax and vm.coordenaday=:coordenaday", nativeQuery = true)
	    @Transactional
	    int updateCellRedFija(@Param("coordenadax") int coordenadax,@Param("coordenaday") int coordenaday);
	  
	  @Modifying
	    @Query(value = "update opendata_usr.vm_calidad_cobertura_red_movil vm set municipios=v.municipios,categoria=v.categoria,num_medidas=v.num_medidas,desde=v.desde,hasta=v.hasta,cuadricula=v.cuadricula,geom_25830=v.geom_25830,latencia=v.latencia,valorintensidadsenial=v.valorintensidadsenial,velocidadbajada=v.velocidadbajada,velocidadsubida=v.velocidadsubida,rangolatencia=v.rangolatencia,rangointensidadsenial=v.rangointensidadsenial,rangovelocidadbajada=v.rangovelocidadbajada,rangovelocidadsubida=v.rangovelocidadsubida from opendata_usr.v_calidad_cobertura_red_movil v where vm.coordenadax=v.coordenadax and vm.coordenaday=v.coordenaday and vm.coordenadax=:coordenadax and vm.coordenaday=:coordenaday", nativeQuery = true)
	    @Transactional
	    int updateCellRedMovil(@Param("coordenadax") int coordenadax,@Param("coordenaday") int coordenaday);

	  @Modifying
	    @Query(value = "update opendata_usr.vm_calidad_cobertura_best vm set calidad=:calidad , fecha=:fecha where ine=:ine and categoria=:categoria", nativeQuery = true)
	    @Transactional
	    int updateBest(@Param("fecha") Calendar fecha,@Param("ine") int ine,@Param("categoria") String categoria,@Param("calidad") String calidad);

	    @Query(value = "select calidad from opendata_usr.vm_calidad_cobertura_best vm where categoria=:categoria and ine=:ine", nativeQuery = true)
	    String getTheBest(@Param("ine") int ine,@Param("categoria") String categoria);

	    @Query(value = "select vb.id||' - '||vb.descripcion from codrangovelocidadbajada vb where calcularrangovelocidadbajada(:velocidadbajada, :categoria) = vb.id", nativeQuery = true)
	    String getCalidad(@Param("velocidadbajada") double velocidad, @Param("categoria") String categoria);
	    
	    @Query(value = "select calidad from v_calidad_cobertura_red_fija_info where  coordenadax=:x and  coordenaday=:y", nativeQuery = true)
	    String getCalidadRedFija(@Param("x") double x,@Param("y") double y);
	    
	    @Query(value = "select calidad from v_calidad_cobertura_red_movil_info where coordenadax=:x and  coordenaday=:y", nativeQuery = true)
	    String getCalidadRedMovil(@Param("x") double x,@Param("y") double y);
	    
	  @Modifying
	    @Query(value = "insert into opendata_usr.vm_calidad_cobertura_red_fija (coordenadax, coordenaday, municipios,categoria,num_medidas,desde,hasta,cuadricula,geom_25830,latencia,valorintensidadsenial,velocidadbajada,velocidadsubida,rangolatencia,rangointensidadsenial,rangovelocidadbajada,rangovelocidadsubida) select coordenadax, coordenaday, municipios, categoria,num_medidas,desde,hasta,cuadricula,geom_25830,latencia,valorintensidadsenial,velocidadbajada,velocidadsubida,rangolatencia,rangointensidadsenial,rangovelocidadbajada,rangovelocidadsubida from opendata_usr.v_calidad_cobertura_red_fija t where coordenadax=:coordenadax and coordenaday=:coordenaday", nativeQuery = true)
	    @Transactional
	    int insertCellRedFija(@Param("coordenadax") int coordenadax,@Param("coordenaday") int coordenaday);
	  
	  @Modifying
	    @Query(value = "insert into opendata_usr.vm_calidad_cobertura_red_movil (coordenadax, coordenaday, municipios, categoria,num_medidas,desde,hasta,cuadricula,geom_25830,latencia,valorintensidadsenial,velocidadbajada,velocidadsubida,rangolatencia,rangointensidadsenial,rangovelocidadbajada,rangovelocidadsubida) select coordenadax, coordenaday, municipios, categoria,num_medidas,desde,hasta,cuadricula,geom_25830,latencia,valorintensidadsenial,velocidadbajada,velocidadsubida,rangolatencia,rangointensidadsenial,rangovelocidadbajada,rangovelocidadsubida from opendata_usr.v_calidad_cobertura_red_movil t where coordenadax=:coordenadax and coordenaday=:coordenaday", nativeQuery = true)
	    @Transactional
	    int insertCellRedMovil(@Param("coordenadax") int coordenadax,@Param("coordenaday") int coordenaday);

	  @Modifying
	    @Query(value = "insert into opendata_usr.vm_calidad_cobertura_best (ine, categoria,fecha, calidad) VALUES (:ine,:categoria,:fecha,:calidad)", nativeQuery = true)
	    @Transactional
	    int insertBest(@Param("fecha") Calendar fecha,@Param("ine") int ine,@Param("categoria") String categoria,@Param("calidad") String calidad);

}
