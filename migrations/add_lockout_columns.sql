-- Add columns to support account lockout
ALTER TABLE usuarios
  ADD COLUMN IF NOT EXISTS failed_login_attempts INT DEFAULT 0,
  ADD COLUMN IF NOT EXISTS lockout_until DATETIME NULL;

-- Ensure index on lockout_until for quick checks
SET @s = (SELECT IF(COUNT(*)=0,'ALTER TABLE usuarios ADD INDEX idx_usuarios_lockout_until (lockout_until);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND INDEX_NAME='idx_usuarios_lockout_until');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT 'OK' AS result;
