USE constructora_nominas;

-- PUNTO 16: OPERACIONES DML BÁSICAS CON VALIDACIONES Y FILTROS

-- 16.1 INSERT: Registrar un nuevo cliente
INSERT INTO CLIENTE (nombre, domicilio) VALUES ('Grupo Constructor del Pacífico', 'Av. Juárez 900, Ensenada');

-- 16.2 UPDATE: Reactivar a un empleado que estaba en baja temporal (RN-03)
UPDATE EMPLEADO 
SET estatus = 'activo' 
WHERE id_empleado = 7 AND estatus = 'baja_temporal';

-- 16.3 DELETE: Eliminar un cliente sin obras asociadas (si tuviera obras, RESTRICT lo impide)
DELETE FROM CLIENTE 
WHERE id_cliente = 3 
  AND NOT EXISTS (SELECT 1 FROM OBRA WHERE id_cliente = 3);

-- 16.4 SELECT con filtrado, ordenamiento y JOIN multitabla
SELECT E.id_empleado, CONCAT(E.nombre, ' ', E.apellido_paterno) AS trabajador, 
       P.nombre AS puesto, E.sueldo_diario, E.tipo_contratacion
FROM EMPLEADO E
INNER JOIN PUESTO P ON E.id_puesto = P.id_puesto
WHERE E.estatus = 'activo'
ORDER BY E.sueldo_diario DESC, E.apellido_paterno ASC;


-- PUNTO 17: DIEZ CONSULTAS SQL DOCUMENTADAS

-- [C1 - JOIN] Desglose de asistencias por trabajador y obra en la semana 1
SELECT A.fecha, CONCAT(E.nombre, ' ', E.apellido_paterno) AS empleado,
       COALESCE(O.nombre, 'FALTA INJUSTIFICADA / SIN OBRA') AS obra,
       A.asistio, A.horas_extra
FROM ASISTENCIA A
INNER JOIN EMPLEADO E ON A.id_empleado = E.id_empleado
LEFT JOIN OBRA O ON A.id_obra = O.id_obra
WHERE A.fecha BETWEEN '2026-11-09' AND '2026-11-14'
ORDER BY E.id_empleado, A.fecha;

-- [C2 - JOIN] Obras activas con sus responsables (Ingeniero y Supervisor) y Cliente
SELECT O.nombre AS obra, C.nombre AS cliente,
       CONCAT(R.nombre, ' ', R.apellido_paterno) AS responsable,
       CONCAT(S.nombre, ' ', S.apellido_paterno) AS supervisor,
       O.presupuesto_mano_obra
FROM OBRA O
INNER JOIN CLIENTE C ON O.id_cliente = C.id_cliente
INNER JOIN EMPLEADO R ON O.id_responsable = R.id_empleado
INNER JOIN EMPLEADO S ON O.id_supervisor = S.id_empleado
WHERE O.estatus = 'activa';

-- [C3 - JOIN] Historial de pagos netos por nómina con datos del empleado y semana en que se liquidó
SELECT N.id_nomina, N.fecha_inicio, N.fecha_fin,
       CONCAT(E.nombre, ' ', E.apellido_paterno) AS empleado,
       PE.total_bruto, PE.total_descuentos, PE.total_neto, PE.estatus, PE.fecha_pago
FROM PAGO_EMPLEADO PE
INNER JOIN NOMINA N ON PE.id_nomina = N.id_nomina
INNER JOIN EMPLEADO E ON PE.id_empleado = E.id_empleado
ORDER BY N.fecha_inicio DESC, E.apellido_paterno;

-- [C4 - SUBCONSULTA NOT EXISTS] Empleados activos que no registraron ninguna asistencia en la semana 1
SELECT E.id_empleado, CONCAT(E.nombre, ' ', E.apellido_paterno) AS empleado, P.nombre AS puesto
FROM EMPLEADO E
INNER JOIN PUESTO P ON E.id_puesto = P.id_puesto
WHERE E.estatus = 'activo'
  AND NOT EXISTS (
      SELECT 1 FROM ASISTENCIA A 
      WHERE A.id_empleado = E.id_empleado 
        AND A.fecha BETWEEN '2026-11-09' AND '2026-11-14'
  );

-- [C5 - SUBCONSULTA EXISTS] Empleados elegibles a bono en obras terminadas a tiempo (al menos 1 asistencia)
SELECT DISTINCT E.id_empleado, CONCAT(E.nombre, ' ', E.apellido_paterno) AS empleado, O.nombre AS obra_terminada
FROM EMPLEADO E
CROSS JOIN OBRA O
WHERE O.estatus = 'terminada'
  AND O.fecha_fin_real <= O.fecha_fin_programada
  AND EXISTS (
      SELECT 1 FROM ASISTENCIA A 
      WHERE A.id_empleado = E.id_empleado AND A.id_obra = O.id_obra AND A.asistio = 1
  );

-- [C6 - AGREGACIÓN / GROUP BY / HAVING] Obras cuyo gasto acumulado supera el 1% de su presupuesto
SELECT O.id_obra, O.nombre AS obra, O.presupuesto_mano_obra,
       COALESCE(SUM(CASE WHEN N.id_nomina IS NOT NULL THEN DN.costo_bruto END), 0) AS gasto_autorizado,
       (O.presupuesto_mano_obra - COALESCE(SUM(CASE WHEN N.id_nomina IS NOT NULL THEN DN.costo_bruto END), 0)) AS saldo_restante
FROM OBRA O
LEFT JOIN DETALLE_NOMINA DN ON O.id_obra = DN.id_obra
-- Solo se cuentan los detalles de nóminas autorizadas o pagadas; los borradores quedan con N = NULL
LEFT JOIN NOMINA N ON DN.id_nomina = N.id_nomina AND N.estatus IN ('autorizada', 'pagada')
GROUP BY O.id_obra, O.nombre, O.presupuesto_mano_obra
HAVING gasto_autorizado > 0;

-- [C7 - AGREGACIÓN / GROUP BY / HAVING] Empleados con más de 3 horas extra acumuladas en nóminas autorizadas
SELECT E.id_empleado, CONCAT(E.nombre, ' ', E.apellido_paterno) AS empleado,
       SUM(DN.dias_trabajados) AS total_dias,
       SUM(DN.horas_extra) AS total_horas_extra,
       SUM(DN.costo_bruto) AS ingresos_brutos_totales
FROM EMPLEADO E
INNER JOIN DETALLE_NOMINA DN ON E.id_empleado = DN.id_empleado
INNER JOIN NOMINA N ON DN.id_nomina = N.id_nomina
WHERE N.estatus IN ('autorizada', 'pagada')
GROUP BY E.id_empleado, E.nombre, E.apellido_paterno
HAVING SUM(DN.horas_extra) > 3;

-- [C8 - AGREGACIÓN / GROUP BY] Resumen de préstamos otorgados y saldo recuperado por tipo
SELECT tipo, COUNT(*) AS cantidad_creditos,
       SUM(monto) AS capital_otorgado,
       SUM(monto - saldo_pendiente) AS capital_recuperado,
       SUM(saldo_pendiente) AS cartera_pendiente
FROM PRESTAMO
GROUP BY tipo;

-- [C9 - COMPLEJA / SUBCONSULTA ESCALAR] Efectivo requerido para la semana actual (Nómina nueva + Pagos pendientes anteriores)
SELECT 
    COALESCE((SELECT SUM(total_neto) FROM PAGO_EMPLEADO WHERE id_nomina = 1 AND estatus = 'pendiente'), 0) AS adeudos_anteriores,
    COALESCE((SELECT SUM(costo_bruto) FROM DETALLE_NOMINA WHERE id_nomina = 2), 0) AS bruto_semana_actual,
    COALESCE((SELECT SUM(total_neto) FROM PAGO_EMPLEADO WHERE id_nomina = 1 AND estatus = 'pendiente'), 0) +
    COALESCE((SELECT SUM(costo_bruto) FROM DETALLE_NOMINA WHERE id_nomina = 2), 0) AS efectivo_total_requerido;

-- [C10 - COMPLEJA / CTE] Auditoría de tarifas: comparar sueldo histórico aplicado vs sueldo actual del empleado
WITH TarifasUsadas AS (
    SELECT DN.id_nomina, DN.id_empleado, O.nombre AS obra,
           DN.sueldo_diario_aplicado, E.sueldo_diario AS sueldo_actual
    FROM DETALLE_NOMINA DN
    INNER JOIN EMPLEADO E ON DN.id_empleado = E.id_empleado
    INNER JOIN OBRA O ON DN.id_obra = O.id_obra
)
SELECT *, (sueldo_actual - sueldo_diario_aplicado) AS diferencia_tarifa
FROM TarifasUsadas;


-- PUNTO 18: VISTAS FUNCIONALES Y TRANSACCIONES (COMMIT / ROLLBACK)

-- Vista 1: Estado financiero y alertas de saldo por obra (RN-12)
CREATE OR REPLACE VIEW vw_estado_presupuesto_obras AS
SELECT 
    O.id_obra,
    O.nombre AS obra,
    C.nombre AS cliente,
    O.estatus,
    O.presupuesto_mano_obra,
    COALESCE(SUM(CASE WHEN N.estatus IN ('autorizada', 'pagada') THEN DN.costo_bruto ELSE 0 END), 0) AS gasto_autorizado,
    (O.presupuesto_mano_obra - COALESCE(SUM(CASE WHEN N.estatus IN ('autorizada', 'pagada') THEN DN.costo_bruto ELSE 0 END), 0)) AS saldo_disponible,
    O.limite_alerta_saldo,
    CASE 
        WHEN (O.presupuesto_mano_obra - COALESCE(SUM(CASE WHEN N.estatus IN ('autorizada', 'pagada') THEN DN.costo_bruto ELSE 0 END), 0)) <= O.limite_alerta_saldo 
        THEN 'ALERTA: SALDO BAJO'
        ELSE 'OK'
    END AS estado_presupuesto
FROM OBRA O
INNER JOIN CLIENTE C ON O.id_cliente = C.id_cliente
LEFT JOIN DETALLE_NOMINA DN ON O.id_obra = DN.id_obra
LEFT JOIN NOMINA N ON DN.id_nomina = N.id_nomina
GROUP BY O.id_obra, O.nombre, C.nombre, O.estatus, O.presupuesto_mano_obra, O.limite_alerta_saldo;

-- Vista 2: Concentrado de pagos pendientes de entrega (RN-13)
CREATE OR REPLACE VIEW vw_pagos_pendientes_entrega AS
SELECT 
    PE.id_pago_empleado,
    N.id_nomina AS semana_origen,
    N.fecha_inicio,
    N.fecha_fin,
    E.id_empleado,
    CONCAT(E.nombre, ' ', E.apellido_paterno, ' ', COALESCE(E.apellido_materno, '')) AS trabajador,
    PE.total_neto AS monto_a_entregar
FROM PAGO_EMPLEADO PE
INNER JOIN NOMINA N ON PE.id_nomina = N.id_nomina
INNER JOIN EMPLEADO E ON PE.id_empleado = E.id_empleado
WHERE PE.estatus = 'pendiente';

-- Demostración de consulta de vistas
SELECT * FROM vw_estado_presupuesto_obras;
SELECT * FROM vw_pagos_pendientes_entrega;

-- Transacción 1: Caso con COMMIT (Entrega de pago pendiente al empleado 5 en la semana 2)
START TRANSACTION;
    UPDATE PAGO_EMPLEADO 
    SET estatus = 'entregado', fecha_pago = '2026-11-21', id_nomina_pago = 2 
    WHERE id_pago_empleado = 3 AND estatus = 'pendiente';
COMMIT;

-- Transacción 2: Caso controlado con ROLLBACK (Simulación de error al registrar efectivo negativo o inconsistente)
START TRANSACTION;
    UPDATE NOMINA SET efectivo_recibido = 50000.00 WHERE id_nomina = 2;
    -- Se detecta error operativo antes de confirmar
ROLLBACK;