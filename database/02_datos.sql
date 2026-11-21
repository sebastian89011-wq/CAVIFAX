USE constructora_nominas;

-- Puestos
INSERT INTO PUESTO (nombre, descripcion) VALUES
('Ingeniero', 'Administrador general, responsable de obra y autorización de nómina'),
('Supervisor general', 'Corresponsable de campo y control de asistencias'),
('Albañil', 'Ejecución de obra negra, acabados e instalaciones'),
('Ayudante general', 'Apoyo operativo en obra y acarreo de materiales');

-- Empleados (1 Ingeniero, 1 Supervisor, 5 Operativos)
INSERT INTO EMPLEADO (id_puesto, nombre, apellido_paterno, apellido_materno, telefono, domicilio, curp, nss, clave_ine, documentos_completos, especialidad, tipo_contratacion, fecha_ingreso, sueldo_diario, pago_hora_extra, estatus) VALUES
(1, 'Roberto', 'Mendoza', 'Silva', '6461112233', 'Av. Reforma 450, Ensenada', 'MESR850312HBCRRL01', '12345678901', 'MNSLRB85031202H100', TRUE, NULL, 'permanente', '2020-01-15', 950.00, 150.00, 'activo'),
(2, 'Carlos', 'Ramírez', 'Gómez', '6462223344', 'Calle Sexta 120, Ensenada', 'RAGC880720HBCRML02', '12345678902', 'RMGMCR88072002H200', TRUE, NULL, 'permanente', '2021-03-10', 700.00, 110.00, 'activo'),
(3, 'Juan', 'Pérez', 'López', '6463334455', 'Col. Popular 89, Ensenada', 'PELJ900515HBCRPN03', '12345678903', 'PRLPJN90051502H300', TRUE, 'soldador', 'permanente', '2022-06-01', 500.00, 85.00, 'activo'),
(3, 'Miguel', 'Torres', 'Vega', '6464445566', 'Calle Diamante 34, Ensenada', 'TOVM921108HBCRTG04', '12345678904', 'TRVGMG92110802H400', TRUE, 'electricista', 'permanente', '2022-08-20', 520.00, 90.00, 'activo'),
(4, 'Luis', 'Hernández', 'Cruz', '6465556677', 'Fracc. Valle Dorado 210, Ensenada', 'HECL980214HBCRHR05', '12345678905', 'HRCRLS98021402H500', TRUE, NULL, 'permanente', '2024-02-01', 350.00, 60.00, 'activo'),
(4, 'Pedro', 'Sánchez', 'Ruiz', '6466667788', 'Col. Obrera 55, Ensenada', 'SARP990930HBCRSN06', '12345678906', 'SNRZPD99093002H600', FALSE, NULL, 'subcontratado', '2026-09-01', 360.00, 60.00, 'activo'),
(3, 'Jorge', 'Castro', 'Díaz', '6467778899', 'Calle Riveroll 712, Ensenada', 'CADJ870411HBCRCS07', '12345678907', 'CSDZJR87041102H700', TRUE, NULL, 'permanente', '2023-05-12', 480.00, 80.00, 'baja_temporal');

-- Usuario (Solo Ingeniero Roberto Mendoza)
INSERT INTO USUARIO (id_empleado, nombre_usuario, contrasena_hash, activo) VALUES
(1, 'rmendoza', '$2a$12$KIX9v8ExampleHashBcryptParaIngeniero123456', TRUE);

-- Clientes
INSERT INTO CLIENTE (nombre, domicilio) VALUES
('Inmobiliaria Costa Azul S.A. de C.V.', 'Blvd. Costero 1500, Zona Centro, Ensenada'),
('Desarrollos Industriales del Norte', 'Carretera Transpeninsular Km 108, Ensenada');

-- Obras (2 Activas, 1 Terminada a tiempo)
INSERT INTO OBRA (id_cliente, id_responsable, id_supervisor, nombre, ubicacion, fecha_inicio, fecha_fin_programada, fecha_fin_real, presupuesto_mano_obra, limite_alerta_saldo, bono_terminacion_por_trabajador, estatus) VALUES
(1, 1, 2, 'Residencial Punta Banda', 'Av. Delante 890, Ensenada', '2026-10-01', '2026-12-20', NULL, 120000.00, 20000.00, 1000.00, 'activa'),
(2, 1, 2, 'Nave Industrial El Sauzal', 'Parque Industrial El Sauzal Lote 4', '2026-10-15', '2027-01-30', NULL, 250000.00, 40000.00, 1500.00, 'activa'),
(1, 1, 2, 'Remodelación Locales Centro', 'Calle Primera 320, Ensenada', '2026-09-01', '2026-10-31', '2026-10-29', 45000.00, 5000.00, 800.00, 'terminada');

-- Préstamos
INSERT INTO PRESTAMO (id_empleado, tipo, monto, fecha, abono_semanal, saldo_pendiente, estatus) VALUES
(3, 'prestamo', 3000.00, '2026-11-01', 500.00, 2500.00, 'activo'),
(5, 'anticipo', 800.00, '2026-11-10', 400.00, 400.00, 'activo');

-- Asistencias (Históricas de Obra 3 + Semana 1 del 2026-11-09 al 2026-11-14)
INSERT INTO ASISTENCIA (id_empleado, id_obra, fecha, asistio, horas_extra) VALUES
-- Participación previa en Obra 3 (terminada a tiempo en octubre, elegibles a bono)
(3, 3, '2026-10-15', TRUE, 0.00),
(4, 3, '2026-10-16', TRUE, 1.00),
-- Juan Pérez (id 3): Obra 1 (Lun-Mie) y Obra 2 (Jue-Vie), falta el Sábado (id_obra NULL)
(3, 1, '2026-11-09', TRUE, 2.00),
(3, 1, '2026-11-10', TRUE, 0.00),
(3, 1, '2026-11-11', TRUE, 1.00),
(3, 2, '2026-11-12', TRUE, 2.00),
(3, 2, '2026-11-13', TRUE, 0.00),
(3, NULL, '2026-11-14', FALSE, 0.00),
-- Miguel Torres (id 4): trabaja toda la semana en Obra 2
(4, 2, '2026-11-09', TRUE, 1.00),
(4, 2, '2026-11-10', TRUE, 1.00),
(4, 2, '2026-11-11', TRUE, 0.00),
(4, 2, '2026-11-12', TRUE, 2.00),
(4, 2, '2026-11-13', TRUE, 0.00),
(4, 2, '2026-11-14', TRUE, 0.00),
-- Luis Hernández (id 5): trabaja 4 días en Obra 1
(5, 1, '2026-11-09', TRUE, 0.00),
(5, 1, '2026-11-10', TRUE, 0.00),
(5, 1, '2026-11-11', TRUE, 2.00),
(5, 1, '2026-11-12', TRUE, 0.00);

-- Nóminas (Semana 1 Autorizada y Semana 2 en Borrador)
INSERT INTO NOMINA (id_usuario_autoriza, fecha_inicio, fecha_fin, estatus, fecha_autorizacion, efectivo_recibido) VALUES
(1, '2026-11-09', '2026-11-14', 'autorizada', '2026-11-14 14:30:00', 8500.00),
(NULL, '2026-11-16', '2026-11-21', 'borrador', NULL, 0.00);

-- Detalles de Nómina (Costo bruto por obra para Semana 1 y Semana 2)
INSERT INTO DETALLE_NOMINA (id_nomina, id_empleado, id_obra, dias_trabajados, horas_extra, sueldo_diario_aplicado, pago_hora_extra_aplicado, bono, costo_bruto) VALUES
-- Semana 1 (id_nomina = 1):
-- Juan: Obra 1 (3d * 500 + 3he * 85 = 1755.00), Obra 2 (2d * 500 + 2he * 85 = 1170.00)
-- Miguel: Obra 2 (6d * 520 + 4he * 90 = 3480.00) | Luis: Obra 1 (4d * 350 + 2he * 60 = 1520.00)
(1, 3, 1, 3, 3.00, 500.00, 85.00, 0.00, 1755.00),
(1, 3, 2, 2, 2.00, 500.00, 85.00, 0.00, 1170.00),
(1, 4, 2, 6, 4.00, 520.00, 90.00, 0.00, 3480.00),
(1, 5, 1, 4, 2.00, 350.00, 60.00, 0.00, 1520.00),
-- Semana 2 en borrador (id_nomina = 2):
-- Juan: Obra 1 (5d * 500 + 2he * 85 = 2670.00) | Miguel: Obra 2 (5d * 520 + 0he * 90 = 2600.00)
(2, 3, 1, 5, 2.00, 500.00, 85.00, 0.00, 2670.00),
(2, 4, 2, 5, 0.00, 520.00, 90.00, 0.00, 2600.00);

-- Abonos de préstamo Semana 1 (Un solo descuento por empleado y nómina)
INSERT INTO ABONO_PRESTAMO (id_prestamo, id_nomina, monto_abonado, fecha) VALUES
(1, 1, 500.00, '2026-11-14'),
(2, 1, 400.00, '2026-11-14');

-- Pago Empleado Semana 1
-- Juan: Bruto 2925 - Abono 500 = 2425 (Entregado)
-- Miguel: Bruto 3480 - Abono 0 = 3480 (Entregado)
-- Luis: Bruto 1520 - Abono 400 = 1120 (Pendiente de cobro)
INSERT INTO PAGO_EMPLEADO (id_nomina, id_empleado, total_bruto, total_descuentos, total_neto, estatus, fecha_pago, id_nomina_pago) VALUES
(1, 3, 2925.00, 500.00, 2425.00, 'entregado', '2026-11-14', 1),
(1, 4, 3480.00, 0.00, 3480.00, 'entregado', '2026-11-14', 1),
(1, 5, 1520.00, 400.00, 1120.00, 'pendiente', NULL, NULL);