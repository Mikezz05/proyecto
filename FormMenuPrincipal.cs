#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace SistemaPintoSalinas
{
    public class FormMenuPrincipal : Form
    {
        string rolUsuario;
        private Panel panelMenuIzquierdo;
        private Panel panelHeader;
        private Panel panelContenedor;
        private Label lblTituloPantalla;

        public FormMenuPrincipal(string rol)
        {
            this.rolUsuario = rol;
            ConfigurarMenu();
        }

        private void ConfigurarMenu()
        {
            this.Text = "Sistema Escolar - Usuario: " + rolUsuario;
            this.Size = new Size(1100, 700);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 245, 249);
            this.MinimumSize = new Size(800, 600); 

            // --- 1. CREAR LA CUADRÍCULA MAESTRA ---
            TableLayoutPanel tablaLayout = new TableLayoutPanel();
            tablaLayout.Dock = DockStyle.Fill;
            tablaLayout.ColumnCount = 2;
            tablaLayout.RowCount = 2;
            tablaLayout.Margin = new Padding(0);
            tablaLayout.Padding = new Padding(0);

            tablaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F)); 
            tablaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  
            tablaLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));        
            tablaLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));        

            this.Controls.Add(tablaLayout);

            // --- 2. CREAR LOS PANELES Y METERLOS EN LA CUADRÍCULA ---
            panelMenuIzquierdo = new Panel() { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Color.FromArgb(15, 23, 42) };
            panelHeader = new Panel() { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Color.White };
            panelContenedor = new Panel() { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Color.FromArgb(245, 245, 249) };

            tablaLayout.Controls.Add(panelMenuIzquierdo, 0, 0);
            tablaLayout.SetRowSpan(panelMenuIzquierdo, 2); 
            tablaLayout.Controls.Add(panelHeader, 1, 0);
            tablaLayout.Controls.Add(panelContenedor, 1, 1);

            // --- 3. CONTENIDO DEL MENÚ LATERAL ---
            Panel panelLogo = new Panel() { Dock = DockStyle.Top, Height = 100 };
            Label lblBrand = new Label() { Text = "SISTEMA\nESCOLAR", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            panelLogo.Controls.Add(lblBrand);
            panelMenuIzquierdo.Controls.Add(panelLogo);

            // --- 4. CONTENIDO DEL HEADER ---
            lblTituloPantalla = new Label() { Text = "DASHBOARD PRINCIPAL", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), AutoSize = true, Location = new Point(20, 18) };
            panelHeader.Controls.Add(lblTituloPantalla);

            Label lblUser = new Label() { Text = "Rol: " + rolUsuario, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Location = new Point(500, 20) };
            panelHeader.Controls.Add(lblUser);

            Button btnCerrarSesion = new Button() { Text = "Cerrar Sesión", Width = 120, Height = 35, Location = new Point(650, 12), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(220, 53, 69), Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCerrarSesion.FlatAppearance.BorderSize = 0;
            btnCerrarSesion.Click += (s, e) => { try { AuditHelper.Log("Sesion", "Cerrar", $"user={SessionManager.CurrentUser}"); } catch {} this.Close(); new FormLogin().Show(); };
            panelHeader.Controls.Add(btnCerrarSesion);

            panelHeader.Resize += (s, e) => {
                lblUser.Location = new Point(panelHeader.Width - 300, 20);
                btnCerrarSesion.Location = new Point(panelHeader.Width - 150, 12);
            };

            // --- 5. CONTENIDO DEL CONTENEDOR (LOGO DE TAMAÑO EXACTO) ---
            // Pintamos el logo a mano para que nunca se estire y siempre mida 350x350
            // paint logo only on dashboard (when no child form is loaded)
            panelContenedor.Paint += (s, e) => {
                // if any child control is present we consider a module open
                if (panelContenedor.Controls.Count > 0) return;
                try {
                    Image logo = Image.FromFile("Logo.png");
                    int tamañoLogo = 350; // Puedes cambiar este número si lo quieres más grande o pequeño
                    int x = (panelContenedor.Width - tamañoLogo) / 2;
                    int y = (panelContenedor.Height - tamañoLogo) / 2;
                    e.Graphics.DrawImage(logo, x, y, tamañoLogo, tamañoLogo);
                } catch { }
            };
            // Repaint when resized so logo stays centered
            panelContenedor.Resize += (s, e) => panelContenedor.Invalidate();

            GenerarBotonesMenu();
        }

        private void GenerarBotonesMenu()
        {
            // manual should be available for all users
            AgregarBotonMenu("Manual de Usuario", (s, e) => AbrirManual());

            if (rolUsuario == "Admin")
            {
                AgregarBotonMenu("Centro de Reportes", (s, e) => AbrirFormulario(new FormReportesConsultas()));
                AgregarBotonMenu("Respaldo BD", (s, e) => AbrirFormulario(new FormRespaldo()));
                AgregarBotonMenu("Auditoría Usuarios", (s, e) => AbrirFormulario(new FormAuditoria()));
                AgregarBotonMenu("Aprobación de Serials", (s, e) => AbrirFormulario(new FormSerialManagement()));
                AgregarBotonMenu("Certificación", (s, e) => AbrirFormulario(new FormCertificacion()));
            }

            if (rolUsuario == "Admin" || rolUsuario == "Docente")
            {
                AgregarBotonMenu("Calificaciones", (s, e) => AbrirFormulario(new FormCalificaciones()));
            }

            if (rolUsuario == "Admin")
            {
                AgregarBotonMenu("Gestión Académica", (s, e) => AbrirFormulario(new FormAcademico()));
                AgregarBotonMenu("Inscripciones", (s, e) => AbrirFormulario(new FormEstudiantes()));
            }

            if (rolUsuario == "Estudiante")
            {
                AgregarBotonMenu("Mis Notas", (s, e) => AbrirFormulario(new FormVistaEstudiante()));
            }
        }

        private void AgregarBotonMenu(string texto, EventHandler eventoClick)
        {
            Button btn = new Button();
            btn.Text = "   " + texto;
            btn.Dock = DockStyle.Top;
            btn.Height = 50;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.LightGray;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(30, 41, 59); btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.ForeColor = Color.LightGray; };

            btn.Click += eventoClick;

            panelMenuIzquierdo.Controls.Add(btn);
            panelMenuIzquierdo.Controls.SetChildIndex(btn, 0);
        }

        private void AbrirFormulario(Form formHijo)
        {
            if (panelContenedor.Controls.Count > 0)
            {
                Control controlesAnteriores = panelContenedor.Controls[0];
                if (controlesAnteriores is Form) { ((Form)controlesAnteriores).Close(); }
            }

            panelContenedor.Controls.Clear();

            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;
            // IMPORTANTE: Hacemos que el fondo de los formularios hijos sea transparente o del mismo color
            formHijo.BackColor = Color.FromArgb(245, 245, 249);
            
            lblTituloPantalla.Text = formHijo.Text.ToUpper();

            panelContenedor.Controls.Add(formHijo);
            panelContenedor.Tag = formHijo;
            formHijo.Show();
            try { AuditHelper.Log("Menu", "AbrirModulo", $"modulo={formHijo.Text}"); } catch {}
        }

        private void AbrirManual()
        {
            try {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Manual.txt");
                if (!File.Exists(ruta)) {
                    MessageBox.Show("No se encontró el archivo de manual.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var psi = new System.Diagnostics.ProcessStartInfo(ruta) {
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                try { AuditHelper.Log("Manual", "Abrir", $"ruta={ruta}"); } catch {}
            } catch (Exception ex) {
                MessageBox.Show("No se pudo abrir el manual: " + ex.Message);
            }
        }
    }
}