#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormCalificaciones : Form
    {
        private TabControl tabControl;

        private ComboBox cmbCursos1, cmbEstudiantes1;
        private NumericUpDown numNota1, numNota2, numNota3;
        private TextBox txtDesc1, txtDesc2, txtDesc3;
        private Button btnGuardar, btnModificar; 
        private Label lblEstado;
        private int idCalificacionExistente = 0;

        private ComboBox cmbCursos2;
        private DataGridView dgvPromedios;
        private Button btnCalcularTodo;

        private Bitmap logoFondoTabs;

        public FormCalificaciones()
        {
            // --- CARGA DE LOGO OPTIMIZADA ---
            try {
                logoFondoTabs = new Bitmap("Logo.png");
                logoFondoTabs.MakeTransparent(Color.White);
            } catch { }

            ConfigurarFormulario();
            try { CargarListas(); } catch { }
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Gestión de Calificaciones";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            TabPage tabRegistro = new TabPage("Registro de Evaluaciones");
            ConfigurarTabRegistro(tabRegistro);
            tabControl.TabPages.Add(tabRegistro);

            TabPage tabPromedios = new TabPage("Cierre de Curso y Promedios");
            ConfigurarTabPromedios(tabPromedios);
            tabControl.TabPages.Add(tabPromedios);

            this.Controls.Add(tabControl);
        }

        private void AgregarMarcaDeAgua(TabPage p)
        {
            // watermark disabled for all tabs - logo only on dashboard
        }

        private void ConfigurarTabRegistro(TabPage p)
        {
            p.BackColor = Color.WhiteSmoke;
            AgregarMarcaDeAgua(p);

            Label lblT = new Label() { Text = "Registro de Notas Parciales", Location = new Point(20, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            p.Controls.Add(lblT);

            p.Controls.Add(new Label() { Text = "Curso:", Location = new Point(20, 60), BackColor = Color.Transparent });
            cmbCursos1 = new ComboBox() { Location = new Point(20, 85), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCursos1.SelectedIndexChanged += (s, e) => {
                if (cmbCursos1.SelectedValue != null)
                    CargarEstudiantesParaCurso(Convert.ToInt32(cmbCursos1.SelectedValue));
                BuscarNota();
            };
            p.Controls.Add(cmbCursos1);

            p.Controls.Add(new Label() { Text = "Estudiante:", Location = new Point(340, 60), BackColor = Color.Transparent });
            cmbEstudiantes1 = new ComboBox() { Location = new Point(340, 85), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEstudiantes1.SelectedIndexChanged += (s, e) => BuscarNota();
            p.Controls.Add(cmbEstudiantes1);

            lblEstado = new Label() { Text = "Estado: ESPERANDO DATOS", Location = new Point(660, 88), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, BackColor = Color.Transparent };
            p.Controls.Add(lblEstado);

            int y = 150;
            p.Controls.Add(new Label() { Text = "Evaluación 1 (30%):", Location = new Point(20, y), Font = new Font("Arial", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label() { Text = "Nota (1-20):", Location = new Point(20, y + 25), BackColor = Color.Transparent });
            numNota1 = CrearNum(20, y + 50); p.Controls.Add(numNota1);
            p.Controls.Add(new Label() { Text = "Descripción (ej. Examen):", Location = new Point(120, y + 25), BackColor = Color.Transparent });
            txtDesc1 = new TextBox() { Location = new Point(120, y + 50), Width = 200 }; p.Controls.Add(txtDesc1);

            int y2 = y + 80;
            p.Controls.Add(new Label() { Text = "Evaluación 2 (30%):", Location = new Point(20, y2), Font = new Font("Arial", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label() { Text = "Nota (1-20):", Location = new Point(20, y2 + 25), BackColor = Color.Transparent });
            numNota2 = CrearNum(20, y2 + 50); p.Controls.Add(numNota2);
            p.Controls.Add(new Label() { Text = "Descripción:", Location = new Point(120, y2 + 25), BackColor = Color.Transparent });
            txtDesc2 = new TextBox() { Location = new Point(120, y2 + 50), Width = 200 }; p.Controls.Add(txtDesc2);

            int y3 = y2 + 80;
            p.Controls.Add(new Label() { Text = "Evaluación 3 (40%):", Location = new Point(20, y3), Font = new Font("Arial", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label() { Text = "Nota (1-20):", Location = new Point(20, y3 + 25), BackColor = Color.Transparent });
            numNota3 = CrearNum(20, y3 + 50); p.Controls.Add(numNota3);
            p.Controls.Add(new Label() { Text = "Descripción:", Location = new Point(120, y3 + 25), BackColor = Color.Transparent });
            txtDesc3 = new TextBox() { Location = new Point(120, y3 + 50), Width = 200 }; p.Controls.Add(txtDesc3);

            btnGuardar = new Button() { Text = "Guardar Nueva Nota", Location = new Point(400, 200), Width = 200, Height = 50, BackColor = Color.LightGreen, Enabled = false };
            btnGuardar.Click += BtnGuardar_Click;
            p.Controls.Add(btnGuardar);

            btnModificar = new Button() { Text = "Modificar Existente", Location = new Point(400, 260), Width = 200, Height = 50, BackColor = Color.LightYellow, Enabled = false };
            btnModificar.Click += BtnModificar_Click;
            p.Controls.Add(btnModificar);
        }

        private void ConfigurarTabPromedios(TabPage p)
        {
            p.BackColor = Color.WhiteSmoke;
            AgregarMarcaDeAgua(p);

            p.Controls.Add(new Label() { Text = "Seleccionar Curso para Cerrar:", Location = new Point(20, 20), BackColor = Color.Transparent });
            cmbCursos2 = new ComboBox() { Location = new Point(20, 45), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCursos2.SelectedIndexChanged += (s, e) => CargarTablaPromedios();
            p.Controls.Add(cmbCursos2);

            btnCalcularTodo = new Button() { Text = "Recalcular Promedios y Cerrar", Location = new Point(340, 43), Width = 250 };
            btnCalcularTodo.Click += BtnCalcularTodo_Click;
            p.Controls.Add(btnCalcularTodo);

            Button btnEliminarPromedio = new Button() { Text = "Eliminar Nota Seleccionada", Location = new Point(600, 43), Width = 200, BackColor = Color.LightCoral };
            btnEliminarPromedio.Click += BtnEliminarPromedio_Click;
            p.Controls.Add(btnEliminarPromedio);

            dgvPromedios = new DataGridView() { Location = new Point(20, 90), Size = new Size(900, 450), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            p.Controls.Add(dgvPromedios);
        }

        private NumericUpDown CrearNum(int x, int y) {
            return new NumericUpDown() { Location = new Point(x, y), Width = 80, DecimalPlaces = 2, Maximum = 20 };
        }

        private void BuscarNota()
        {
            if (cmbCursos1.SelectedValue == null || cmbEstudiantes1.SelectedValue == null) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    // localizar la inscripción y la calificación asociada
                    string qIns = "SELECT id FROM inscripciones WHERE estudiante_id=@e AND curso_id=@c LIMIT 1";
                    MySqlCommand cmdIns = new MySqlCommand(qIns, conn);
                    cmdIns.Parameters.AddWithValue("@e", cmbEstudiantes1.SelectedValue);
                    cmdIns.Parameters.AddWithValue("@c", cmbCursos1.SelectedValue);
                    var insIdObj = cmdIns.ExecuteScalar();
                    if (insIdObj == null) {
                        // no hay inscripción
                        idCalificacionExistente = 0;
                        numNota1.Value = 0; numNota2.Value = 0; numNota3.Value = 0;
                        txtDesc1.Clear(); txtDesc2.Clear(); txtDesc3.Clear();
                        lblEstado.Text = "Estudiante no inscrito en el curso";
                        lblEstado.ForeColor = Color.Gray;
                        btnGuardar.Enabled = false; btnModificar.Enabled = false;
                        return;
                    }
                    int insId = Convert.ToInt32(insIdObj);

                    string q = "SELECT * FROM calificaciones WHERE inscripcion_id=@ins LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@ins", insId);
                    using (MySqlDataReader r = cmd.ExecuteReader()) {
                        if (r.Read()) {
                            idCalificacionExistente = Convert.ToInt32(r["id"]);
                            numNota1.Value = Convert.ToDecimal(r["nota1"]); numNota2.Value = Convert.ToDecimal(r["nota2"]); numNota3.Value = Convert.ToDecimal(r["nota3"]);
                            txtDesc1.Text = r["desc1"].ToString(); txtDesc2.Text = r["desc2"].ToString(); txtDesc3.Text = r["desc3"].ToString();
                            lblEstado.Text = "Modo: EDITAR (Nota ya existe)"; 
                            lblEstado.ForeColor = Color.OrangeRed;
                            btnModificar.Enabled = true; btnGuardar.Enabled = false;
                        } else {
                            idCalificacionExistente = 0;
                            numNota1.Value = 0; numNota2.Value = 0; numNota3.Value = 0;
                            txtDesc1.Clear(); txtDesc2.Clear(); txtDesc3.Clear();
                            lblEstado.Text = "Modo: NUEVO REGISTRO"; 
                            lblEstado.ForeColor = Color.Green;
                            btnGuardar.Enabled = true; btnModificar.Enabled = false;
                        }
                    }
                }
            } catch { }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbCursos1.SelectedValue == null || cmbEstudiantes1.SelectedValue == null) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        // asegurar que exista una inscripción y obtener su id
                        string qIns = "SELECT id FROM inscripciones WHERE estudiante_id=@e AND curso_id=@c LIMIT 1";
                        MySqlCommand cmdIns = new MySqlCommand(qIns, conn, tx);
                        cmdIns.Parameters.AddWithValue("@e", cmbEstudiantes1.SelectedValue);
                        cmdIns.Parameters.AddWithValue("@c", cmbCursos1.SelectedValue);
                        var insObj = cmdIns.ExecuteScalar();
                        int insId;
                        if (insObj == null) {
                            // crear inscripción automática si no existe
                            MySqlCommand createIns = new MySqlCommand("INSERT INTO inscripciones (estudiante_id, curso_id, fecha_inscripcion) VALUES (@e,@c,@f)", conn, tx);
                            createIns.Parameters.AddWithValue("@e", cmbEstudiantes1.SelectedValue);
                            createIns.Parameters.AddWithValue("@c", cmbCursos1.SelectedValue);
                            createIns.Parameters.AddWithValue("@f", DateTime.Now);
                            createIns.ExecuteNonQuery();
                            insId = Convert.ToInt32(new MySql.Data.MySqlClient.MySqlCommand("SELECT LAST_INSERT_ID()", conn, tx).ExecuteScalar());
                        } else insId = Convert.ToInt32(insObj);

                        string q = "INSERT INTO calificaciones (inscripcion_id, nota1, desc1, nota2, desc2, nota3, desc3, nota_final) VALUES (@ins, @n1, @d1, @n2, @d2, @n3, @d3, 0)";
                        MySqlCommand cmd = new MySqlCommand(q, conn, tx);
                        cmd.Parameters.AddWithValue("@ins", insId);
                        cmd.Parameters.AddWithValue("@n1", numNota1.Value); cmd.Parameters.AddWithValue("@d1", txtDesc1.Text);
                        cmd.Parameters.AddWithValue("@n2", numNota2.Value); cmd.Parameters.AddWithValue("@d2", txtDesc2.Text);
                        cmd.Parameters.AddWithValue("@n3", numNota3.Value); cmd.Parameters.AddWithValue("@d3", txtDesc3.Text);
                        cmd.ExecuteNonQuery();

                        tx.Commit();
                        MessageBox.Show("Nota Creada Correctamente."); 
                        try { AuditHelper.Log("Calificaciones", "Crear", $"inscripcion={insId}; estudiante={cmbEstudiantes1.SelectedValue}; curso={cmbCursos1.SelectedValue}"); } catch {}
                        BuscarNota(); 
                    } catch {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (idCalificacionExistente == 0) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        string q = "UPDATE calificaciones SET nota1=@n1, desc1=@d1, nota2=@n2, desc2=@d2, nota3=@n3, desc3=@d3 WHERE id=@id";
                        MySqlCommand cmd = new MySqlCommand(q, conn, tx);
                        cmd.Parameters.AddWithValue("@n1", numNota1.Value); cmd.Parameters.AddWithValue("@d1", txtDesc1.Text);
                        cmd.Parameters.AddWithValue("@n2", numNota2.Value); cmd.Parameters.AddWithValue("@d2", txtDesc2.Text);
                        cmd.Parameters.AddWithValue("@n3", numNota3.Value); cmd.Parameters.AddWithValue("@d3", txtDesc3.Text);
                        cmd.Parameters.AddWithValue("@id", idCalificacionExistente);
                        cmd.ExecuteNonQuery();

                        tx.Commit();
                        MessageBox.Show("Nota Modificada Correctamente.");
                        try { AuditHelper.Log("Calificaciones", "Modificar", $"calificacion_id={idCalificacionExistente}; estudiante={cmbEstudiantes1.SelectedValue}; curso={cmbCursos1.SelectedValue}"); } catch {}
                    } catch {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnCalcularTodo_Click(object sender, EventArgs e)
        {
             if (cmbCursos2.SelectedValue == null) return;
             try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        // obtener parámetros desde la tabla de configuración
                        decimal p1 = 0.30m, p2 = 0.30m, p3 = 0.40m; decimal notaMin = 10.00m;
                        try {
                            MySqlCommand qc = new MySqlCommand("SELECT porcentaje_evaluacion_1, porcentaje_evaluacion_2, porcentaje_evaluacion_3, nota_minima_aprobatoria FROM configuracion_sistema WHERE id=1 LIMIT 1", conn, tx);
                            using var rc = qc.ExecuteReader();
                            if (rc.Read()) {
                                p1 = Convert.ToDecimal(rc["porcentaje_evaluacion_1"]) / 100m;
                                p2 = Convert.ToDecimal(rc["porcentaje_evaluacion_2"]) / 100m;
                                p3 = Convert.ToDecimal(rc["porcentaje_evaluacion_3"]) / 100m;
                                notaMin = Convert.ToDecimal(rc["nota_minima_aprobatoria"]);
                            }
                        } catch { }

                        string qUpdate = "UPDATE calificaciones cal JOIN inscripciones i ON cal.inscripcion_id = i.id " +
                                         "SET cal.nota_final = (cal.nota1*@p1 + cal.nota2*@p2 + cal.nota3*@p3), " +
                                         "cal.aprobado = CASE WHEN (cal.nota1*@p1 + cal.nota2*@p2 + cal.nota3*@p3) >= @min THEN 1 ELSE 0 END " +
                                         "WHERE i.curso_id = @c";
                        MySqlCommand cmd = new MySqlCommand(qUpdate, conn, tx);
                        cmd.Parameters.AddWithValue("@p1", p1);
                        cmd.Parameters.AddWithValue("@p2", p2);
                        cmd.Parameters.AddWithValue("@p3", p3);
                        cmd.Parameters.AddWithValue("@min", notaMin);
                        cmd.Parameters.AddWithValue("@c", Convert.ToInt32(cmbCursos2.SelectedValue));
                        cmd.ExecuteNonQuery();

                        tx.Commit();
                        MessageBox.Show("Cierre de curso exitoso. Promedios calculados.");
                        try { AuditHelper.Log("Calificaciones", "CierreCurso", $"curso_id={cmbCursos2.SelectedValue}"); } catch {}
                        CargarTablaPromedios();
                    } catch {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
             } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnEliminarPromedio_Click(object sender, EventArgs e)
        {
            if (cmbCursos2.SelectedValue == null) return;
            if (dgvPromedios.CurrentRow == null) return;
            int id = Convert.ToInt32(dgvPromedios.CurrentRow.Cells["id"].Value);
            if (MessageBox.Show("¿Eliminar la calificación seleccionada?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        MySqlCommand del = new MySqlCommand("DELETE FROM calificaciones WHERE id=@id", conn, tx);
                        del.Parameters.AddWithValue("@id", id);
                        del.ExecuteNonQuery();
                        tx.Commit();
                    } catch {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
                try { AuditHelper.Log("Calificaciones", "Eliminar", $"calificacion_id={id}; curso_id={cmbCursos2.SelectedValue}"); } catch {}
                CargarTablaPromedios();
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void CargarTablaPromedios()
        {
            if (cmbCursos2.SelectedValue == null) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string q = "SELECT cal.id, e.nombres, e.apellidos, cal.nota1, cal.nota2, cal.nota3, cal.nota_final AS Definitiva, " +
                               "CASE WHEN cal.aprobado=1 THEN 'APROBADO' ELSE 'REPROBADO' END AS Estado " +
                               "FROM calificaciones cal JOIN inscripciones i ON cal.inscripcion_id = i.id JOIN estudiantes e ON i.estudiante_id = e.id WHERE i.curso_id = @c";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@c", Convert.ToInt32(cmbCursos2.SelectedValue));
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    dgvPromedios.DataSource = dt;
                    if (dgvPromedios.Columns.Contains("id"))
                        dgvPromedios.Columns["id"].Visible = false;
                }
            } catch { }
        }

        private void CargarListas() {
            ConexionDB db = new ConexionDB();
            using (MySqlConnection conn = db.GetConnection()) {
                conn.Open();
                MySqlDataAdapter daC = new MySqlDataAdapter("SELECT id, nombre_curso FROM cursos", conn);
                DataTable dtC = new DataTable(); daC.Fill(dtC);
                cmbCursos1.DisplayMember = "nombre_curso"; cmbCursos1.ValueMember = "id"; cmbCursos1.DataSource = dtC;
                cmbCursos2.DisplayMember = "nombre_curso"; cmbCursos2.ValueMember = "id"; cmbCursos2.DataSource = dtC.Copy();
                
                // students list is populated when a course is chosen
            }
        }

        private void CargarEstudiantesParaCurso(int cursoId)
        {
            ConexionDB db = new ConexionDB();
            using (MySqlConnection conn = db.GetConnection()) {
                conn.Open();
                string q = "SELECT e.id, CONCAT(e.nombres, ' ', e.apellidos) AS nombre " +
                           "FROM inscripciones i JOIN estudiantes e ON i.estudiante_id = e.id WHERE i.curso_id = @c";
                MySqlDataAdapter da = new MySqlDataAdapter(q, conn);
                da.SelectCommand.Parameters.AddWithValue("@c", cursoId);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbEstudiantes1.DisplayMember = "nombre";
                cmbEstudiantes1.ValueMember = "id";
                cmbEstudiantes1.DataSource = dt;
            }
        }
    }
}