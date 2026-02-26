#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormEstudiantes : Form
    {
        private TextBox txtCedula, txtNombres, txtApellidos, txtTelefono, txtCorreo;
        private CheckedListBox clbCursos;
        private TextBox txtDuracion; 
        private CheckBox chkRequisitos; 
        private DataGridView dgvEstudiantes; 
        private Button btnGuardar, btnLimpiar, btnEditar, btnEliminar;
        
        private int idSeleccionado = 0;
        private string ciOriginal = null; 
        private DataTable dtCursosDisponibles; 
        private List<int> cursosOriginales = new List<int>();

        public FormEstudiantes()
        {
            ConfigurarFormulario();


            try { 
                AsegurarIndice();
                CargarCursosEnCombo(); 
                CargarEstudiantes(); 
            } catch { } 
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Gestión de Inscripción y Asignación";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            int y = 30; int lblX = 20; int txtX = 140; int spacing = 40;

            Label lblTitulo = new Label() { Text = "Datos del Estudiante", Location = new Point(20, 10), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            this.Controls.Add(lblTitulo);

            this.Controls.Add(new Label() { Text = "Cédula:", Location = new Point(lblX, y + spacing), BackColor = Color.Transparent });
            txtCedula = new TextBox() { Location = new Point(txtX, y + spacing), Width = 150 };
            this.Controls.Add(txtCedula);

            this.Controls.Add(new Label() { Text = "Nombres:", Location = new Point(lblX, y + spacing * 2), BackColor = Color.Transparent });
            txtNombres = new TextBox() { Location = new Point(txtX, y + spacing * 2), Width = 200 };
            this.Controls.Add(txtNombres);

            this.Controls.Add(new Label() { Text = "Apellidos:", Location = new Point(lblX, y + spacing * 3), BackColor = Color.Transparent });
            txtApellidos = new TextBox() { Location = new Point(txtX, y + spacing * 3), Width = 200 };
            this.Controls.Add(txtApellidos);

            this.Controls.Add(new Label() { Text = "Teléfono:", Location = new Point(lblX, y + spacing * 4), BackColor = Color.Transparent });
            txtTelefono = new TextBox() { Location = new Point(txtX, y + spacing * 4), Width = 150 };
            this.Controls.Add(txtTelefono);

            this.Controls.Add(new Label() { Text = "E-Mail:", Location = new Point(lblX, y + spacing * 5), BackColor = Color.Transparent });
            txtCorreo = new TextBox() { Location = new Point(txtX, y + spacing * 5), Width = 200 };
            this.Controls.Add(txtCorreo);

            this.Controls.Add(new Label() { Text = "Cursos (marcar):", Location = new Point(lblX, y + spacing * 6), BackColor = Color.Transparent });
            clbCursos = new CheckedListBox() { Location = new Point(txtX, y + spacing * 6), Width = 200, Height = 80, CheckOnClick = true, IntegralHeight = false, ScrollAlwaysVisible = true };
            clbCursos.ItemCheck += ClbCursos_ItemCheck;
            clbCursos.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(clbCursos);

            this.Controls.Add(new Label() { Text = "Duración:", Location = new Point(lblX, y + spacing * 9), BackColor = Color.Transparent });
            txtDuracion = new TextBox() { Location = new Point(txtX, y + spacing * 9), Width = 80, ReadOnly = true, BackColor = Color.WhiteSmoke };
            this.Controls.Add(txtDuracion);

            chkRequisitos = new CheckBox() { Text = "Requisitos Verificados", Location = new Point(txtX, y + spacing * 10), Width = 250, BackColor = Color.Transparent };
            this.Controls.Add(chkRequisitos);

            int btnY = y + spacing * 12;
            btnGuardar = new Button() { Text = "Inscribir", Location = new Point(20, btnY), Width = 100, Height=40, BackColor = Color.LightGreen };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            btnEditar = new Button() { Text = "Modificar", Location = new Point(130, btnY), Width = 100, Height = 40, BackColor = Color.LightYellow };
            btnEditar.Click += BtnEditar_Click;
            this.Controls.Add(btnEditar);

            btnEliminar = new Button() { Text = "Eliminar", Location = new Point(240, btnY), Width = 100, Height = 40, BackColor = Color.LightSalmon };
            btnEliminar.Click += BtnEliminar_Click;
            this.Controls.Add(btnEliminar);

            btnLimpiar = new Button() { Text = "Limpiar", Location = new Point(20, btnY + 50), Width = 320 };
            btnLimpiar.Click += (s, e) => LimpiarCampos();
            this.Controls.Add(btnLimpiar);

            dgvEstudiantes = new DataGridView() { Location = new Point(400, 50), Size = new Size(660, 550), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvEstudiantes.CellClick += DgvEstudiantes_CellClick;
            this.Controls.Add(dgvEstudiantes);
        }

        private class CursoItem { public int Id; public string Nombre; public int Duracion;
            public override string ToString() => Nombre;
        }

        private void CargarCursosEnCombo() {
            ConexionDB db = new ConexionDB();
            using (MySqlConnection conn = db.GetConnection()) {
                conn.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT id, nombre_curso, duracion_meses FROM cursos", conn);
                dtCursosDisponibles = new DataTable();
                da.Fill(dtCursosDisponibles);

                clbCursos.Items.Clear();
                foreach (DataRow row in dtCursosDisponibles.Rows) {
                    clbCursos.Items.Add(new CursoItem {
                        Id = Convert.ToInt32(row["id"]),
                        Nombre = row["nombre_curso"].ToString(),
                        Duracion = Convert.ToInt32(row["duracion_meses"])
                    });
                }
            }
        }

        private void ClbCursos_ItemCheck(object sender, ItemCheckEventArgs e) {
            this.BeginInvoke((MethodInvoker)delegate {
                var seleccionados = clbCursos.CheckedItems.Cast<CursoItem>().ToList();
                if (e.NewValue == CheckState.Checked) seleccionados.Add((CursoItem)clbCursos.Items[e.Index]);
                if (e.NewValue == CheckState.Unchecked) seleccionados.RemoveAll(c => c.Id == ((CursoItem)clbCursos.Items[e.Index]).Id);

                if (seleccionados.Count == 1) txtDuracion.Text = seleccionados[0].Duracion.ToString();
                else txtDuracion.Clear();
            });
        }

        private bool ValidarDatos() {
            if (string.IsNullOrWhiteSpace(txtCedula.Text)) return false;
            if (string.IsNullOrWhiteSpace(txtNombres.Text) || string.IsNullOrWhiteSpace(txtApellidos.Text)) return false;
            if (clbCursos.CheckedItems.Count == 0) return false;
            return true;
        }

        private void BtnGuardar_Click(object sender, EventArgs e) {
            if (!ValidarDatos()) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();

                    // 1) Crear o obtener estudiante
                    long estudianteId = 0;
                    MySqlCommand find = new MySqlCommand("SELECT id FROM estudiantes WHERE ci=@ci LIMIT 1", conn);
                    find.Parameters.AddWithValue("@ci", txtCedula.Text);
                    var fid = find.ExecuteScalar();
                    if (fid != null) estudianteId = Convert.ToInt64(fid);
                    else {
                        string qIns = "INSERT INTO estudiantes (ci, nombres, apellidos, telefono, correo, fecha_registro) VALUES (@ci, @no, @ap, @te, @co, @fe)";
                        MySqlCommand cmdIns = new MySqlCommand(qIns, conn);
                        cmdIns.Parameters.AddWithValue("@ci", txtCedula.Text);
                        cmdIns.Parameters.AddWithValue("@no", txtNombres.Text);
                        cmdIns.Parameters.AddWithValue("@ap", txtApellidos.Text);
                        cmdIns.Parameters.AddWithValue("@te", txtTelefono.Text);
                        cmdIns.Parameters.AddWithValue("@co", txtCorreo.Text);
                        cmdIns.Parameters.AddWithValue("@fe", DateTime.Now);
                        cmdIns.ExecuteNonQuery();
                        estudianteId = Convert.ToInt64(new MySql.Data.MySqlClient.MySqlCommand("SELECT LAST_INSERT_ID()", conn).ExecuteScalar());
                    }

                    // 2) Crear inscripciones por curso seleccionado
                    foreach (CursoItem curso in clbCursos.CheckedItems) {
                        MySqlCommand exists = new MySqlCommand("SELECT COUNT(*) FROM inscripciones WHERE estudiante_id=@e AND curso_id=@cu", conn);
                        exists.Parameters.AddWithValue("@e", estudianteId);
                        exists.Parameters.AddWithValue("@cu", curso.Id);
                        if (Convert.ToInt64(exists.ExecuteScalar()) > 0) continue;

                        string q = "INSERT INTO inscripciones (estudiante_id, curso_id, requisitos_verificados, fecha_inscripcion) VALUES (@e, @cu, @req, @fe)";
                        MySqlCommand cmd = new MySqlCommand(q, conn);
                        cmd.Parameters.AddWithValue("@e", estudianteId);
                        cmd.Parameters.AddWithValue("@cu", curso.Id);
                        cmd.Parameters.AddWithValue("@req", chkRequisitos.Checked);
                        cmd.Parameters.AddWithValue("@fe", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Inscripciones guardadas."); 
                    try { AuditHelper.Log("Estudiantes", "CrearInscripciones", $"estudiante_id={estudianteId}; cursos={string.Join(',', clbCursos.CheckedItems.Cast<CursoItem>().Select(c=>c.Id))}"); } catch {}
                    LimpiarCampos(); CargarEstudiantes();
                }
            } catch (MySqlException ex) when (ex.Number == 1062) {
                // duplicate key, should no longer happen but handle gracefully
                MessageBox.Show("El alumno ya está inscrito en uno de los cursos seleccionados.");
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void CargarEstudiantes() {
            ConexionDB db = new ConexionDB();
            using (MySqlConnection conn = db.GetConnection()) {
                conn.Open();
                string q = "SELECT e.id AS id, e.ci, e.nombres, e.apellidos, GROUP_CONCAT(c.nombre_curso SEPARATOR ', ') AS cursos " +
                           "FROM estudiantes e LEFT JOIN inscripciones i ON e.id = i.estudiante_id LEFT JOIN cursos c ON i.curso_id = c.id " +
                           "GROUP BY e.id ORDER BY e.id DESC";
                MySqlDataAdapter da = new MySqlDataAdapter(q, conn);
                DataTable dt = new DataTable(); da.Fill(dt);
                dgvEstudiantes.DataSource = dt;
            }
        }

        private void DgvEstudiantes_CellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex >= 0) {
                idSeleccionado = Convert.ToInt32(dgvEstudiantes.Rows[e.RowIndex].Cells["id"].Value);
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    // cargar datos del estudiante
                    MySqlCommand cmd = new MySqlCommand("SELECT ci, nombres, apellidos, telefono, correo FROM estudiantes WHERE id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", idSeleccionado);
                    using (MySqlDataReader r = cmd.ExecuteReader()) {
                        if (r.Read()) {
                            txtCedula.Text = r["ci"].ToString();
                            txtNombres.Text = r["nombres"].ToString();
                            txtApellidos.Text = r["apellidos"].ToString();
                            txtTelefono.Text = r["telefono"].ToString();
                            txtCorreo.Text = r["correo"].ToString();
                        }
                    }

                    // cargar inscripciones y marcar cursos
                    for (int i = 0; i < clbCursos.Items.Count; i++) clbCursos.SetItemChecked(i, false);
                    cursosOriginales.Clear();
                    MySqlCommand qIns = new MySqlCommand("SELECT curso_id, requisitos_verificados FROM inscripciones WHERE estudiante_id=@id", conn);
                    qIns.Parameters.AddWithValue("@id", idSeleccionado);
                    using (MySqlDataReader r2 = qIns.ExecuteReader()) {
                        bool anyReq = false;
                        while (r2.Read()) {
                            int cursoId = Convert.ToInt32(r2["curso_id"]);
                            cursosOriginales.Add(cursoId);
                            anyReq = anyReq || Convert.ToBoolean(r2["requisitos_verificados"]);
                            for (int i = 0; i < clbCursos.Items.Count; i++) {
                                var item = (CursoItem)clbCursos.Items[i];
                                if (item.Id == cursoId) clbCursos.SetItemChecked(i, true);
                            }
                        }
                        chkRequisitos.Checked = anyReq;
                    }
                    ciOriginal = txtCedula.Text;
                }
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e) {
            if (idSeleccionado == 0) return;
            if (!ValidarDatos()) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();

                    // comenzar transacción única para actualizar estudiante e inscripciones
                    using var tx = conn.BeginTransaction();
                    try {
                        // actualizar datos del estudiante
                        string upd = "UPDATE estudiantes SET ci=@ci, nombres=@no, apellidos=@ap, telefono=@te, correo=@co WHERE id=@id";
                        MySqlCommand ucmd = new MySqlCommand(upd, conn, tx);
                        ucmd.Parameters.AddWithValue("@ci", txtCedula.Text);
                        ucmd.Parameters.AddWithValue("@no", txtNombres.Text);
                        ucmd.Parameters.AddWithValue("@ap", txtApellidos.Text);
                        ucmd.Parameters.AddWithValue("@te", txtTelefono.Text);
                        ucmd.Parameters.AddWithValue("@co", txtCorreo.Text);
                        ucmd.Parameters.AddWithValue("@id", idSeleccionado);
                        ucmd.ExecuteNonQuery();

                        // manejar inscripciones: insertar faltantes
                        long estudianteId = idSeleccionado;
                        foreach (CursoItem curso in clbCursos.CheckedItems) {
                            MySqlCommand exists = new MySqlCommand("SELECT COUNT(*) FROM inscripciones WHERE estudiante_id=@e AND curso_id=@cu", conn, tx);
                            exists.Parameters.AddWithValue("@e", estudianteId);
                            exists.Parameters.AddWithValue("@cu", curso.Id);
                            if (Convert.ToInt64(exists.ExecuteScalar()) > 0) continue;

                            string q = "INSERT INTO inscripciones (estudiante_id, curso_id, requisitos_verificados, fecha_inscripcion) VALUES (@e, @cu, @req, @fe)";
                            MySqlCommand cmd = new MySqlCommand(q, conn, tx);
                            cmd.Parameters.AddWithValue("@e", estudianteId);
                            cmd.Parameters.AddWithValue("@cu", curso.Id);
                            cmd.Parameters.AddWithValue("@req", chkRequisitos.Checked);
                            cmd.Parameters.AddWithValue("@fe", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }

                        // eliminar inscripciones no seleccionadas
                        var seleccionados = clbCursos.CheckedItems.Cast<CursoItem>().Select(c => c.Id).ToList();
                        if (seleccionados.Count > 0) {
                            var names = seleccionados.Select((id, idx) => "@cid" + idx).ToArray();
                            string inClause = string.Join(",", names);
                            string delSql = $"DELETE FROM inscripciones WHERE estudiante_id = @id AND curso_id NOT IN ({inClause})";
                            MySqlCommand del = new MySqlCommand(delSql, conn, tx);
                            del.Parameters.AddWithValue("@id", estudianteId);
                            for (int i = 0; i < seleccionados.Count; i++) del.Parameters.AddWithValue(names[i], seleccionados[i]);
                            del.ExecuteNonQuery();
                        } else {
                            MySqlCommand delAll = new MySqlCommand("DELETE FROM inscripciones WHERE estudiante_id = @id", conn, tx);
                            delAll.Parameters.AddWithValue("@id", estudianteId);
                            delAll.ExecuteNonQuery();
                        }

                        tx.Commit();
                        MessageBox.Show("Inscripciones guardadas.");
                        try { AuditHelper.Log("Estudiantes", "CrearInscripciones", $"estudiante_id={estudianteId}; cursos={string.Join(',', clbCursos.CheckedItems.Cast<CursoItem>().Select(c=>c.Id))}"); } catch {}
                        LimpiarCampos(); CargarEstudiantes();
                    } catch (Exception exTx) {
                        try { tx.Rollback(); } catch { }
                        MessageBox.Show(exTx.Message);
                    }
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnEliminar_Click(object sender, EventArgs e) {
            if (idSeleccionado == 0) return;
            if (MessageBox.Show("¿Eliminar el estudiante seleccionado? Esta acción es irreversible.", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        // eliminar calificaciones relacionadas
                        using var delCal = new MySqlCommand("DELETE cal FROM calificaciones cal JOIN inscripciones i ON cal.inscripcion_id = i.id WHERE i.estudiante_id = @id", conn, tx);
                        delCal.Parameters.AddWithValue("@id", idSeleccionado);
                        delCal.ExecuteNonQuery();
                        // eliminar inscripciones
                        using var delIns = new MySqlCommand("DELETE FROM inscripciones WHERE estudiante_id = @id", conn, tx);
                        delIns.Parameters.AddWithValue("@id", idSeleccionado);
                        delIns.ExecuteNonQuery();
                        // eliminar estudiante
                        using var delEst = new MySqlCommand("DELETE FROM estudiantes WHERE id = @id", conn, tx);
                        delEst.Parameters.AddWithValue("@id", idSeleccionado);
                        delEst.ExecuteNonQuery();

                        tx.Commit();
                        MessageBox.Show("Eliminado."); 
                        try { AuditHelper.Log("Estudiantes", "Eliminar", $"estudiante_id={idSeleccionado}"); } catch {}
                        LimpiarCampos(); CargarEstudiantes();
                    } catch {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void AsegurarIndice()
        {
            ConexionDB db = new ConexionDB();
            using var conn = db.GetConnection();
            conn.Open();
            // NO-OP: la creación de índices y cambios en esquema se debe realizar mediante
            // scripts de migración (database_schema.sql). Evitamos DDL en runtime.
        }

        private void AsegurarIndice(MySqlConnection conn)
        {
            // NO-OP: manejar índices desde scripts de migración
        }

        private void LimpiarCampos() {
            txtCedula.Clear(); txtNombres.Clear(); txtApellidos.Clear(); txtTelefono.Clear(); txtCorreo.Clear();
            chkRequisitos.Checked = false;
            for (int i = 0; i < clbCursos.Items.Count; i++) clbCursos.SetItemChecked(i, false);
            txtDuracion.Clear();
            idSeleccionado = 0; ciOriginal = null;
        }
    }
}