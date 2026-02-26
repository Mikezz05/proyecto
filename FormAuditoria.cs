#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormAuditoria : Form
    {
        private DateTimePicker dtpDesde, dtpHasta;
        private ComboBox cmbUsuarios, cmbAccion;
        private Button btnBuscar, btnLimpiar;
        private DataGridView dgvAuditoria;

        public FormAuditoria()
        {
            ConfigurarFormulario();


            CargarUsuarios();
            BuscarAuditoria(""); 
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Módulo de Auditoría del Sistema";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitulo = new Label() { Text = "Rastro de Auditoría", Location = new Point(20, 20), Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            this.Controls.Add(lblTitulo);

            this.Controls.Add(new Label() { Text = "Desde:", Location = new Point(20, 60), AutoSize = true, BackColor = Color.Transparent });
            dtpDesde = new DateTimePicker() { Location = new Point(20, 80), Format = DateTimePickerFormat.Short, Width = 120 };
            this.Controls.Add(dtpDesde);

            this.Controls.Add(new Label() { Text = "Hasta:", Location = new Point(150, 60), AutoSize = true, BackColor = Color.Transparent });
            dtpHasta = new DateTimePicker() { Location = new Point(150, 80), Format = DateTimePickerFormat.Short, Width = 120 };
            this.Controls.Add(dtpHasta);

            this.Controls.Add(new Label() { Text = "Usuario:", Location = new Point(280, 60), AutoSize = true, BackColor = Color.Transparent });
            cmbUsuarios = new ComboBox() { Location = new Point(280, 80), Width = 150, DropDownStyle = ComboBoxStyle.DropDown };
            cmbUsuarios.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbUsuarios.AutoCompleteSource = AutoCompleteSource.CustomSource;
            cmbUsuarios.KeyDown += CmbUsuarios_KeyDown;
            this.Controls.Add(cmbUsuarios);

            this.Controls.Add(new Label() { Text = "Acción:", Location = new Point(440, 60), AutoSize = true, BackColor = Color.Transparent });
            cmbAccion = new ComboBox() { Location = new Point(440, 80), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAccion.Items.AddRange(new string[] { "TODAS", "Insertar", "Actualizar", "Eliminar", "Login" });
            cmbAccion.SelectedIndex = 0;
            this.Controls.Add(cmbAccion);

            btnBuscar = new Button() { Text = "Aplicar Filtros", Location = new Point(610, 78), Width = 120, BackColor = Color.LightBlue };
            btnBuscar.Click += BtnBuscar_Click;
            this.Controls.Add(btnBuscar);

            btnLimpiar = new Button() { Text = "Ver Todo", Location = new Point(740, 78), Width = 100 };
            btnLimpiar.Click += (s, e) => { cmbUsuarios.SelectedIndex = 0; cmbAccion.SelectedIndex = 0; BuscarAuditoria(""); };
            this.Controls.Add(btnLimpiar);

            dgvAuditoria = new DataGridView() { Location = new Point(20, 130), Size = new Size(940, 400), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            this.Controls.Add(dgvAuditoria);
        }

        private void CargarUsuarios()
        {
            try {
                cmbUsuarios.Items.Add("TODOS");
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT usuario FROM auditoria", conn);
                    using (MySqlDataReader r = cmd.ExecuteReader()) {
                        System.Collections.Specialized.StringCollection ac = new System.Collections.Specialized.StringCollection();
                        while (r.Read()) {
                            string u = r["usuario"].ToString();
                            cmbUsuarios.Items.Add(u);
                            ac.Add(u);
                        }
                        cmbUsuarios.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                        cmbUsuarios.AutoCompleteCustomSource.AddRange(ac.Cast<string>().ToArray());
                    }
                }
                cmbUsuarios.SelectedIndex = 0;
            } catch { cmbUsuarios.Items.Add("TODOS"); cmbUsuarios.SelectedIndex = 0; }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string filtro = " WHERE fecha >= @d AND fecha <= @h ";
            if (!string.IsNullOrWhiteSpace(cmbUsuarios.Text) && cmbUsuarios.Text != "TODOS") filtro += " AND usuario = @user ";
            if (cmbAccion.SelectedIndex > 0) filtro += " AND accion = @accion ";
            BuscarAuditoria(filtro);
        }

        private void BuscarAuditoria(string condicionales)
        {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    string q = "SELECT fecha AS Fecha, usuario AS Usuario, accion AS Accion, modulo AS Modulo, detalles AS Detalles FROM auditoria " + condicionales + " ORDER BY fecha DESC";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    if (condicionales != "") {
                        cmd.Parameters.AddWithValue("@d", dtpDesde.Value.Date);
                        cmd.Parameters.AddWithValue("@h", dtpHasta.Value.Date.AddDays(1).AddSeconds(-1));
                        if (condicionales.Contains("@user")) cmd.Parameters.AddWithValue("@user", cmbUsuarios.Text.Trim());
                        if (condicionales.Contains("@accion")) cmd.Parameters.AddWithValue("@accion", cmbAccion.SelectedItem.ToString());
                    }
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvAuditoria.DataSource = dt;
                }
            } catch (Exception ex) { MessageBox.Show("Error al auditar: " + ex.Message); }
        }

        private void CmbUsuarios_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                BtnBuscar_Click(btnBuscar, EventArgs.Empty);
                e.Handled = true; e.SuppressKeyPress = true;
            }
        }
    }
}