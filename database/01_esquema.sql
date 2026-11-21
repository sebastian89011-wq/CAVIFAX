DROP DATABASE IF EXISTS constructora_nominas;
CREATE DATABASE constructora_nominas CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE constructora_nominas;

-- 1. PUESTO
CREATE TABLE PUESTO (
    id_puesto INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(40) NOT NULL UNIQUE,
    descripcion VARCHAR(150) NULL
) ENGINE=InnoDB;

-- 2. EMPLEADO
CREATE TABLE EMPLEADO (
    id_empleado INT AUTO_INCREMENT PRIMARY KEY,
    id_puesto INT NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    apellido_paterno VARCHAR(40) NOT NULL,
    apellido_materno VARCHAR(40) NULL,
    telefono CHAR(10) NOT NULL,
    domicilio VARCHAR(200) NOT NULL,
    curp CHAR(18) NOT NULL UNIQUE,
    nss CHAR(11) NOT NULL UNIQUE,
    clave_ine CHAR(18) NOT NULL,
    documentos_completos BOOLEAN NOT NULL DEFAULT FALSE,
    especialidad ENUM('soldador', 'electricista') NULL,
    tipo_contratacion ENUM('permanente', 'subcontratado') NOT NULL,
    fecha_ingreso DATE NOT NULL,
    sueldo_diario DECIMAL(8,2) NOT NULL,
    pago_hora_extra DECIMAL(8,2) NOT NULL,
    estatus ENUM('activo', 'baja_temporal', 'baja_definitiva') NOT NULL DEFAULT 'activo',
    CONSTRAINT fk_empleado_puesto FOREIGN KEY (id_puesto) REFERENCES PUESTO(id_puesto) ON DELETE RESTRICT,
    CONSTRAINT chk_empleado_sueldo CHECK (sueldo_diario > 0),
    CONSTRAINT chk_empleado_he CHECK (pago_hora_extra > 0),
    CONSTRAINT chk_empleado_curp_len CHECK (CHAR_LENGTH(curp) = 18),
    CONSTRAINT chk_empleado_nss_len CHECK (CHAR_LENGTH(nss) = 11)
) ENGINE=InnoDB;

-- 3. CLIENTE
CREATE TABLE CLIENTE (
    id_cliente INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    domicilio VARCHAR(200) NOT NULL
) ENGINE=InnoDB;

-- 4. OBRA
CREATE TABLE OBRA (
    id_obra INT AUTO_INCREMENT PRIMARY KEY,
    id_cliente INT NOT NULL,
    id_responsable INT NOT NULL,
    id_supervisor INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    ubicacion VARCHAR(200) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin_programada DATE NOT NULL,
    fecha_fin_real DATE NULL,
    presupuesto_mano_obra DECIMAL(12,2) NOT NULL,
    limite_alerta_saldo DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    bono_terminacion_por_trabajador DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    estatus ENUM('activa', 'suspendida', 'terminada') NOT NULL DEFAULT 'activa',
    CONSTRAINT fk_obra_cliente FOREIGN KEY (id_cliente) REFERENCES CLIENTE(id_cliente) ON DELETE RESTRICT,
    CONSTRAINT fk_obra_responsable FOREIGN KEY (id_responsable) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT,
    CONSTRAINT fk_obra_supervisor FOREIGN KEY (id_supervisor) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT,
    CONSTRAINT chk_obra_presupuesto CHECK (presupuesto_mano_obra > 0),
    CONSTRAINT chk_obra_alerta CHECK (limite_alerta_saldo >= 0 AND limite_alerta_saldo <= presupuesto_mano_obra),
    CONSTRAINT chk_obra_bono CHECK (bono_terminacion_por_trabajador >= 0),
    CONSTRAINT chk_obra_fechas CHECK (fecha_fin_programada >= fecha_inicio)
) ENGINE=InnoDB;

-- 5. ASISTENCIA
CREATE TABLE ASISTENCIA (
    id_asistencia INT AUTO_INCREMENT PRIMARY KEY,
    id_empleado INT NOT NULL,
    id_obra INT NULL,
    fecha DATE NOT NULL,
    asistio BOOLEAN NOT NULL,
    horas_extra DECIMAL(4,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT fk_asistencia_empleado FOREIGN KEY (id_empleado) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT,
    CONSTRAINT fk_asistencia_obra FOREIGN KEY (id_obra) REFERENCES OBRA(id_obra) ON DELETE RESTRICT,
    CONSTRAINT uq_asistencia_empleado_fecha UNIQUE (id_empleado, fecha),
    CONSTRAINT chk_asistencia_he CHECK (horas_extra >= 0),
    CONSTRAINT chk_asistencia_logica CHECK (
        (asistio = 1 AND id_obra IS NOT NULL) OR 
        (asistio = 0 AND id_obra IS NULL AND horas_extra = 0)
    )
) ENGINE=InnoDB;

-- 6. USUARIO
CREATE TABLE USUARIO (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    id_empleado INT NOT NULL UNIQUE,
    nombre_usuario VARCHAR(30) NOT NULL UNIQUE,
    contrasena_hash VARCHAR(255) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_usuario_empleado FOREIGN KEY (id_empleado) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- 7. NOMINA
CREATE TABLE NOMINA (
    id_nomina INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario_autoriza INT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    estatus ENUM('borrador', 'autorizada', 'pagada') NOT NULL DEFAULT 'borrador',
    fecha_autorizacion DATETIME NULL,
    efectivo_recibido DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT fk_nomina_usuario FOREIGN KEY (id_usuario_autoriza) REFERENCES USUARIO(id_usuario) ON DELETE RESTRICT,
    CONSTRAINT uq_nomina_semana UNIQUE (fecha_inicio, fecha_fin),
    CONSTRAINT chk_nomina_fechas CHECK (fecha_fin >= fecha_inicio),
    CONSTRAINT chk_nomina_efectivo CHECK (efectivo_recibido >= 0)
) ENGINE=InnoDB;

-- 8. DETALLE_NOMINA
CREATE TABLE DETALLE_NOMINA (
    id_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_nomina INT NOT NULL,
    id_empleado INT NOT NULL,
    id_obra INT NOT NULL,
    dias_trabajados TINYINT UNSIGNED NOT NULL,
    horas_extra DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    sueldo_diario_aplicado DECIMAL(8,2) NOT NULL,
    pago_hora_extra_aplicado DECIMAL(8,2) NOT NULL,
    bono DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    costo_bruto DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_detalle_nomina FOREIGN KEY (id_nomina) REFERENCES NOMINA(id_nomina) ON DELETE RESTRICT,
    CONSTRAINT fk_detalle_empleado FOREIGN KEY (id_empleado) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT,
    CONSTRAINT fk_detalle_obra FOREIGN KEY (id_obra) REFERENCES OBRA(id_obra) ON DELETE RESTRICT,
    CONSTRAINT uq_detalle_nomina_emp_obra UNIQUE (id_nomina, id_empleado, id_obra),
    CONSTRAINT chk_detalle_valores CHECK (dias_trabajados >= 0 AND horas_extra >= 0 AND bono >= 0 AND costo_bruto >= 0)
) ENGINE=InnoDB;

-- 9. PRESTAMO
CREATE TABLE PRESTAMO (
    id_prestamo INT AUTO_INCREMENT PRIMARY KEY,
    id_empleado INT NOT NULL,
    tipo ENUM('anticipo', 'prestamo') NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    fecha DATE NOT NULL,
    abono_semanal DECIMAL(10,2) NOT NULL,
    saldo_pendiente DECIMAL(10,2) NOT NULL,
    estatus ENUM('activo', 'liquidado') NOT NULL DEFAULT 'activo',
    CONSTRAINT fk_prestamo_empleado FOREIGN KEY (id_empleado) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT,
    CONSTRAINT chk_prestamo_montos CHECK (monto > 0 AND abono_semanal > 0 AND saldo_pendiente >= 0 AND saldo_pendiente <= monto)
) ENGINE=InnoDB;

-- 10. ABONO_PRESTAMO
CREATE TABLE ABONO_PRESTAMO (
    id_abono INT AUTO_INCREMENT PRIMARY KEY,
    id_prestamo INT NOT NULL,
    id_nomina INT NOT NULL,
    monto_abonado DECIMAL(10,2) NOT NULL,
    fecha DATE NOT NULL,
    CONSTRAINT fk_abono_prestamo FOREIGN KEY (id_prestamo) REFERENCES PRESTAMO(id_prestamo) ON DELETE RESTRICT,
    CONSTRAINT fk_abono_nomina FOREIGN KEY (id_nomina) REFERENCES NOMINA(id_nomina) ON DELETE RESTRICT,
    CONSTRAINT uq_abono_prestamo_nomina UNIQUE (id_prestamo, id_nomina),
    CONSTRAINT chk_abono_monto CHECK (monto_abonado > 0)
) ENGINE=InnoDB;

-- 11. PAGO_EMPLEADO
CREATE TABLE PAGO_EMPLEADO (
    id_pago_empleado INT AUTO_INCREMENT PRIMARY KEY,
    id_nomina INT NOT NULL,
    id_empleado INT NOT NULL,
    total_bruto DECIMAL(10,2) NOT NULL,
    total_descuentos DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    total_neto DECIMAL(10,2) NOT NULL,
    estatus ENUM('pendiente', 'entregado') NOT NULL DEFAULT 'pendiente',
    fecha_pago DATE NULL,
    id_nomina_pago INT NULL,
    CONSTRAINT fk_pago_nomina FOREIGN KEY (id_nomina) REFERENCES NOMINA(id_nomina) ON DELETE RESTRICT,
    CONSTRAINT fk_pago_empleado FOREIGN KEY (id_empleado) REFERENCES EMPLEADO(id_empleado) ON DELETE RESTRICT,
    CONSTRAINT fk_pago_nomina_entrega FOREIGN KEY (id_nomina_pago) REFERENCES NOMINA(id_nomina) ON DELETE RESTRICT,
    CONSTRAINT uq_pago_nomina_empleado UNIQUE (id_nomina, id_empleado),
    CONSTRAINT chk_pago_neto CHECK (total_neto >= 0 AND total_descuentos >= 0 AND total_bruto >= 0)
) ENGINE=InnoDB;