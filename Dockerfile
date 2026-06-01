FROM maven:3.6.3-openjdk-8 AS builder
WORKDIR /build
COPY ws-cobertura /build
RUN mvn package -P produccion -DskipTests

FROM eclipse-temurin:8-jre-jammy
WORKDIR /app
COPY --from=builder /build/target/ws-cobertura.jar ws-cobertura.jar
ENTRYPOINT ["java", "-jar", "ws-cobertura.jar"]
