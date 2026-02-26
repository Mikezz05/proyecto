#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormRecuperar : Form
    {
        private TextBox txtEmailOrCI, txtNewPass, txtConfirm, txtSecAnswer;
        private Button btnBuscar, btnActualizar, btnVerifySec;
        private Label lblUsuario, lblSecQuestion;
        private PictureBox picToggleNewPass, picToggleConfirm;

        public FormRecuperar()
        {
            this.Text = "Recuperar cuenta";
            this.Size = new Size(420, 420);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label() { Text = "Ingresa tu correo o CI registrado:", Location = new Point(20, 20), AutoSize = true });
            txtEmailOrCI = new TextBox() { Location = new Point(20, 45), Width = 260 };
            this.Controls.Add(txtEmailOrCI);

            btnBuscar = new Button() { Text = "Buscar cuenta", Location = new Point(290, 43), Width = 90 };
            btnBuscar.Click += BtnBuscar_Click;
            this.Controls.Add(btnBuscar);

            lblUsuario = new Label() { Text = "", Location = new Point(20, 85), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            this.Controls.Add(lblUsuario);

            // Pregunta de seguridad
            lblSecQuestion = new Label() { Text = "", Location = new Point(20, 110), AutoSize = true, Visible = false, Font = new Font("Arial", 9, FontStyle.Bold) };
            this.Controls.Add(lblSecQuestion);
            txtSecAnswer = new TextBox() { Location = new Point(20, 135), Width = 260, Visible = false };
            this.Controls.Add(txtSecAnswer);
            btnVerifySec = new Button() { Text = "Verificar respuesta", Location = new Point(290, 133), Width = 90, Visible = false };
            btnVerifySec.Click += BtnVerifySec_Click;
            this.Controls.Add(btnVerifySec);

            // Nueva contraseña
            this.Controls.Add(new Label() { Text = "Nueva contraseña:", Location = new Point(20, 200), AutoSize = true, Visible = false });
            txtNewPass = new TextBox() { Location = new Point(20, 225), Width = 260, PasswordChar = '*', Visible = false };
            this.Controls.Add(txtNewPass);
            picToggleNewPass = new PictureBox() { Location = new Point(290, 225), Size = new Size(20, 20), Cursor = Cursors.Hand, Visible = false };
            picToggleNewPass.Image = SystemIcons.Information.ToBitmap();
            picToggleNewPass.SizeMode = PictureBoxSizeMode.StretchImage;
            picToggleNewPass.Click += (s, e) => { txtNewPass.PasswordChar = txtNewPass.PasswordChar == '\0' ? '*' : '\0'; };
            this.Controls.Add(picToggleNewPass);

            this.Controls.Add(new Label() { Text = "Confirmar contraseña:", Location = new Point(20, 260), AutoSize = true, Visible = false });
            txtConfirm = new TextBox() { Location = new Point(20, 285), Width = 260, PasswordChar = '*', Visible = false };
            this.Controls.Add(txtConfirm);
            picToggleConfirm = new PictureBox() { Location = new Point(290, 285), Size = new Size(20, 20), Cursor = Cursors.Hand, Visible = false };
            picToggleConfirm.Image = SystemIcons.Information.ToBitmap();
            picToggleConfirm.SizeMode = PictureBoxSizeMode.StretchImage;
            picToggleConfirm.Click += (s, e) => { txtConfirm.PasswordChar = txtConfirm.PasswordChar == '\0' ? '*' : '\0'; };
            this.Controls.Add(picToggleConfirm);

            btnActualizar = new Button() { Text = "Aplicar", Location = new Point(20, 320), Width = 360, Visible = false };
            btnActualizar.Click += BtnActualizar_Click;
            this.Controls.Add(btnActualizar);
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string clave = txtEmailOrCI.Text.Trim();
            if (string.IsNullOrEmpty(clave)) { MessageBox.Show("Ingrese correo o CI registrado."); return; }

            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string q = "SELECT usuario, security_q FROM usuarios WHERE email = @c OR ci = @c LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@c", clave);
                    using (MySqlDataReader r = cmd.ExecuteReader()) {
                        if (r.Read()) {
                            string usuario = r["usuario"].ToString();
                            string question = r["security_q"] == DBNull.Value ? null : r["security_q"].ToString();
                            lblUsuario.Text = "Cuenta encontrada: " + usuario;

                            if (string.IsNullOrWhiteSpace(question)) {
                                MessageBox.Show("La cuenta no tiene pregunta de seguridad registrada. Inicia sesión normalmente y registra una pregunta de seguridad en tu perfil.");
                                return;
                            }

                            lblSecQuestion.Text = "Pregunta: " + question;
                            lblSecQuestion.Visible = true;
                            txtSecAnswer.Visible = true;
                            btnVerifySec.Visible = true;
                        } else {
                            MessageBox.Show("No se encontró cuenta con esos datos.");
                        }
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Error consulta: " + ex.Message); }
        }

        private void BtnVerifySec_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSecAnswer.Text)) { MessageBox.Show("Ingrese la respuesta de seguridad."); return; }
            string usuario = lblUsuario.Text.Replace("Cuenta encontrada: ", "").Trim();
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    MySqlCommand chk = new MySqlCommand("SELECT security_a FROM usuarios WHERE usuario=@u", conn);
                    chk.Parameters.AddWithValue("@u", usuario);
                    var res = chk.ExecuteScalar();
                    if (res == null || res == DBNull.Value) { MessageBox.Show("No hay respuesta de seguridad registrada."); return; }
                    string storedHash = res.ToString();
                    string providedHash = ComputeSHA256(txtSecAnswer.Text.Trim());
                    if (storedHash != providedHash) { MessageBox.Show("Respuesta incorrecta."); return; }

                    // mostrar campos para nueva contraseña
                    txtNewPass.Visible = true;
                    txtConfirm.Visible = true;
                    picToggleNewPass.Visible = true;
                    picToggleConfirm.Visible = true;
                    btnActualizar.Visible = true;
                    MessageBox.Show("Respuesta verificada. Ahora ingresa la nueva contraseña.");
                }
            } catch (Exception ex) { MessageBox.Show("Error validando respuesta: " + ex.Message); }
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            string nueva = txtNewPass.Text.Trim();
            if (nueva.Length < 4) { MessageBox.Show("La contraseña debe tener al menos 4 caracteres."); return; }
            if (nueva != txtConfirm.Text.Trim()) { MessageBox.Show("Las contraseñas no coinciden."); return; }
            string usuario = lblUsuario.Text.Replace("Cuenta encontrada: ", "").Trim();
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string q = "UPDATE usuarios SET password = @p WHERE usuario = @u";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@p", nueva);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    int aff = cmd.ExecuteNonQuery();
                    if (aff > 0) {
                        MessageBox.Show("Contraseña actualizada. Ahora puedes iniciar sesión.");
                    } else {
                        MessageBox.Show("No se pudo actualizar la contraseña (quizá es igual a la anterior).");
                    }

                    // lectura de verificación
                    MySqlCommand chk = new MySqlCommand("SELECT password FROM usuarios WHERE usuario=@u", conn);
                    chk.Parameters.AddWithValue("@u", usuario);
                    var actual = chk.ExecuteScalar()?.ToString();
                    MessageBox.Show("Valor almacenado: '" + actual + "'");

                    if (aff > 0) this.Close();
                }
            } catch (Exception ex) { MessageBox.Show("Error actualización: " + ex.Message); }
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
