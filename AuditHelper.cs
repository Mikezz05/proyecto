using System;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public static class AuditHelper
    {
        // Log simple auditoría: crea módulo/acción si no existen y registra la entrada
        public static void Log(string modulo, string accion, string detalles)
        {
            try {
                string usuario = SessionManager.CurrentUser ?? "SYSTEM";
                ConexionDB db = new ConexionDB();
                using (var conn = db.GetConnection()) {
                    conn.Open();
                    // asegurar módulo
                    using var cmdMod = new MySqlCommand("INSERT INTO modulos_auditoria (nombre, descripcion) SELECT @m,@m WHERE NOT EXISTS (SELECT 1 FROM modulos_auditoria WHERE nombre=@m)", conn);
                    cmdMod.Parameters.AddWithValue("@m", modulo);
                    cmdMod.ExecuteNonQuery();
                    using var cmdGetMod = new MySqlCommand("SELECT id FROM modulos_auditoria WHERE nombre=@m LIMIT 1", conn);
                    cmdGetMod.Parameters.AddWithValue("@m", modulo);
                    int modId = Convert.ToInt32(cmdGetMod.ExecuteScalar());

                    // asegurar acción
                    using var cmdAct = new MySqlCommand("INSERT INTO acciones_auditoria (nombre, descripcion) SELECT @a,@a WHERE NOT EXISTS (SELECT 1 FROM acciones_auditoria WHERE nombre=@a)", conn);
                    cmdAct.Parameters.AddWithValue("@a", accion);
                    cmdAct.ExecuteNonQuery();
                    using var cmdGetAct = new MySqlCommand("SELECT id FROM acciones_auditoria WHERE nombre=@a LIMIT 1", conn);
                    cmdGetAct.Parameters.AddWithValue("@a", accion);
                    int actId = Convert.ToInt32(cmdGetAct.ExecuteScalar());

                    // insertar auditoría
                    using var ins = new MySqlCommand("INSERT INTO auditoria (usuario, accion_id, modulo_id, detalles, ip_origen) VALUES (@u,@act,@mod,@d,NULL)", conn);
                    ins.Parameters.AddWithValue("@u", usuario);
                    ins.Parameters.AddWithValue("@act", actId);
                    ins.Parameters.AddWithValue("@mod", modId);
                    ins.Parameters.AddWithValue("@d", detalles ?? "");
                    ins.ExecuteNonQuery();
                }
            } catch {
                // no lanzar excepciones de auditoría a la UI
            }
        }
    }
}
