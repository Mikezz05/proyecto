#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormRegistro : Form
    {
        private TextBox txtNombre, txtUsuario, txtPass, txtSerial, txtCI, txtEmail, txtSecQuestion, txtSecAnswer;
        private ComboBox cmbRol;
        private Button btnRegistrar, btnVolver;
        private Label lblSerial;

        public FormRegistro()
        {
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Registro de Nuevo Usuario";
            this.Size = new Size(450, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            int x = 50; int y = 30; int w = 280;

            Label lblT = new Label() { Text = "Crear Cuenta", Location = new Point(130, 10), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true };
            this.Controls.Add(lblT);

            this.Controls.Add(new Label() { Text = "Nombre Completo:", Location = new Point(x, y + 30) });
            txtNombre = new TextBox() { Location = new Point(x, y + 55), Width = w };
            this.Controls.Add(txtNombre);

            this.Controls.Add(new Label() { Text = "Usuario:", Location = new Point(x, y + 90) });
            txtUsuario = new TextBox() { Location = new Point(x, y + 115), Width = w };
            this.Controls.Add(txtUsuario);

            this.Controls.Add(new Label() { Text = "CI (Cédula):", Location = new Point(x, y + 150) });
            txtCI = new TextBox() { Location = new Point(x, y + 175), Width = w };
            this.Controls.Add(txtCI);

            this.Controls.Add(new Label() { Text = "Email:", Location = new Point(x, y + 210) });
            txtEmail = new TextBox() { Location = new Point(x, y + 235), Width = w };
            this.Controls.Add(txtEmail);

            // contraseña justo después del email
            this.Controls.Add(new Label() { Text = "Contraseña:", Location = new Point(x, y + 270) });
            txtPass = new TextBox() { Location = new Point(x, y + 295), Width = w, PasswordChar = '*' };
            this.Controls.Add(txtPass);

            this.Controls.Add(new Label() { Text = "Tipo de Usuario:", Location = new Point(x, y + 330) });
            cmbRol = new ComboBox() { Location = new Point(x, y + 355), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            // Cargar roles desde la base de datos (display nombre, value id)
            try {
                ConexionDB db = new ConexionDB();
                using (var conn = db.GetConnection()) {
                    conn.Open();
                    var da = new MySql.Data.MySqlClient.MySqlDataAdapter("SELECT id,nombre FROM roles ORDER BY id", conn);
                    var dt = new System.Data.DataTable();
                    da.Fill(dt);
                    cmbRol.DataSource = dt;
                    cmbRol.DisplayMember = "nombre";
                    cmbRol.ValueMember = "id";
                }
            } catch { /* si falla, mostramos los roles por defecto */
                cmbRol.Items.Add("Estudiante");
                cmbRol.Items.Add("Docente");
                cmbRol.Items.Add("Admin");
            }
            cmbRol.SelectedIndexChanged += CmbRol_Changed;
            this.Controls.Add(cmbRol);

            // Campo Serial (se usaba para entrada manual). Ya no se requiere: el sistema generará seriales automáticamente para Docente/Admin.
            lblSerial = new Label() { Text = "(Se generará un serial único para Docentes/Admins)", Location = new Point(x, y + 390), Visible = false, ForeColor = Color.DarkGreen, Font = new Font("Arial", 9, FontStyle.Italic) };
            this.Controls.Add(lblSerial);
            txtSerial = new TextBox() { Location = new Point(x, y + 415), Width = w, Visible = false, ReadOnly = true };
            this.Controls.Add(txtSerial);

            // seguridad al final, después del serial
            this.Controls.Add(new Label() { Text = "Pregunta de seguridad (opcional):", Location = new Point(x, y + 460), AutoSize = true });
            txtSecQuestion = new TextBox() { Location = new Point(x, y + 485), Width = w };
            this.Controls.Add(txtSecQuestion);

            this.Controls.Add(new Label() { Text = "Respuesta de seguridad:", Location = new Point(x, y + 520), AutoSize = true });
            txtSecAnswer = new TextBox() { Location = new Point(x, y + 545), Width = w };
            this.Controls.Add(txtSecAnswer);

            btnRegistrar = new Button() { Text = "Registrarse", Location = new Point(x, 660), Width = w, Height = 40, BackColor = Color.LightBlue };
            btnRegistrar.Click += BtnRegistrar_Click;
            this.Controls.Add(btnRegistrar);

            btnVolver = new Button() { Text = "Volver al Login", Location = new Point(x, 710), Width = w };
            btnVolver.Click += (s, e) => { new FormLogin().Show(); this.Hide(); };
            this.Controls.Add(btnVolver);
        }

        private void CmbRol_Changed(object sender, EventArgs e)
        {
            if (cmbRol.SelectedItem == null) return;
            string seleccion = cmbRol.SelectedItem.ToString();
            
            // Informar que el serial será generado automáticamente para Docente/Admin
            bool requiereSerial = (seleccion == "Docente" || seleccion == "Admin");
            lblSerial.Visible = requiereSerial;
            txtSerial.Visible = false; // input manual deshabilitado
            if (seleccion == "Docente") lblSerial.Text = "Se generará un Serial Carnet Docente al crear la cuenta.";
            if (seleccion == "Admin") lblSerial.Text = "Se generará un Serial Administrativo al crear la cuenta.";
        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            // trim inputs to avoid leading/trailing spaces
            txtUsuario.Text = txtUsuario.Text.Trim();
            txtPass.Text = txtPass.Text.Trim();
            txtNombre.Text = txtNombre.Text.Trim();
            txtCI.Text = txtCI.Text.Trim();
            txtEmail.Text = txtEmail.Text.Trim();
            txtSecQuestion.Text = txtSecQuestion.Text.Trim();
            txtSecAnswer.Text = txtSecAnswer.Text.Trim();

            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPass.Text) || cmbRol.SelectedItem == null) {
                MessageBox.Show("Complete los datos básicos."); return;
            }

            // opcional: validar formato de email si fue ingresado
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !txtEmail.Text.Contains("@")) { MessageBox.Show("Email inválido."); return; }

            // obtener rol seleccionado (nombre y id)
            string rol = cmbRol.Text;
            int roleId = cmbRol.SelectedValue is int ? (int)cmbRol.SelectedValue : 0;

            // --- VALIDACIONES DE SEGURIDAD ---
            
            // Para Docente/Admin el serial se genera automáticamente y se asocia al usuario (se muestra una vez).

            // --- GUARDADO ---
            try {
                // Las migraciones/ALTER TABLE deben aplicarse con el script SQL (database_schema.sql)
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string q = "INSERT INTO usuarios (usuario, password, role_id, email, security_q, security_a, is_role_approved) VALUES (@u, @p, @role, @em, @sq, @sa, @approved)";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@u", txtUsuario.Text);
                    // Guardar contraseña con PBKDF2
                    string passHash = Utils.CreatePasswordHash(txtPass.Text);
                    cmd.Parameters.AddWithValue("@p", passHash);
                    cmd.Parameters.AddWithValue("@role", roleId);
                    cmd.Parameters.AddWithValue("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);
                    // seguridad: pregunta y hash de respuesta
                    string secQ = string.IsNullOrWhiteSpace(txtSecQuestion.Text) ? null : txtSecQuestion.Text.Trim();
                    string aHash = string.IsNullOrWhiteSpace(txtSecAnswer.Text) ? null : Utils.ComputeSHA256(txtSecAnswer.Text.Trim());
                    cmd.Parameters.AddWithValue("@sq", (object)secQ ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sa", (object)aHash ?? DBNull.Value);

                    // para Docente/Admin la cuenta queda PENDIENTE de aprobación por un administrador (no generar serial automático)
                    bool approved = !(rol == "Docente" || rol == "Admin");
                    cmd.Parameters.AddWithValue("@approved", approved ? 1 : 0);
                    cmd.ExecuteNonQuery();
                    long newUserId = cmd.LastInsertedId;

                    // Insertar datos en la tabla correspondiente según rol
                    // separar nombre completo en nombres/apellidos si es posible
                    string nombres = txtNombre.Text.Trim();
                    string apellidos = "";
                    var parts = nombres.Split(' ');
                    if (parts.Length > 1) { apellidos = parts[parts.Length - 1]; nombres = string.Join(' ', parts, 0, parts.Length - 1); }

                    if (rol == "Docente") {
                        using var cmd2 = new MySqlCommand("INSERT INTO docentes (usuario_id, ci, nombres, apellidos) VALUES (@uid,@ci,@n,@a)", conn);
                        cmd2.Parameters.AddWithValue("@uid", newUserId);
                        cmd2.Parameters.AddWithValue("@ci", string.IsNullOrWhiteSpace(txtCI.Text) ? (object)DBNull.Value : txtCI.Text);
                        cmd2.Parameters.AddWithValue("@n", nombres);
                        cmd2.Parameters.AddWithValue("@a", apellidos);
                        cmd2.ExecuteNonQuery();
                        // no generar serial aquí: espera aprobación administrativa
                    } else if (rol == "Admin") {
                        using var cmd2 = new MySqlCommand("INSERT INTO administrativos (usuario_id, ci, nombres, apellidos, cargo) VALUES (@uid,@ci,@n,@a,@cargo)", conn);
                        cmd2.Parameters.AddWithValue("@uid", newUserId);
                        cmd2.Parameters.AddWithValue("@ci", string.IsNullOrWhiteSpace(txtCI.Text) ? (object)DBNull.Value : txtCI.Text);
                        cmd2.Parameters.AddWithValue("@n", nombres);
                        cmd2.Parameters.AddWithValue("@a", apellidos);
                        cmd2.Parameters.AddWithValue("@cargo", "Administrador");
                        cmd2.ExecuteNonQuery();
                        // no generar serial aquí: espera aprobación administrativa
                    } else {
                        using var cmd2 = new MySqlCommand("INSERT INTO estudiantes (usuario_id, ci, nombres, apellidos) VALUES (@uid,@ci,@n,@a)", conn);
                        cmd2.Parameters.AddWithValue("@uid", newUserId);
                        cmd2.Parameters.AddWithValue("@ci", string.IsNullOrWhiteSpace(txtCI.Text) ? (object)DBNull.Value : txtCI.Text);
                        cmd2.Parameters.AddWithValue("@n", nombres);
                        cmd2.Parameters.AddWithValue("@a", apellidos);
                        cmd2.ExecuteNonQuery();
                    }

                    MessageBox.Show("¡Cuenta creada exitosamente! Inicie sesión.");
                    new FormLogin().Show();
                    this.Hide();
                }
            } catch (Exception ex) { MessageBox.Show("Error (El usuario ya existe): " + ex.Message); }
        }

        // Asegurar que la tabla usuarios tiene columnas 'ci', 'email', 'security_q', 'security_a', las crea si no existen
        private void EnsureUsuarioColumns()
        {
            // NO-OP: Las migraciones de esquema deben ejecutarse mediante el script SQL
            // database_schema.sql proporcionado en el proyecto. Esto evita que la aplicación
            // cliente ejecute DDL en tiempo de ejecución.
        }

        private string ComputeSHA256(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create()) {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // Nota: la verificación de seriales manuales ha sido reemplazada por seriales generados
        // automáticamente y asociados al `usuario_id` en la tabla `seriales` (migración add_seriales_table.sql)
    }
}