-- Añade tabla seriales para asociar seriales seguros a usuarios
CREATE TABLE IF NOT EXISTS seriales (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  usuario_id BIGINT NOT NULL,
  serial_hash VARCHAR(512) NOT NULL,
  creado_en DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY ux_usuario_serial (usuario_id),
  INDEX ix_serial_usuario (usuario_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Nota: si la constraint FK no se pudo crear por incompatibilidades de esquema,
-- se mantiene la columna `usuario_id` con índice. La FK puede añadirse manualmente
-- con ALTER TABLE una vez que los tipos/collations coincidan.
