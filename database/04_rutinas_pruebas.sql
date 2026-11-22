USE constructora_nominas;
DELIMITER //

-- TRIGGERS (PUNTO 19)

CREATE TRIGGER trg_validar_asistencia_before_insert
BEFORE INSERT ON ASISTENCIA
FOR EACH ROW
BEGIN
    DECLARE v_emp_estatus VARCHAR(20);
    DECLARE v_obra_estatus VARCHAR(20);
    DECLARE v_fecha_inicio DATE;
    DECLARE v_fecha_fin DATE;

    SELECT estatus INTO v_emp_estatus FROM EMPLEADO WHERE id_empleado = NEW.id_empleado;
    IF v_emp_estatus <> 'activo' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error RN-06: Solo empleados activos pueden registrar asistencia.';
    END IF;

    IF NEW.asistio = 1 THEN
        SELECT estatus, fecha_inicio, fecha_fin_programada 
        INTO v_obra_estatus, v_fecha_inicio, v_fecha_fin
        FROM OBRA WHERE id_obra = NEW.id_obra;

        IF v_obra_estatus <> 'activa' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error RN-06: La obra seleccionada no está activa.';
        END IF;

        IF NEW.fecha < v_fecha_inicio OR NEW.fecha > v_fecha_fin THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error RN-06: La fecha de asistencia está fuera del periodo programado de la obra.';
        END IF;
    END IF;
END//

CREATE TRIGGER trg_bloquear_edicion_nomina_autorizada
BEFORE UPDATE ON DETALLE_NOMINA
FOR EACH ROW
BEGIN
    DECLARE v_nomina_estatus VARCHAR(20);
    SELECT estatus INTO v_nomina_estatus FROM NOMINA WHERE id_nomina = OLD.id_nomina;

    IF v_nomina_estatus <> 'borrador' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error RN-09: No se puede modificar el detalle de una nómina ya autorizada o pagada.';
    END IF;
END//

-- FUNCIÓN ALMACENADA (PUNTO 20)

CREATE FUNCTION fn_saldo_disponible_obra(p_id_obra INT) 
RETURNS DECIMAL(12,2)
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_presupuesto DECIMAL(12,2) DEFAULT 0.00;
    DECLARE v_gastado DECIMAL(12,2) DEFAULT 0.00;

    SELECT presupuesto_mano_obra INTO v_presupuesto 
    FROM OBRA WHERE id_obra = p_id_obra;

    SELECT COALESCE(SUM(DN.costo_bruto), 0.00) INTO v_gastado
    FROM DETALLE_NOMINA DN
    INNER JOIN NOMINA N ON DN.id_nomina = N.id_nomina
    WHERE DN.id_obra = p_id_obra AND N.estatus IN ('autorizada', 'pagada');

    RETURN (v_presupuesto - v_gastado);
END//

-- PROCEDIMIENTOS ALMACENADOS CON MANEJO DE ERRORES (PUNTO 20)

-- Procedimiento 1: Registrar préstamo validando que no exista otro activo (RN-14)
CREATE PROCEDURE sp_registrar_prestamo(
    IN p_id_empleado INT,
    IN p_tipo ENUM('anticipo', 'prestamo'),
    IN p_monto DECIMAL(10,2),
    IN p_abono_semanal DECIMAL(10,2),
    IN p_fecha DATE
)
BEGIN
    DECLARE v_activos INT DEFAULT 0;
    DECLARE v_estatus_emp VARCHAR(20);

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;
        SELECT estatus INTO v_estatus_emp FROM EMPLEADO WHERE id_empleado = p_id_empleado;
        IF v_estatus_emp IS NULL OR v_estatus_emp <> 'activo' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error: El empleado no existe o no está activo.';
        END IF;

        SELECT COUNT(*) INTO v_activos 
        FROM PRESTAMO WHERE id_empleado = p_id_empleado AND estatus = 'activo';

        IF v_activos > 0 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error RN-14: El empleado ya cuenta con un préstamo o anticipo activo.';
        END IF;

        IF p_abono_semanal > p_monto OR p_monto <= 0 OR p_abono_semanal <= 0 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error: Montos de préstamo o abono semanal inválidos.';
        END IF;

        INSERT INTO PRESTAMO (id_empleado, tipo, monto, fecha, abono_semanal, saldo_pendiente, estatus)
        VALUES (p_id_empleado, p_tipo, p_monto, p_fecha, p_abono_semanal, p_monto, 'activo');
    COMMIT;
END//

-- Procedimiento 2: Autorizar nómina semanal (Consolida pagos, aplica abonos de préstamo y actualiza saldos)
CREATE PROCEDURE sp_autorizar_nomina(
    IN p_id_nomina INT,
    IN p_id_usuario INT,
    IN p_efectivo_recibido DECIMAL(12,2)
)
BEGIN
    DECLARE v_estatus_nomina VARCHAR(20);
    DECLARE v_fin INT DEFAULT 0;
    DECLARE v_id_emp INT;
    DECLARE v_bruto DECIMAL(10,2);
    DECLARE v_id_prestamo INT;
    DECLARE v_abono DECIMAL(10,2);
    DECLARE v_saldo_prestamo DECIMAL(10,2);
    DECLARE v_descuento_real DECIMAL(10,2);
    DECLARE v_fecha_fin DATE;

    DECLARE cur_empleados CURSOR FOR
        SELECT id_empleado, SUM(costo_bruto)
        FROM DETALLE_NOMINA
        WHERE id_nomina = p_id_nomina
        GROUP BY id_empleado;

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_fin = 1;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;
        SELECT estatus, fecha_fin INTO v_estatus_nomina, v_fecha_fin 
        FROM NOMINA WHERE id_nomina = p_id_nomina FOR UPDATE;

        IF v_estatus_nomina IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error: La nómina especificada no existe.';
        END IF;

        IF v_estatus_nomina <> 'borrador' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error RN-09: Solo se pueden autorizar nóminas en estatus borrador.';
        END IF;

        OPEN cur_empleados;
        bucle_empleados: LOOP
            FETCH cur_empleados INTO v_id_emp, v_bruto;
            IF v_fin = 1 THEN
                LEAVE bucle_empleados;
            END IF;

            SET v_id_prestamo = NULL;
            SET v_descuento_real = 0.00;

            -- Buscar si el trabajador tiene préstamo activo (RN-14).
            -- Va en su propio bloque: si no tiene préstamo, el NOT FOUND lo atiende este handler
            -- y NO el del cursor (si no, v_fin = 1 y se saltaban los empleados restantes).
            BEGIN
                DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_id_prestamo = NULL;

                SELECT id_prestamo, abono_semanal, saldo_pendiente
                INTO v_id_prestamo, v_abono, v_saldo_prestamo
                FROM PRESTAMO
                WHERE id_empleado = v_id_emp AND estatus = 'activo'
                LIMIT 1 FOR UPDATE;
            END;

            IF v_id_prestamo IS NOT NULL THEN
                -- El descuento no puede superar el saldo del préstamo ni el total bruto (neto nunca negativo)
                SET v_descuento_real = LEAST(v_abono, v_saldo_prestamo, v_bruto);

                IF v_descuento_real > 0 THEN
                    INSERT INTO ABONO_PRESTAMO (id_prestamo, id_nomina, monto_abonado, fecha)
                    VALUES (v_id_prestamo, p_id_nomina, v_descuento_real, v_fecha_fin);

                    UPDATE PRESTAMO 
                    -- MySQL aplica el SET de izquierda a derecha: el estatus va primero,
                    -- mientras saldo_pendiente todavía tiene el valor anterior.
                    SET estatus = IF(saldo_pendiente - v_descuento_real = 0, 'liquidado', 'activo'),
                        saldo_pendiente = saldo_pendiente - v_descuento_real
                    WHERE id_prestamo = v_id_prestamo;
                END IF;
            END IF;

            -- Generar registro consolidado PAGO_EMPLEADO
            INSERT INTO PAGO_EMPLEADO (id_nomina, id_empleado, total_bruto, total_descuentos, total_neto, estatus)
            VALUES (p_id_nomina, v_id_emp, v_bruto, v_descuento_real, (v_bruto - v_descuento_real), 'pendiente');
        END LOOP;
        CLOSE cur_empleados;

        -- Cambiar estado de la nómina a autorizada
        UPDATE NOMINA 
        SET estatus = 'autorizada',
            id_usuario_autoriza = p_id_usuario,
            fecha_autorizacion = NOW(),
            efectivo_recibido = p_efectivo_recibido
        WHERE id_nomina = p_id_nomina;
    COMMIT;
END//

DELIMITER ;

-- PUNTO 21: DEMOSTRACIÓN DE CASOS VÁLIDOS Y NO VÁLIDOS EN RUTINAS

-- Caso Válido 1: Consultar función de saldo disponible en Obra 1 (120,000 - 1,755 = 118,245)
SELECT fn_saldo_disponible_obra(1) AS saldo_obra_1;

-- Caso Válido 2: Registrar préstamo a empleado sin adeudos (Miguel Torres, id 4)
CALL sp_registrar_prestamo(4, 'anticipo', 1000.00, 500.00, '2026-11-16');

-- Caso Inválido 1 (Dispara error en SP): Intentar segundo préstamo activo a Juan Pérez (id 3)
-- CALL sp_registrar_prestamo(3, 'prestamo', 2000.00, 500.00, '2026-11-16');

-- Caso Inválido 2 (Dispara Trigger 1): Asistencia en obra terminada (Obra 3)
-- INSERT INTO ASISTENCIA (id_empleado, id_obra, fecha, asistio, horas_extra) VALUES (3, 3, '2026-11-16', TRUE, 0);

-- Caso Inválido 3 (Dispara Trigger 2): Modificar costo bruto de nómina 1 ya autorizada
-- UPDATE DETALLE_NOMINA SET costo_bruto = 9999.00 WHERE id_detalle = 1;