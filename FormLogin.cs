#nullable disable
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormLogin : Form
    {
        private TextBox txtUser, txtPass;
        private Button btnLogin, btnRegister, btnRecuperar, btnCerrar;
        private PictureBox picTogglePass;
        private LinkLabel lblQrHint;
        private Panel panelIzquierdo, panelDerecho;

        // Para poder arrastrar la ventana sin bordes
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        public FormLogin() { ConfigurarDiseño(); }

        private void ConfigurarDiseño()
        {
            this.Text = "Acceso - Escuela Antonio Pinto Salinas";
            this.Size = new Size(700, 400); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; 
            this.BackColor = Color.White;

            // --- PANEL IZQUIERDO (Branding) ---
            // Le damos tamaño exacto de 250px de ancho
            panelIzquierdo = new Panel() { Location = new Point(0, 0), Size = new Size(250, 400), BackColor = Color.FromArgb(15, 23, 42) }; 
            panelIzquierdo.MouseDown += Panel_MouseDown; 
            
            Label lblMarca = new Label() { Text = "Sistema\nEscolar\nPinto Salinas", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(20, 120), AutoSize = true };
            lblMarca.MouseDown += Panel_MouseDown;
            panelIzquierdo.Controls.Add(lblMarca);
            this.Controls.Add(panelIzquierdo);

            // --- PANEL DERECHO (Login) ---
            // Forzamos a que empiece en el pixel 250 (justo donde termina el azul)
            panelDerecho = new Panel() { Location = new Point(250, 0), Size = new Size(450, 400), BackColor = Color.White };
            panelDerecho.MouseDown += Panel_MouseDown;

            // Botón de cerrar (X)
            btnCerrar = new Button() { Text = "X", Size = new Size(40, 40), Location = new Point(410, 0), FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Font = new Font("Arial", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => Application.Exit();
            btnCerrar.MouseEnter += (s, e) => { btnCerrar.BackColor = Color.Red; btnCerrar.ForeColor = Color.White; };
            btnCerrar.MouseLeave += (s, e) => { btnCerrar.BackColor = Color.White; btnCerrar.ForeColor = Color.Gray; };
            panelDerecho.Controls.Add(btnCerrar);

            // Título
            Label lblTitulo = new Label() { Text = "INICIAR SESIÓN", Location = new Point(75, 50), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), AutoSize = true };
            panelDerecho.Controls.Add(lblTitulo);

            // Usuario
            panelDerecho.Controls.Add(new Label() { Text = "USUARIO", Location = new Point(75, 110), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray });
            txtUser = new TextBox() { Location = new Point(75, 135), Width = 300, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.None };
            Panel lineUser = new Panel() { Location = new Point(75, 160), Width = 300, Height = 2, BackColor = Color.LightGray };
            txtUser.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { new FormQr(txtUser.Text.Trim()).ShowDialog(); } };
            panelDerecho.Controls.Add(txtUser);
            panelDerecho.Controls.Add(lineUser);

            // Contraseña
            panelDerecho.Controls.Add(new Label() { Text = "CONTRASEÑA", Location = new Point(75, 180), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray });
            txtPass = new TextBox() { Location = new Point(75, 205), Width = 270, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.None, PasswordChar = '●' };
            Panel linePass = new Panel() { Location = new Point(75, 230), Width = 300, Height = 2, BackColor = Color.LightGray };
            panelDerecho.Controls.Add(txtPass);
            panelDerecho.Controls.Add(linePass);

            // Mostrar contraseña
            picTogglePass = new PictureBox() { Location = new Point(350, 205), Size = new Size(20, 20), Cursor = Cursors.Hand, SizeMode = PictureBoxSizeMode.StretchImage };
            try { picTogglePass.Image = SystemIcons.Information.ToBitmap(); } catch { }
            picTogglePass.Click += (s, e) => { txtPass.PasswordChar = txtPass.PasswordChar == '\0' ? '●' : '\0'; };
            panelDerecho.Controls.Add(picTogglePass);

            // Botón Ingresar
            btnLogin = new Button() { Text = "INGRESAR", Location = new Point(75, 260), Width = 300, Height = 45, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            panelDerecho.Controls.Add(btnLogin);

            // Links inferiores
            lblQrHint = new LinkLabel() { Text = "Consulta tus notas por QR", Location = new Point(75, 320), LinkColor = Color.FromArgb(37, 99, 235), Font = new Font("Segoe UI", 9), AutoSize = true };
            lblQrHint.LinkClicked += (s, e) => { new FormQr().ShowDialog(); };
            panelDerecho.Controls.Add(lblQrHint);

            btnRegister = new Button() { Text = "¿No tienes cuenta? Regístrate", Location = new Point(230, 315), Width = 180, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleLeft };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += (s, e) => { new FormRegistro().Show(); this.Hide(); };
            panelDerecho.Controls.Add(btnRegister);

            btnRecuperar = new Button() { Text = "Recuperar contraseña", Location = new Point(70, 350), Width = 150, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleLeft };
            btnRecuperar.FlatAppearance.BorderSize = 0;
            btnRecuperar.Click += (s, e) => { new FormRecuperar().ShowDialog(); };
            panelDerecho.Controls.Add(btnRecuperar);

            this.Controls.Add(panelDerecho);
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try {
                // Las migraciones de esquema se deben realizar con scripts SQL (no en runtime)
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        string q = "SELECT u.id, u.password, u.security_q, u.lockout_until, u.is_role_approved, r.nombre AS rol FROM usuarios u JOIN roles r ON u.role_id = r.id WHERE u.usuario=@u LIMIT 1";
                        MySqlCommand cmd = new MySqlCommand(q, conn, tx);
                        cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());

                        int userId = 0; string rol = null; string stored = null; string secQ = null; DateTime? lockoutUntil = null; bool isApproved = true;
                        using (MySqlDataReader r = cmd.ExecuteReader()) {
                            if (!r.Read()) {
                                // usuario no encontrado
                                tx.Rollback();
                                MessageBox.Show("Datos incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            userId = Convert.ToInt32(r["id"]);
                            rol = r["rol"].ToString();
                            stored = r["password"].ToString();
                            secQ = r["security_q"] == DBNull.Value ? null : r["security_q"].ToString();
                            try { isApproved = r["is_role_approved"] == DBNull.Value ? true : Convert.ToInt32(r["is_role_approved"]) == 1; } catch { isApproved = true; }
                            try { if (r["lockout_until"] != DBNull.Value) lockoutUntil = Convert.ToDateTime(r["lockout_until"]); } catch { }
                        }

                        // si está bloqueada
                        if (lockoutUntil.HasValue && lockoutUntil.Value > DateTime.Now) {
                            tx.Rollback();
                            MessageBox.Show($"Cuenta bloqueada hasta {lockoutUntil.Value}.", "Bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            try { AuditHelper.Log("Sesion", "LoginBlocked", $"usuario={txtUser.Text.Trim()}; until={lockoutUntil}"); } catch {}
                            return;
                        }

                        bool needsUpgrade;
                        bool ok = Utils.VerifyPassword(stored, txtPass.Text.Trim(), out needsUpgrade);
                        if (!ok) {
                            // incrementar fallos y posiblemente bloquear
                            try {
                                using var upFail = new MySqlCommand("UPDATE usuarios SET failed_login_attempts = IFNULL(failed_login_attempts,0) + 1 WHERE id=@id", conn, tx);
                                upFail.Parameters.AddWithValue("@id", userId);
                                upFail.ExecuteNonQuery();
                                using var getFail = new MySqlCommand("SELECT failed_login_attempts FROM usuarios WHERE id=@id", conn, tx);
                                getFail.Parameters.AddWithValue("@id", userId);
                                var fa = Convert.ToInt32(getFail.ExecuteScalar());
                                int threshold = 5; int lockMinutes = 15;
                                if (fa >= threshold) {
                                    using var setLock = new MySqlCommand("UPDATE usuarios SET lockout_until=@lu, failed_login_attempts=0 WHERE id=@id", conn, tx);
                                    setLock.Parameters.AddWithValue("@lu", DateTime.Now.AddMinutes(lockMinutes));
                                    setLock.Parameters.AddWithValue("@id", userId);
                                    setLock.ExecuteNonQuery();
                                    tx.Commit();
                                    try { AuditHelper.Log("Sesion", "AccountLocked", $"usuario={txtUser.Text.Trim()}; threshold={threshold}"); } catch {}
                                    MessageBox.Show($"Cuenta bloqueada temporalmente por demasiados intentos. Intenta de nuevo en {lockMinutes} minutos.", "Bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                // commit del incremento de fallos
                                tx.Commit();
                            } catch {
                                try { tx.Rollback(); } catch { }
                            }
                            try { AuditHelper.Log("Sesion", "LoginFail", $"usuario={txtUser.Text.Trim()}"); } catch {}
                            MessageBox.Show("Datos incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Si contraseña legacy SHA256, actualizar a PBKDF2
                        if (needsUpgrade) {
                            string newHash = Utils.CreatePasswordHash(txtPass.Text.Trim());
                            try {
                                using var up = new MySqlCommand("UPDATE usuarios SET password=@p WHERE id=@id", conn, tx);
                                up.Parameters.AddWithValue("@p", newHash);
                                up.Parameters.AddWithValue("@id", userId);
                                up.ExecuteNonQuery();
                            } catch { /* no bloquear login si la actualización falla */ }
                        }

                        // Si el rol requiere serial (Docente/Admin), verificar que la cuenta esté aprobada y luego el serial
                        if (rol == "Docente" || rol == "Admin") {
                            if (!isApproved) {
                                tx.Rollback();
                                MessageBox.Show("Cuenta pendiente de aprobación por un administrador. Contacte al soporte.", "Pendiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            string storedSerialHash = null;
                            try {
                                using var qSerial = new MySqlCommand("SELECT serial_hash FROM seriales WHERE usuario_id=@uid LIMIT 1", conn, tx);
                                qSerial.Parameters.AddWithValue("@uid", userId);
                                var sh = qSerial.ExecuteScalar();
                                if (sh == null) {
                                    tx.Rollback();
                                    MessageBox.Show("No se ha registrado un serial para esta cuenta. Contacte al administrador.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                storedSerialHash = sh.ToString();
                            } catch {
                                // si falla la consulta, no permitir login
                                tx.Rollback();
                                MessageBox.Show("Error verificando serial. Intente más tarde.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            using (FormSerialPrompt prompt = new FormSerialPrompt()) {
                                var dr = prompt.ShowDialog();
                                if (dr != DialogResult.OK) { tx.Rollback(); return; }
                                string provided = prompt.Serial;
                                if (!SerialHelper.VerifyHashedSerial(storedSerialHash, provided)) {
                                    // contar como intento fallido
                                    try {
                                        using var upFail = new MySqlCommand("UPDATE usuarios SET failed_login_attempts = IFNULL(failed_login_attempts,0) + 1 WHERE id=@id", conn, tx);
                                        upFail.Parameters.AddWithValue("@id", userId);
                                        upFail.ExecuteNonQuery();
                                        using var getFail = new MySqlCommand("SELECT failed_login_attempts FROM usuarios WHERE id=@id", conn, tx);
                                        getFail.Parameters.AddWithValue("@id", userId);
                                        var fa = Convert.ToInt32(getFail.ExecuteScalar());
                                        int threshold = 5; int lockMinutes = 15;
                                        if (fa >= threshold) {
                                            using var setLock = new MySqlCommand("UPDATE usuarios SET lockout_until=@lu, failed_login_attempts=0 WHERE id=@id", conn, tx);
                                            setLock.Parameters.AddWithValue("@lu", DateTime.Now.AddMinutes(lockMinutes));
                                            setLock.Parameters.AddWithValue("@id", userId);
                                            setLock.ExecuteNonQuery();
                                            tx.Commit();
                                            try { AuditHelper.Log("Sesion", "AccountLocked", $"usuario={txtUser.Text.Trim()}; threshold={threshold}"); } catch {}
                                            MessageBox.Show($"Cuenta bloqueada temporalmente por demasiados intentos. Intenta de nuevo en {lockMinutes} minutos.", "Bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                        tx.Commit();
                                    } catch {
                                        try { tx.Rollback(); } catch { }
                                    }
                                    try { AuditHelper.Log("Sesion", "LoginFail_Serial", $"usuario={txtUser.Text.Trim()}"); } catch {}
                                    MessageBox.Show("Serial incorrecto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }
                        }

                        // reset failed attempts y lockout en login exitoso
                        try {
                            using var reset = new MySqlCommand("UPDATE usuarios SET failed_login_attempts=0, lockout_until=NULL WHERE id=@id", conn, tx);
                            reset.Parameters.AddWithValue("@id", userId);
                            reset.ExecuteNonQuery();
                        } catch { }

                        tx.Commit();

                        if (string.IsNullOrWhiteSpace(secQ)) {
                            MessageBox.Show("Debes registrar una pregunta de seguridad antes de continuar.");
                            this.Hide();
                            using (FormSetSecurity sfs = new FormSetSecurity(txtUser.Text)) {
                                sfs.ShowDialog();
                            }
                            this.Show();
                            return;
                        }

                        // establecer contexto de sesión y auditar login exitoso
                        SessionManager.UserId = userId;
                        SessionManager.CurrentUser = txtUser.Text.Trim();
                        SessionManager.Role = rol;
                        try { AuditHelper.Log("Sesion", "LoginSuccess", $"usuario={SessionManager.CurrentUser}; userId={userId}; rol={rol}"); } catch {}

                        this.Hide();
                        FormMenuPrincipal menu = new FormMenuPrincipal(rol);
                        menu.FormClosed += (s, args) => Application.Exit();
                        menu.Show();
                    } catch (Exception exTx) {
                        try { tx.Rollback(); } catch { }
                        MessageBox.Show("Error interno durante login: " + exTx.Message);
                        return;
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Error conexión: " + ex.Message); }
        }

        private void EnsureUsuarioColumns()
        {
            // NO-OP: no se permiten DDL en tiempo de ejecución; aplique migrations con el script SQL
        }
    }
}