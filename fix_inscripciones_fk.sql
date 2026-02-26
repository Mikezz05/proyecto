-- Fix foreign key on inscripciones to reference new estudiantes table
ALTER TABLE inscripciones DROP FOREIGN KEY IF EXISTS inscripciones_ibfk_1;
ALTER TABLE inscripciones ADD CONSTRAINT inscripciones_ibfk_1 FOREIGN KEY (estudiante_id) REFERENCES estudiantes(id) ON DELETE CASCADE;
-- Show create table for verification
SHOW CREATE TABLE inscripciones\G
