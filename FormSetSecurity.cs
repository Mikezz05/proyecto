#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormSetSecurity : Form
    {
        private string usuario;
        private TextBox txtQuestion, txtAnswer;
        private Button btnSave;

        public FormSetSecurity(string usuario)
        {
            this.usuario = usuario;
            this.Text = "Registrar pregunta de seguridad";
            this.Size = new Size(420, 220);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label() { Text = "Pregunta de seguridad:", Location = new Point(20, 20), AutoSize = true });
            txtQuestion = new TextBox() { Location = new Point(20, 45), Width = 360 };
            this.Controls.Add(txtQuestion);

            this.Controls.Add(new Label() { Text = "Respuesta (guárdala en un lugar seguro):", Location = new Point(20, 80), AutoSize = true });
            txtAnswer = new TextBox() { Location = new Point(20, 105), Width = 360 };
            this.Controls.Add(txtAnswer);

            btnSave = new Button() { Text = "Guardar", Location = new Point(20, 140), Width = 360 };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string q = txtQuestion.Text.Trim();
            string a = txtAnswer.Text.Trim();
            if (string.IsNullOrWhiteSpace(q) || string.IsNullOrWhiteSpace(a)) { MessageBox.Show("Ingrese pregunta y respuesta."); return; }

            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        // Asegurar columnas (DDL se ejecuta idempotente si faltan columnas)
                        MySqlCommand chkQ = new MySqlCommand("SHOW COLUMNS FROM usuarios LIKE 'security_q'", conn, tx);
                        var r1 = chkQ.ExecuteScalar();
                        if (r1 == null) {
                            new MySqlCommand("ALTER TABLE usuarios ADD COLUMN security_q VARCHAR(255) NULL", conn, tx).ExecuteNonQuery();
                        }
                        MySqlCommand chkA = new MySqlCommand("SHOW COLUMNS FROM usuarios LIKE 'security_a'", conn, tx);
                        var r2 = chkA.ExecuteScalar();
                        if (r2 == null) {
                            new MySqlCommand("ALTER TABLE usuarios ADD COLUMN security_a VARCHAR(255) NULL", conn, tx).ExecuteNonQuery();
                        }

                        string upd = "UPDATE usuarios SET security_q = @q, security_a = @a WHERE usuario = @u";
                        MySqlCommand cmd = new MySqlCommand(upd, conn, tx);
                        cmd.Parameters.AddWithValue("@q", q);
                        cmd.Parameters.AddWithValue("@a", ComputeSHA256(a));
                        cmd.Parameters.AddWithValue("@u", usuario);
                        int aff = cmd.ExecuteNonQuery();
                        if (aff > 0) {
                            tx.Commit();
                            MessageBox.Show("Pregunta de seguridad registrada.");
                            try { AuditHelper.Log("Seguridad", "SetSecurity", $"usuario={usuario}; quien={SessionManager.CurrentUser}"); } catch {}
                            this.Close();
                        } else {
                            tx.Rollback();
                            MessageBox.Show("No se pudo guardar la pregunta. Contacte al administrador.");
                        }
                    } catch (Exception exTx) {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Error guardando pregunta: " + ex.Message); }
        }

        private string ComputeSHA256(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create()) {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
