#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormAcademico : Form
    {
        private TabControl tabControl;
        private TextBox txtNombreCurso;
        private NumericUpDown numDuracion;
        private ComboBox cmbDocentes;
        private Button btnAsignar, btnEliminarCurso;
        private DataGridView dgvCursos;

        private MonthCalendar calendario;
        private ComboBox cmbTipoActividad;
        private TextBox txtDescripcionActividad;
        private Button btnGuardarEvento, btnEliminarEvento;
        private DataGridView dgvAgenda;
        private Label lblFechaSeleccionada;

        // Memoria para el logo de los Tabs
        private Bitmap logoFondoTabs;

        public FormAcademico()
        {
            // --- CARGA DE LOGO OPTIMIZADA ---
            try {
                logoFondoTabs = new Bitmap("Logo.png");
                logoFondoTabs.MakeTransparent(Color.White);
            } catch { }

            ConfigurarFormulario();
            try { 
                CargarDocentes(); 
                CargarCursos(); 
                CargarAgenda(); 
            } catch {}
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Gestión Académica - Planificación y Carga";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill; 

            TabPage tabCarga = new TabPage("Asignación de Cursos y Docentes");
            ConfigurarTabCarga(tabCarga);
            tabControl.TabPages.Add(tabCarga);

            TabPage tabCalendario = new TabPage("Calendario y Planificación");
            ConfigurarTabCalendario(tabCalendario);
            tabControl.TabPages.Add(tabCalendario);

            this.Controls.Add(tabControl);
        }

        private void AgregarMarcaDeAgua(TabPage p)
        {
            p.Paint += (s, e) => {
                if (logoFondoTabs != null) {
                    int tamañoLogo = 350; 
                    int x = (p.Width - tamañoLogo) / 2;
                    int y = (p.Height - tamañoLogo) / 2;
                    e.Graphics.DrawImage(logoFondoTabs, x, y, tamañoLogo, tamañoLogo);
                }
            };
            p.Resize += (s, e) => p.Invalidate();
        }

        private void ConfigurarTabCarga(TabPage p)
        {
            p.BackColor = Color.WhiteSmoke;
            AgregarMarcaDeAgua(p);

            Label lblTitulo = new Label() { Text = "Creación de Cursos", Location = new Point(20, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            p.Controls.Add(lblTitulo);

            p.Controls.Add(new Label() { Text = "Nombre Asignatura:", Location = new Point(20, 60), AutoSize = true, BackColor = Color.Transparent });
            txtNombreCurso = new TextBox() { Location = new Point(20, 85), Width = 250 };
            p.Controls.Add(txtNombreCurso);

            p.Controls.Add(new Label() { Text = "Duración (Meses):", Location = new Point(20, 120), AutoSize = true, BackColor = Color.Transparent });
            numDuracion = new NumericUpDown() { Location = new Point(20, 145), Width = 100, Minimum = 1, Maximum = 12 };
            p.Controls.Add(numDuracion);

            p.Controls.Add(new Label() { Text = "Docente Responsable:", Location = new Point(20, 180), AutoSize = true, BackColor = Color.Transparent });
            cmbDocentes = new ComboBox() { Location = new Point(20, 205), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            p.Controls.Add(cmbDocentes);

            btnAsignar = new Button() { Text = "Crear Curso", Location = new Point(20, 250), Width = 250, Height = 40, BackColor = Color.LightSteelBlue };
            btnAsignar.Click += BtnAsignar_Click;
            p.Controls.Add(btnAsignar);

            btnEliminarCurso = new Button() { Text = "Eliminar Curso", Location = new Point(20, 300), Width = 250, Height = 40, BackColor = Color.LightCoral };
            btnEliminarCurso.Click += BtnEliminarCurso_Click;
            p.Controls.Add(btnEliminarCurso);

            Label lblLista = new Label() { Text = "Cursos Activos", Location = new Point(320, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            p.Controls.Add(lblLista);

            dgvCursos = new DataGridView() { Location = new Point(320, 60), Size = new Size(580, 480), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            p.Controls.Add(dgvCursos);
        }

        private void ConfigurarTabCalendario(TabPage p)
        {
            p.BackColor = Color.WhiteSmoke;
            AgregarMarcaDeAgua(p);

            Label lblTitulo = new Label() { Text = "Calendario Digital de Actividades", Location = new Point(20, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            p.Controls.Add(lblTitulo);

            calendario = new MonthCalendar();
            calendario.Location = new Point(20, 60);
            calendario.MaxSelectionCount = 1; 
            calendario.DateChanged += Calendario_DateChanged; 
            p.Controls.Add(calendario);

            int inputY = 240; 
            
            lblFechaSeleccionada = new Label() { Text = "Fecha: " + DateTime.Today.ToShortDateString(), Location = new Point(20, inputY), Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true, ForeColor = Color.Blue, BackColor = Color.Transparent };
            p.Controls.Add(lblFechaSeleccionada);

            p.Controls.Add(new Label() { Text = "Tipo de Actividad:", Location = new Point(20, inputY + 30), BackColor = Color.Transparent });
            cmbTipoActividad = new ComboBox() { Location = new Point(20, inputY + 55), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipoActividad.Items.AddRange(new string[] { "Evaluación", "Clase Especial", "Reunión Docente", "Entrega de Notas", "Actividad Cultural", "Cierre Administrativo" });
            cmbTipoActividad.SelectedIndex = 0;
            p.Controls.Add(cmbTipoActividad);

            p.Controls.Add(new Label() { Text = "Descripción / Detalles:", Location = new Point(20, inputY + 90), BackColor = Color.Transparent });
            txtDescripcionActividad = new TextBox() { Location = new Point(20, inputY + 115), Width = 230, Height = 60, Multiline = true };
            p.Controls.Add(txtDescripcionActividad);

            btnGuardarEvento = new Button() { Text = "Agendar Actividad", Location = new Point(20, inputY + 190), Width = 230, Height = 40, BackColor = Color.LightGreen };
            btnGuardarEvento.Click += BtnGuardarEvento_Click;
            p.Controls.Add(btnGuardarEvento);

            btnEliminarEvento = new Button() { Text = "Borrar Seleccionado", Location = new Point(20, inputY + 240), Width = 230, Height = 30, BackColor = Color.LightSalmon };
            btnEliminarEvento.Click += BtnEliminarEvento_Click;
            p.Controls.Add(btnEliminarEvento);

            Label lblAgenda = new Label() { Text = "Agenda Planificada (Ordenada por Fecha)", Location = new Point(300, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            p.Controls.Add(lblAgenda);

            dgvAgenda = new DataGridView() { Location = new Point(300, 60), Size = new Size(600, 480), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvAgenda.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            p.Controls.Add(dgvAgenda);
        }

        private void Calendario_DateChanged(object sender, DateRangeEventArgs e) {
            lblFechaSeleccionada.Text = "Fecha: " + e.Start.ToShortDateString();
        }

        private void BtnGuardarEvento_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtDescripcionActividad.Text)) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string query = "INSERT INTO actividades_calendario (fecha, tipo_actividad, descripcion) VALUES (@f, @t, @d)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@f", calendario.SelectionStart); 
                    cmd.Parameters.AddWithValue("@t", cmbTipoActividad.Text);
                    cmd.Parameters.AddWithValue("@d", txtDescripcionActividad.Text);
                    cmd.ExecuteNonQuery();
                    txtDescripcionActividad.Clear();
                    CargarAgenda(); 
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnEliminarEvento_Click(object sender, EventArgs e) {
            if (dgvAgenda.SelectedRows.Count == 0) return;
            if (MessageBox.Show("¿Eliminar actividad?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                try {
                    int idBorrar = Convert.ToInt32(dgvAgenda.SelectedRows[0].Cells["id"].Value);
                    ConexionDB db = new ConexionDB();
                    using (MySqlConnection conn = db.GetConnection()) {
                        conn.Open();
                        new MySqlCommand("DELETE FROM actividades_calendario WHERE id = " + idBorrar, conn).ExecuteNonQuery();
                        CargarAgenda();
                    }
                } catch { }
            }
        }

        private void CargarAgenda() {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string query = "SELECT id, fecha, tipo_actividad AS 'Actividad', descripcion AS 'Detalle' FROM actividades_calendario ORDER BY fecha DESC";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    dgvAgenda.DataSource = dt;
                }
            } catch { }
        }
        
        private void CargarDocentes() {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter("SELECT id, nombre_completo FROM usuarios WHERE rol = 'Docente'", conn);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    cmbDocentes.DisplayMember = "nombre_completo";
                    cmbDocentes.ValueMember = "id";
                    cmbDocentes.DataSource = dt;
                }
            } catch { }
        }

        private void BtnAsignar_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtNombreCurso.Text) || cmbDocentes.SelectedIndex == -1) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string query = "INSERT INTO cursos (nombre_curso, duracion_meses, docente_id) VALUES (@nom, @dur, @docId)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nom", txtNombreCurso.Text);
                    cmd.Parameters.AddWithValue("@dur", numDuracion.Value);
                    cmd.Parameters.AddWithValue("@docId", cmbDocentes.SelectedValue);
                    cmd.ExecuteNonQuery();
                    txtNombreCurso.Clear();
                    CargarCursos();
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnEliminarCurso_Click(object sender, EventArgs e) {
            if (dgvCursos.SelectedRows.Count == 0) return;
            if (MessageBox.Show("¿Eliminar curso seleccionado?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                try {
                    int idBorrar = Convert.ToInt32(dgvCursos.SelectedRows[0].Cells["id"].Value);
                    ConexionDB db = new ConexionDB();
                    using (MySqlConnection conn = db.GetConnection()) {
                        conn.Open();
                        // primero comprobar si hay entidades relacionadas
                        MySqlCommand cmdCheck1 = new MySqlCommand("SELECT COUNT(*) FROM estudiantes WHERE curso_id = @id", conn);
                        cmdCheck1.Parameters.AddWithValue("@id", idBorrar);
                        int cantEst = Convert.ToInt32(cmdCheck1.ExecuteScalar());
                        MySqlCommand cmdCheck2 = new MySqlCommand("SELECT COUNT(*) FROM calificaciones WHERE curso_id = @id", conn);
                        cmdCheck2.Parameters.AddWithValue("@id", idBorrar);
                        int cantCal = Convert.ToInt32(cmdCheck2.ExecuteScalar());
                        if (cantEst > 0 || cantCal > 0) {
                            MessageBox.Show("No se puede eliminar el curso porque hay estudiantes o calificaciones asociadas.\n" +
                                "Elimine primero esos registros o cambieles de curso.", "Operación cancelada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        MySqlCommand cmd = new MySqlCommand("DELETE FROM cursos WHERE id = @id", conn);
                        cmd.Parameters.AddWithValue("@id", idBorrar);
                        cmd.ExecuteNonQuery();
                    }
                    CargarCursos();
                } catch (Exception ex) {
                    MessageBox.Show("Error al borrar curso: " + ex.Message);
                }
            }
        }

        private void CargarCursos() {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string query = "SELECT c.id, c.nombre_curso AS 'Asignatura', c.duracion_meses AS 'Meses', u.nombre_completo AS 'Docente' FROM cursos c INNER JOIN usuarios u ON c.docente_id = u.id";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    dgvCursos.DataSource = dt;
                    if (dgvCursos.Columns.Contains("id"))
                        dgvCursos.Columns["id"].Visible = false;
                }
            } catch { }
        }
    }
}