#nullable disable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormSerialManagement : Form
    {
        private DataGridView dgv;
        private DataGridView dgvHistory;
        private Button btnRefresh, btnApprove, btnClose;
        private Button btnRevoke, btnRegenerate;
        // filtros, export y paginación
        private TextBox txtFilterUser;
        private ComboBox cmbFilterAction;
        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private Button btnApplyFilters, btnClearFilters, btnExportCsv;
        private Button btnPrevPage, btnNextPage;
        private Label lblPageInfo;
        private int currentPage = 0;
        private int pageSize = 50;

        public FormSerialManagement()
        {
            InitializeComponents();
            LoadPending();
            LoadHistory();
        }

        private void InitializeComponents()
        {
            this.Text = "Aprobación de Cuentas - Generación de Serials";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            dgv = new DataGridView() { Location = new Point(10, 10), Size = new Size(760, 160), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            this.Controls.Add(dgv);
            // filtros entre las tablas
            txtFilterUser = new TextBox() { Location = new Point(10, 180), Width = 180, PlaceholderText = "Usuario (contiene)" };
            this.Controls.Add(txtFilterUser);

            cmbFilterAction = new ComboBox() { Location = new Point(200, 180), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterAction.Items.AddRange(new string[] { "", "Approve", "Revoke", "Regenerate" });
            this.Controls.Add(cmbFilterAction);

            dtFrom = new DateTimePicker() { Location = new Point(370, 180), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(-30) };
            this.Controls.Add(dtFrom);
            dtTo = new DateTimePicker() { Location = new Point(530, 180), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            this.Controls.Add(dtTo);

            btnApplyFilters = new Button() { Text = "Aplicar filtros", Location = new Point(690, 176), Size = new Size(80, 28) };
            btnApplyFilters.Click += (s, e) => { currentPage = 0; LoadHistory(); };
            this.Controls.Add(btnApplyFilters);

            btnClearFilters = new Button() { Text = "Limpiar", Location = new Point(690, 206), Size = new Size(80, 28) };
            btnClearFilters.Click += (s, e) => { txtFilterUser.Text = ""; cmbFilterAction.SelectedIndex = 0; dtFrom.Value = DateTime.Now.AddDays(-30); dtTo.Value = DateTime.Now; currentPage = 0; LoadHistory(); };
            this.Controls.Add(btnClearFilters);

            dgvHistory = new DataGridView() { Location = new Point(10, 220), Size = new Size(760, 150), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            this.Controls.Add(dgvHistory);

            btnPrevPage = new Button() { Text = "Anterior", Location = new Point(10, 380), Size = new Size(90, 28) };
            btnPrevPage.Click += (s, e) => { if (currentPage > 0) { currentPage--; LoadHistory(); } };
            this.Controls.Add(btnPrevPage);

            btnNextPage = new Button() { Text = "Siguiente", Location = new Point(110, 380), Size = new Size(90, 28) };
            btnNextPage.Click += (s, e) => { currentPage++; LoadHistory(); };
            this.Controls.Add(btnNextPage);

            lblPageInfo = new Label() { Text = "Página 1", Location = new Point(210, 380), AutoSize = true };
            this.Controls.Add(lblPageInfo);

            btnRefresh = new Button() { Text = "Actualizar", Location = new Point(10, 380), Size = new Size(120, 36) };
            btnRefresh.Click += (s, e) => { LoadPending(); LoadHistory(); };
            this.Controls.Add(btnRefresh);

            btnApprove = new Button() { Text = "Aprobar y Generar Serial", Location = new Point(140, 380), Size = new Size(180, 36) };
            btnApprove.Click += BtnApprove_Click;
            this.Controls.Add(btnApprove);

            btnRegenerate = new Button() { Text = "Regenerar Serial", Location = new Point(330, 380), Size = new Size(140, 36) };
            btnRegenerate.Click += BtnRegenerate_Click;
            this.Controls.Add(btnRegenerate);

            btnRevoke = new Button() { Text = "Revocar Serial", Location = new Point(480, 380), Size = new Size(140, 36) };
            btnRevoke.Click += BtnRevoke_Click;
            this.Controls.Add(btnRevoke);

            btnExportCsv = new Button() { Text = "Exportar CSV", Location = new Point(480, 420), Size = new Size(140, 28) };
            btnExportCsv.Click += BtnExportCsv_Click;
            this.Controls.Add(btnExportCsv);

            btnClose = new Button() { Text = "Cerrar", Location = new Point(660, 380), Size = new Size(110, 36) };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadPending()
        {
            try {
                ConexionDB db = new ConexionDB();
                using var conn = db.GetConnection();
                conn.Open();
                string q = "SELECT u.id, u.usuario, u.email, r.nombre AS rol, IFNULL(u.is_role_approved,0) AS is_role_approved FROM usuarios u JOIN roles r ON u.role_id=r.id WHERE (r.nombre='Docente' OR r.nombre='Admin') ORDER BY u.id";
                using var da = new MySqlDataAdapter(q, conn);
                var dt = new DataTable();
                da.Fill(dt);
                dgv.DataSource = dt;
            } catch (Exception ex) {
                MessageBox.Show("Error cargando cuentas pendientes: " + ex.Message);
            }
        }

        private void LoadHistory()
        {
            try {
                ConexionDB db = new ConexionDB();
                using var conn = db.GetConnection();
                conn.Open();
                // construir consulta con filtros y paginación
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("SELECT au.id AS audit_id, au.usuario, aa.nombre AS accion, ma.nombre AS modulo, au.detalles, au.created_at FROM auditoria au JOIN acciones_auditoria aa ON au.accion_id=aa.id JOIN modulos_auditoria ma ON au.modulo_id=ma.id WHERE ma.nombre='Seriales'");
                var cmd = new MySqlCommand();
                if (!string.IsNullOrWhiteSpace(txtFilterUser.Text)) {
                    sb.Append(" AND au.usuario LIKE @uf");
                    cmd.Parameters.AddWithValue("@uf", "%" + txtFilterUser.Text.Trim() + "%");
                }
                if (cmbFilterAction.SelectedItem != null && !string.IsNullOrWhiteSpace(cmbFilterAction.SelectedItem.ToString())) {
                    sb.Append(" AND aa.nombre = @act");
                    cmd.Parameters.AddWithValue("@act", cmbFilterAction.SelectedItem.ToString());
                }
                // intentar filtrar por created_at si existe
                sb.Append(" ORDER BY au.id DESC LIMIT @limit OFFSET @offset");
                int limit = pageSize;
                int offset = currentPage * pageSize;
                cmd.Parameters.AddWithValue("@limit", limit);
                cmd.Parameters.AddWithValue("@offset", offset);

                cmd.CommandText = sb.ToString();
                cmd.Connection = conn;
                using var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                dgvHistory.DataSource = dt;

                // actualizar etiqueta de página
                lblPageInfo.Text = $"Página {currentPage + 1} (Registros: {dt.Rows.Count})";
            } catch {
                try { dgvHistory.DataSource = null; } catch { }
            }
        }

        private DataTable ApplyHistoryFilters(DataTable source)
        {
            if (source == null) return source;
            string userFilter = txtFilterUser.Text.Trim();
            string actionFilter = cmbFilterAction.SelectedItem == null ? "" : cmbFilterAction.SelectedItem.ToString();
            DateTime from = dtFrom.Value.Date;
            DateTime to = dtTo.Value.Date.AddDays(1).AddSeconds(-1);

            DataTable dt = source.Clone();
            foreach (DataRow r in source.Rows)
            {
                bool ok = true;
                if (!string.IsNullOrEmpty(userFilter))
                {
                    var u = r.Table.Columns.Contains("usuario") && r["usuario"] != DBNull.Value ? r["usuario"].ToString() : "";
                    if (!u.Contains(userFilter, StringComparison.OrdinalIgnoreCase)) ok = false;
                }
                if (ok && !string.IsNullOrEmpty(actionFilter))
                {
                    var a = r.Table.Columns.Contains("accion") && r["accion"] != DBNull.Value ? r["accion"].ToString() : "";
                    if (!a.Equals(actionFilter, StringComparison.OrdinalIgnoreCase)) ok = false;
                }
                if (ok)
                {
                    // intentar filtrar por columna de fecha común si existe
                    DateTime rowDate = DateTime.MinValue;
                    if (r.Table.Columns.Contains("created_at") && r["created_at"] != DBNull.Value)
                    {
                        DateTime.TryParse(r["created_at"].ToString(), out rowDate);
                    }
                    else if (r.Table.Columns.Contains("fecha") && r["fecha"] != DBNull.Value)
                    {
                        DateTime.TryParse(r["fecha"].ToString(), out rowDate);
                    }
                    if (rowDate != DateTime.MinValue)
                    {
                        if (rowDate < from || rowDate > to) ok = false;
                    }
                }
                if (ok) dt.ImportRow(r);
            }
            return dt;
        }

        private void BtnRevoke_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Seleccione una cuenta."); return; }
            var row = dgv.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["id"].Value);
            string usuario = row.Cells["usuario"].Value.ToString();
            var confirm = MessageBox.Show($"Revocar serial y desaprobar la cuenta {usuario}?", "Confirmar revocación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            try {
                ConexionDB db = new ConexionDB();
                using var conn = db.GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();
                try {
                    using var del = new MySqlCommand("DELETE FROM seriales WHERE usuario_id=@uid", conn, tx);
                    del.Parameters.AddWithValue("@uid", userId);
                    del.ExecuteNonQuery();

                    using var up = new MySqlCommand("UPDATE usuarios SET is_role_approved=0 WHERE id=@id", conn, tx);
                    up.Parameters.AddWithValue("@id", userId);
                    up.ExecuteNonQuery();

                    tx.Commit();
                    try { AuditHelper.Log("Seriales", "Revoke", $"usuario={usuario}; userId={userId}; revokedBy={SessionManager.CurrentUser}"); } catch { }
                    MessageBox.Show("Serial revocado y cuenta marcada como pendiente.");
                    LoadPending();
                    LoadHistory();
                } catch (Exception exTx) { try { tx.Rollback(); } catch { } MessageBox.Show("Error revocando: " + exTx.Message); }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnRegenerate_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Seleccione una cuenta."); return; }
            var row = dgv.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["id"].Value);
            string usuario = row.Cells["usuario"].Value.ToString();
            var confirm = MessageBox.Show($"Regenerar serial para {usuario}? Esto invalidará el serial anterior.", "Confirmar regeneración", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            try {
                ConexionDB db = new ConexionDB();
                using var conn = db.GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();
                try {
                    string serial = SerialHelper.GenerateSerial();
                    string hash = SerialHelper.HashSerial(serial);
                    using var up = new MySqlCommand("UPDATE seriales SET serial_hash=@h WHERE usuario_id=@uid", conn, tx);
                    up.Parameters.AddWithValue("@uid", userId);
                    up.Parameters.AddWithValue("@h", hash);
                    int affected = up.ExecuteNonQuery();
                    if (affected == 0) {
                        using var ins = new MySqlCommand("INSERT INTO seriales (usuario_id, serial_hash) VALUES (@uid,@h)", conn, tx);
                        ins.Parameters.AddWithValue("@uid", userId);
                        ins.Parameters.AddWithValue("@h", hash);
                        ins.ExecuteNonQuery();
                    }
                    using var approve = new MySqlCommand("UPDATE usuarios SET is_role_approved=1 WHERE id=@id", conn, tx);
                    approve.Parameters.AddWithValue("@id", userId);
                    approve.ExecuteNonQuery();

                    tx.Commit();
                    try { AuditHelper.Log("Seriales", "Regenerate", $"usuario={usuario}; userId={userId}; regeneratedBy={SessionManager.CurrentUser}"); } catch { }
                    MessageBox.Show($"Nuevo serial generado:\n\n{serial}\n\nEntregue este serial al usuario. Se almacenó sólo su hash.", "Serial Regenerado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPending();
                    LoadHistory();
                } catch (Exception exTx) { try { tx.Rollback(); } catch { } MessageBox.Show("Error regenerando: " + exTx.Message); }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Seleccione una cuenta para aprobar."); return; }
            var row = dgv.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["id"].Value);
            string usuario = row.Cells["usuario"].Value.ToString();
            try {
                ConexionDB db = new ConexionDB();
                using var conn = db.GetConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();
                try {
                    // generar serial
                    string serial = SerialHelper.GenerateSerial();
                    string hash = SerialHelper.HashSerial(serial);
                    using var ins = new MySqlCommand("INSERT INTO seriales (usuario_id, serial_hash) VALUES (@uid,@h)", conn, tx);
                    ins.Parameters.AddWithValue("@uid", userId);
                    ins.Parameters.AddWithValue("@h", hash);
                    ins.ExecuteNonQuery();

                    using var up = new MySqlCommand("UPDATE usuarios SET is_role_approved=1 WHERE id=@id", conn, tx);
                    up.Parameters.AddWithValue("@id", userId);
                    up.ExecuteNonQuery();

                    tx.Commit();

                    // auditar la aprobación
                    try { AuditHelper.Log("Seriales", "Approve", $"usuario={usuario}; userId={userId}; approvedBy={SessionManager.CurrentUser}"); } catch { }

                    MessageBox.Show($"Cuenta aprobada y serial generado:\n\n{serial}\n\nMuestre/entregue este serial al usuario. Se almacenó sólo su hash en la base de datos.", "Aprobado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadPending();
                    LoadHistory();
                } catch (Exception exTx) {
                    try { tx.Rollback(); } catch { }
                    MessageBox.Show("Error aprobando cuenta: " + exTx.Message);
                }
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            try {
                DataTable dt = dgvHistory.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0) { MessageBox.Show("No hay datos para exportar."); return; }
                using SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV files (*.csv)|*.csv", FileName = "serials_history.csv" };
                if (sfd.ShowDialog() != DialogResult.OK) return;
                using var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8);
                // cabeceras
                for (int i = 0; i < dt.Columns.Count; i++) {
                    if (i > 0) sw.Write(',');
                    sw.Write('"');
                    sw.Write(dt.Columns[i].ColumnName.Replace("\"", "\"\""));
                    sw.Write('"');
                }
                sw.WriteLine();
                foreach (DataRow r in dt.Rows) {
                    for (int i = 0; i < dt.Columns.Count; i++) {
                        if (i > 0) sw.Write(',');
                        var v = r[i] == DBNull.Value ? "" : r[i].ToString();
                        v = v.Replace("\"", "\"\"");
                        sw.Write('"'); sw.Write(v); sw.Write('"');
                    }
                    sw.WriteLine();
                }
                MessageBox.Show("Exportación completada.", "CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) { MessageBox.Show("Error exportando CSV: " + ex.Message); }
        }
    }
}
