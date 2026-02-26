#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;

namespace SistemaPintoSalinas
{
    public class FormVistaEstudiante : Form
    {
        private TextBox txtCedula;
        private Button btnBuscar;
        private DataGridView dgvNotas;
        private Label lblNombre;

        public FormVistaEstudiante()
        {
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Portal Estudiantil - Consulta de Notas";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- Encabezado ---
            Label lblTitulo = new Label() { Text = "Historial Académico", Location = new Point(20, 20), Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true, ForeColor = Color.DarkBlue };
            this.Controls.Add(lblTitulo);

            // --- Panel de Búsqueda ---
            GroupBox grpBuscar = new GroupBox() { Text = "Mis Datos", Location = new Point(20, 60), Size = new Size(940, 80) };
            
            grpBuscar.Controls.Add(new Label() { Text = "Ingrese su Cédula:", Location = new Point(20, 30), AutoSize = true });
            
            txtCedula = new TextBox() { Location = new Point(130, 28), Width = 150 };
            grpBuscar.Controls.Add(txtCedula);

            btnBuscar = new Button() { Text = "Ver Mis Calificaciones", Location = new Point(300, 26), Width = 180, BackColor = Color.LightBlue };
            btnBuscar.Click += BtnBuscar_Click;
            grpBuscar.Controls.Add(btnBuscar);

            this.Controls.Add(grpBuscar);

            // --- Datos del Estudiante Encontrado ---
            lblNombre = new Label() { Text = "Estudiante: -", Location = new Point(30, 160), Font = new Font("Arial", 11, FontStyle.Bold) };
            this.Controls.Add(lblNombre);

            // --- Tabla de Resultados ---
            dgvNotas = new DataGridView() { Location = new Point(20, 200), Size = new Size(940, 340), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            this.Controls.Add(dgvNotas);

        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text)) {
                MessageBox.Show("Por favor escriba su cédula.");
                return;
            }

            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    
                    // PASO 1: Verificar si el estudiante existe y traer su nombre
                    // Usamos la cédula (ci) para buscar al estudiante
                    string queryEstudiante = "SELECT CONCAT(nombres, ' ', apellidos) FROM estudiantes WHERE ci = @ci";
                    MySqlCommand cmdEst = new MySqlCommand(queryEstudiante, conn);
                    cmdEst.Parameters.AddWithValue("@ci", txtCedula.Text);
                    
                    object resultadoNombre = cmdEst.ExecuteScalar();

                    if (resultadoNombre != null) {
                        lblNombre.Text = "Estudiante: " + resultadoNombre.ToString().ToUpper();
                        
                        // PASO 2: Traer las notas vinculadas a ese estudiante
                        // Hacemos JOIN entre calificaciones, cursos y estudiantes
                        string queryNotas = 
                            "SELECT " +
                            "   cur.nombre_curso AS 'Materia', " +
                            "   cal.desc1 AS 'Evaluación 1', cal.nota1 AS 'Nota 1', " +
                            "   cal.desc2 AS 'Evaluación 2', cal.nota2 AS 'Nota 2', " +
                            "   cal.desc3 AS 'Evaluación 3', cal.nota3 AS 'Nota 3', " +
                            "   cal.nota_final AS 'Definitiva', " +
                            "   CASE WHEN cal.aprobado = 1 THEN 'APROBADO' ELSE 'REPROBADO' END AS 'Estatus' " +
                            "FROM calificaciones cal " +
                            "INNER JOIN estudiantes est ON cal.estudiante_id = est.id " +
                            "INNER JOIN cursos cur ON cal.curso_id = cur.id " +
                            "WHERE est.ci = @ci"; // <--- AQUÍ ESTÁ LA VINCULACIÓN MAGICA

                        MySqlCommand cmdNotas = new MySqlCommand(queryNotas, conn);
                        cmdNotas.Parameters.AddWithValue("@ci", txtCedula.Text);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmdNotas);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0) {
                            dgvNotas.DataSource = dt;
                        } else {
                            dgvNotas.DataSource = null;
                            MessageBox.Show("El estudiante existe, pero aún no tiene notas cargadas por los docentes.");
                        }

                    } else {
                        MessageBox.Show("Cédula no encontrada en el sistema de inscripciones.");
                        lblNombre.Text = "Estudiante: -";
                        dgvNotas.DataSource = null;
                    }
                }
            } catch (Exception ex) { 
                MessageBox.Show("Error al buscar: " + ex.Message); 
            }
        }

        // construye texto simple a partir de la cédula y la tabla de notas
    }
}