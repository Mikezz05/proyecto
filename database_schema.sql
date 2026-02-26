-- =======================================================================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS - SISTEMA PINTO SALINAS (VERSIÓN AUDITADA 3FN)
-- =======================================================================================
-- Este script garantiza la 3ra Forma Normal (3FN) y elimina la necesidad de ingresar
-- seriales manualmente, vinculando a los usuarios directamente con su entidad (Docente/Admin).
-- =======================================================================================

CREATE DATABASE IF NOT EXISTS escuela_pinto_salinas;
USE escuela_pinto_salinas;

-- 1. TABLA DE CONFIGURACIÓN GLOBAL (Evita valores quemados en el código)
CREATE TABLE IF NOT EXISTS configuracion_sistema (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nota_minima_aprobatoria DECIMAL(4,2) NOT NULL DEFAULT 10.00,
    porcentaje_evaluacion_1 DECIMAL(5,2) NOT NULL DEFAULT 30.00,
    porcentaje_evaluacion_2 DECIMAL(5,2) NOT NULL DEFAULT 30.00,
    porcentaje_evaluacion_3 DECIMAL(5,2) NOT NULL DEFAULT 40.00,
    fecha_actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Insertar configuración por defecto si no existe
INSERT INTO configuracion_sistema (id, nota_minima_aprobatoria) 
SELECT 1, 10.00 FROM DUAL 
WHERE NOT EXISTS (SELECT 1 FROM configuracion_sistema WHERE id = 1);

-- 2. TABLA DE USUARIOS (Autenticación centralizada)
-- Catálogos: roles y estados de usuario para máxima claridad
CREATE TABLE IF NOT EXISTS roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(255)
);

INSERT INTO roles (nombre) SELECT 'Admin' WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre='Admin');
INSERT INTO roles (nombre) SELECT 'Docente' WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre='Docente');
INSERT INTO roles (nombre) SELECT 'Estudiante' WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre='Estudiante');

CREATE TABLE IF NOT EXISTS estados_usuario (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO estados_usuario (nombre) SELECT 'Activo' WHERE NOT EXISTS (SELECT 1 FROM estados_usuario WHERE nombre='Activo');
INSERT INTO estados_usuario (nombre) SELECT 'Inactivo' WHERE NOT EXISTS (SELECT 1 FROM estados_usuario WHERE nombre='Inactivo');
INSERT INTO estados_usuario (nombre) SELECT 'Bloqueado' WHERE NOT EXISTS (SELECT 1 FROM estados_usuario WHERE nombre='Bloqueado');

-- Tabla de usuarios ahora referencia roles y estados por FK (explicito y auditable)
CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role_id INT NOT NULL,
    email VARCHAR(255) UNIQUE,
    security_q VARCHAR(255),
    security_a VARCHAR(255),
    estado_usuario_id INT NOT NULL DEFAULT 1,
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE RESTRICT,
    FOREIGN KEY (estado_usuario_id) REFERENCES estados_usuario(id) ON DELETE RESTRICT
);

-- 3. TABLA DE ADMINISTRATIVOS (Datos específicos del personal admin)
CREATE TABLE IF NOT EXISTS administrativos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL UNIQUE,
    ci VARCHAR(20) NOT NULL UNIQUE,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    cargo VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- 4. TABLA DE DOCENTES (Datos específicos de los profesores)
CREATE TABLE IF NOT EXISTS docentes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL UNIQUE,
    ci VARCHAR(20) NOT NULL UNIQUE,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    especialidad VARCHAR(100),
    telefono VARCHAR(20),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- 5. TABLA DE ESTUDIANTES (Datos personales, separados de la inscripción)
CREATE TABLE IF NOT EXISTS estudiantes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT UNIQUE, -- Opcional: Si el estudiante tiene acceso al sistema
    ci VARCHAR(20) NOT NULL UNIQUE,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    correo VARCHAR(255),
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE SET NULL
);

-- 6. TABLA DE CURSOS (Catálogo de cursos disponibles)
-- Catálogo de estados para cursos
CREATE TABLE IF NOT EXISTS estados_curso (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO estados_curso (nombre) SELECT 'Activo' WHERE NOT EXISTS (SELECT 1 FROM estados_curso WHERE nombre='Activo');
INSERT INTO estados_curso (nombre) SELECT 'Inactivo' WHERE NOT EXISTS (SELECT 1 FROM estados_curso WHERE nombre='Inactivo');

CREATE TABLE IF NOT EXISTS cursos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre_curso VARCHAR(150) NOT NULL UNIQUE,
    duracion_meses INT NOT NULL,
    docente_id INT,
    estado_curso_id INT NOT NULL DEFAULT 1,
    FOREIGN KEY (docente_id) REFERENCES docentes(id) ON DELETE SET NULL,
    FOREIGN KEY (estado_curso_id) REFERENCES estados_curso(id) ON DELETE RESTRICT
);

-- 4.b TABLAS DE SERIALES VÁLIDOS (para procesos de validación inicial)
CREATE TABLE IF NOT EXISTS seriales_validos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    serial VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS seriales_admin (
    id INT AUTO_INCREMENT PRIMARY KEY,
    serial VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 7. TABLA DE INSCRIPCIONES (Relación N:M entre Estudiantes y Cursos)
-- Catálogo de estados para inscripciones
CREATE TABLE IF NOT EXISTS estados_inscripcion (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO estados_inscripcion (nombre) SELECT 'Cursando' WHERE NOT EXISTS (SELECT 1 FROM estados_inscripcion WHERE nombre='Cursando');
INSERT INTO estados_inscripcion (nombre) SELECT 'Aprobado' WHERE NOT EXISTS (SELECT 1 FROM estados_inscripcion WHERE nombre='Aprobado');
INSERT INTO estados_inscripcion (nombre) SELECT 'Reprobado' WHERE NOT EXISTS (SELECT 1 FROM estados_inscripcion WHERE nombre='Reprobado');
INSERT INTO estados_inscripcion (nombre) SELECT 'Retirado' WHERE NOT EXISTS (SELECT 1 FROM estados_inscripcion WHERE nombre='Retirado');

CREATE TABLE IF NOT EXISTS inscripciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    estudiante_id INT NOT NULL,
    curso_id INT NOT NULL,
    fecha_inscripcion DATETIME DEFAULT CURRENT_TIMESTAMP,
    requisitos_verificados BOOLEAN DEFAULT FALSE,
    estado_inscripcion_id INT NOT NULL DEFAULT 1,
    UNIQUE KEY uk_estudiante_curso (estudiante_id, curso_id), -- Evita doble inscripción
    FOREIGN KEY (estudiante_id) REFERENCES estudiantes(id) ON DELETE CASCADE,
    FOREIGN KEY (curso_id) REFERENCES cursos(id) ON DELETE CASCADE,
    FOREIGN KEY (estado_inscripcion_id) REFERENCES estados_inscripcion(id) ON DELETE RESTRICT
);

-- 8. TABLA DE CALIFICACIONES (Depende de la inscripción, no del estudiante suelto)
CREATE TABLE IF NOT EXISTS calificaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    inscripcion_id INT NOT NULL UNIQUE, -- Una calificación final por inscripción
    nota1 DECIMAL(4,2) DEFAULT 0.00,
    desc1 VARCHAR(100),
    nota2 DECIMAL(4,2) DEFAULT 0.00,
    desc2 VARCHAR(100),
    nota3 DECIMAL(4,2) DEFAULT 0.00,
    desc3 VARCHAR(100),
    nota_final DECIMAL(4,2) DEFAULT 0.00,
    aprobado BOOLEAN DEFAULT FALSE,
    fecha_actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (inscripcion_id) REFERENCES inscripciones(id) ON DELETE CASCADE
);

-- 10. TABLA DE EVALUACIONES (Define cada evaluación posible y porcentaje por defecto)
CREATE TABLE IF NOT EXISTS evaluaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    clave VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    porcentaje_default DECIMAL(5,2) NOT NULL DEFAULT 0.00
);

INSERT INTO evaluaciones (clave, descripcion, porcentaje_default) SELECT 'Eval1','Evaluacion 1',30.00 WHERE NOT EXISTS (SELECT 1 FROM evaluaciones WHERE clave='Eval1');
INSERT INTO evaluaciones (clave, descripcion, porcentaje_default) SELECT 'Eval2','Evaluacion 2',30.00 WHERE NOT EXISTS (SELECT 1 FROM evaluaciones WHERE clave='Eval2');
INSERT INTO evaluaciones (clave, descripcion, porcentaje_default) SELECT 'Eval3','Evaluacion 3',40.00 WHERE NOT EXISTS (SELECT 1 FROM evaluaciones WHERE clave='Eval3');

-- Relación entre configuración global y evaluaciones (permite ver/ajustar pesos por sistema)
CREATE TABLE IF NOT EXISTS configuracion_evaluaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    configuracion_id INT NOT NULL,
    evaluacion_id INT NOT NULL,
    porcentaje DECIMAL(5,2) NOT NULL,
    FOREIGN KEY (configuracion_id) REFERENCES configuracion_sistema(id) ON DELETE CASCADE,
    FOREIGN KEY (evaluacion_id) REFERENCES evaluaciones(id) ON DELETE CASCADE,
    UNIQUE KEY uk_config_eval (configuracion_id, evaluacion_id)
);

-- 11. TABLAS DE AUDITORÍA MÁS EXPLÍCITAS
CREATE TABLE IF NOT EXISTS modulos_auditoria (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS acciones_auditoria (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255)
);

-- Ajuste de la tabla auditoria para referenciar módulos/acciones
CREATE TABLE IF NOT EXISTS auditoria (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    usuario VARCHAR(50) NOT NULL,
    accion_id INT NOT NULL,
    modulo_id INT NOT NULL,
    detalles TEXT,
    ip_origen VARCHAR(45),
    FOREIGN KEY (accion_id) REFERENCES acciones_auditoria(id) ON DELETE RESTRICT,
    FOREIGN KEY (modulo_id) REFERENCES modulos_auditoria(id) ON DELETE RESTRICT
);

-- 9. TABLA DE AUDITORÍA (Registro inmutable de acciones)
CREATE TABLE IF NOT EXISTS auditoria (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    usuario VARCHAR(50) NOT NULL,
    accion VARCHAR(100) NOT NULL,
    modulo VARCHAR(100) NOT NULL,
    detalles TEXT,
    ip_origen VARCHAR(45) -- Recomendado para auditorías reales
);

-- =======================================================================================
-- VISTAS ÚTILES (Para simplificar consultas en C# y evitar JOINs complejos repetitivos)
-- =======================================================================================

CREATE OR REPLACE VIEW vw_estudiantes_cursos AS
SELECT 
    i.id AS inscripcion_id,
    e.id AS estudiante_id,
    e.ci,
    e.nombres,
    e.apellidos,
    c.id AS curso_id,
    c.nombre_curso,
    i.estado AS estado_inscripcion
FROM inscripciones i
JOIN estudiantes e ON i.estudiante_id = e.id
JOIN cursos c ON i.curso_id = c.id;

CREATE OR REPLACE VIEW vw_boleta_calificaciones AS
SELECT 
    i.id AS inscripcion_id,
    e.ci,
    CONCAT(e.nombres, ' ', e.apellidos) AS estudiante,
    c.nombre_curso,
    cal.nota1, cal.nota2, cal.nota3, cal.nota_final,
    CASE WHEN cal.aprobado = 1 THEN 'APROBADO' ELSE 'REPROBADO' END AS estado_materia
FROM calificaciones cal
JOIN inscripciones i ON cal.inscripcion_id = i.id
JOIN estudiantes e ON i.estudiante_id = e.id
JOIN cursos c ON i.curso_id = c.id;
