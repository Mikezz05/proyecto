using System;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public static class Seguridad
    {
        // Método global para guardar en la auditoría
        public static void RegistrarAuditoria(string usuarioLogueado, string accion, string modulo, string detalles)
        {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string q = "INSERT INTO auditoria (fecha, usuario, accion, modulo, detalles) VALUES (@f, @u, @a, @m, @d)";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@f", DateTime.Now);
                    cmd.Parameters.AddWithValue("@u", usuarioLogueado);
                    cmd.Parameters.AddWithValue("@a", accion);
                    cmd.Parameters.AddWithValue("@m", modulo);
                    cmd.Parameters.AddWithValue("@d", detalles);
                    cmd.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                // Si falla la auditoría en la base de datos, registramos localmente para forense
                try {
                    string ruta = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auditoria_error.log");
                    string msg = DateTime.Now.ToString("s") + " | Usuario: " + usuarioLogueado + " | Accion: " + accion + " | Ex: " + ex.Message + Environment.NewLine;
                    System.IO.File.AppendAllText(ruta, msg);
                } catch { /* si incluso el logging falla, no hay más remedio que continuar */ }
            }
        }
    }
}