#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Text.RegularExpressions;

namespace SistemaPintoSalinas
{
    public class FormCertificacion : Form
    {
        private ComboBox cmbEstudiantes;
        private ComboBox cmbCursosAprobados;
        private Button btnVerificar, btnGenerar, btnAgendar;
        private Label lblEstadoAdmin, lblEstadoAcad;
        private DateTimePicker dtpCeremonia;

        public FormCertificacion()
        {
            ConfigurarFormulario();


            CargarEstudiantes();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Gestión de Certificación y Egreso";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitulo = new Label() { Text = "Emisión de Certificados", Location = new Point(20, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            this.Controls.Add(lblTitulo);

            this.Controls.Add(new Label() { Text = "Seleccionar Estudiante:", Location = new Point(20, 60), AutoSize = true, BackColor = Color.Transparent });
            cmbEstudiantes = new ComboBox() { Location = new Point(20, 85), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEstudiantes.SelectedIndexChanged += (s, e) => CargarCursosDelEstudiante();
            this.Controls.Add(cmbEstudiantes);

            this.Controls.Add(new Label() { Text = "Seleccionar Curso para Certificar:", Location = new Point(300, 60), AutoSize = true, BackColor = Color.Transparent });
            cmbCursosAprobados = new ComboBox() { Location = new Point(300, 85), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbCursosAprobados);

            GroupBox grpVerificacion = new GroupBox() { Text = "Verificación de Cumplimiento", Location = new Point(20, 130), Size = new Size(640, 100), BackColor = Color.Transparent };
            
            btnVerificar = new Button() { Text = "Verificar Requisitos", Location = new Point(20, 30), Width = 150 };
            btnVerificar.Click += BtnVerificar_Click;
            grpVerificacion.Controls.Add(btnVerificar);

            lblEstadoAdmin = new Label() { Text = "Administrativo: Pendiente", Location = new Point(200, 35), AutoSize = true, ForeColor = Color.Gray };
            grpVerificacion.Controls.Add(lblEstadoAdmin);

            lblEstadoAcad = new Label() { Text = "Académico: Pendiente", Location = new Point(400, 35), AutoSize = true, ForeColor = Color.Gray };
            grpVerificacion.Controls.Add(lblEstadoAcad);

            this.Controls.Add(grpVerificacion);

            btnGenerar = new Button() { Text = "Generar Certificado Digital", Location = new Point(20, 250), Width = 250, Height = 40, BackColor = Color.LightSkyBlue, Enabled = false };
            btnGenerar.Click += BtnGenerar_Click;
            this.Controls.Add(btnGenerar);

            this.Controls.Add(new Label() { Text = "Fecha de Acto de Grado:", Location = new Point(300, 250), AutoSize = true, BackColor = Color.Transparent });
            dtpCeremonia = new DateTimePicker() { Location = new Point(300, 275), Format = DateTimePickerFormat.Short, Width = 120 };
            this.Controls.Add(dtpCeremonia);

            btnAgendar = new Button() { Text = "Agendar Ceremonia", Location = new Point(440, 273), Width = 150 };
            btnAgendar.Click += (s, e) => MessageBox.Show($"Ceremonia agendada para el {dtpCeremonia.Value.ToShortDateString()}. Se ha notificado al estudiante.");
            this.Controls.Add(btnAgendar);
        }

        private void CargarEstudiantes()
        {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                            // get one row per student (ci) to avoid duplicates
                    MySqlDataAdapter da = new MySqlDataAdapter(
                        "SELECT MIN(id) AS id, CONCAT(nombres, ' ', apellidos) AS nombre " +
                        "FROM estudiantes GROUP BY ci", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbEstudiantes.DisplayMember = "nombre";
                    cmbEstudiantes.ValueMember = "id";
                    cmbEstudiantes.DataSource = dt;
                }
            } catch {}
        }

        private void CargarCursosDelEstudiante()
        {
            if (cmbEstudiantes.SelectedValue == null) return;
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    // only list each course once, optionally ensure approved
                    string query = @"SELECT DISTINCT c.id, c.nombre_curso 
                                     FROM calificaciones cal
                                     JOIN cursos c ON cal.curso_id = c.id
                                     WHERE cal.estudiante_id = @estId AND cal.aprobado = 1";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@estId", cmbEstudiantes.SelectedValue);
                    
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbCursosAprobados.DisplayMember = "nombre_curso";
                    cmbCursosAprobados.ValueMember = "id";
                    cmbCursosAprobados.DataSource = dt;
                }
            } catch {}
        }

        private void BtnVerificar_Click(object sender, EventArgs e)
        {
            if (cmbEstudiantes.SelectedValue == null || cmbCursosAprobados.SelectedValue == null) return;

            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    
                    string qAdmin = "SELECT requisitos_verificados FROM estudiantes WHERE id = @estId";
                    MySqlCommand cmd1 = new MySqlCommand(qAdmin, conn);
                    cmd1.Parameters.AddWithValue("@estId", cmbEstudiantes.SelectedValue);
                    bool adminOk = Convert.ToBoolean(cmd1.ExecuteScalar());

                    string qAcad = "SELECT aprobado FROM calificaciones WHERE estudiante_id = @estId AND curso_id = @curId";
                    MySqlCommand cmd2 = new MySqlCommand(qAcad, conn);
                    cmd2.Parameters.AddWithValue("@estId", cmbEstudiantes.SelectedValue);
                    cmd2.Parameters.AddWithValue("@curId", cmbCursosAprobados.SelectedValue);
                    
                    object resAcad = cmd2.ExecuteScalar();
                    bool acadOk = (resAcad != null && Convert.ToBoolean(resAcad));

                    lblEstadoAdmin.Text = adminOk ? "Administrativo: COMPLETADO" : "Administrativo: FALTA DOCUMENTACIÓN";
                    lblEstadoAdmin.ForeColor = adminOk ? Color.Green : Color.Red;

                    lblEstadoAcad.Text = acadOk ? "Académico: APROBADO" : "Académico: REPROBADO O SIN NOTA";
                    lblEstadoAcad.ForeColor = acadOk ? Color.Green : Color.Red;

                    btnGenerar.Enabled = (adminOk && acadOk);
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            string alumno = cmbEstudiantes.Text;
            string curso = cmbCursosAprobados.Text;
            string fecha = DateTime.Now.ToShortDateString();

            // now create a PDF certificate instead of plain text
            string fileName = $"Certificado_{alumno.Replace(" ","_")}_{curso.Replace(" ","_")}.pdf";
            GenerarCertificadoPdf(alumno, curso, fecha, fileName);
            MessageBox.Show($"Certificado PDF generado exitosamente.\nGuardado como: {fileName}", "Sistema de Gestión");
            try { System.Diagnostics.Process.Start(fileName); } catch { }
        }

        private void GenerarCertificadoPdf(string alumno, string curso, string fecha, string outputPath)
        {
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Certificado - " + alumno;
            PdfPage page = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);

            double y = 40;
            double marginLeft = 40;
            double logoWidth = 80;
            double logoHeight = 80;
            try {
                XImage logo = XImage.FromFile("logo.png");
                gfx.DrawImage(logo, marginLeft, y, logoWidth, logoHeight);
            } catch { }

            double textStart = marginLeft + logoWidth + 10;
            string[] headerLines = new[] {
                "República Bolivariana de Venezuela",
                "Escuela de Emprendedores Antonio Pinto Salinas",
                "Certificado de Aprobación"
            };
            foreach (var line in headerLines) {
                var size = gfx.MeasureString(line, fontBold);
                gfx.DrawString(line, fontBold, XBrushes.Black, new XPoint(textStart, y + (logoHeight - size.Height) / 2));
                y += size.Height + 5;
            }
            y = 140;

            gfx.DrawString("Otorgado a:", fontNormal, XBrushes.Black, new XPoint(marginLeft, y));
            y += 25;
            gfx.DrawString(alumno.ToUpper(), fontTitle, XBrushes.Black, new XPoint(marginLeft, y));
            y += 40;
            gfx.DrawString("Por haber cumplido satisfactoriamente los requisitos académicos del curso:", fontNormal, XBrushes.Black, new XPoint(marginLeft, y));
            y += 25;
            gfx.DrawString(curso.ToUpper(), fontBold, XBrushes.Black, new XPoint(marginLeft, y));
            y += 50;
            gfx.DrawString("Fecha de Emisión: " + fecha, fontNormal, XBrushes.Black, new XPoint(marginLeft, y));
            y += 80;
            gfx.DrawString("_____________________________", fontNormal, XBrushes.Black, new XPoint(marginLeft, y));
            gfx.DrawString("Firma de la Dirección", fontNormal, XBrushes.Black, new XPoint(marginLeft, y + 15));

            pdf.Save(outputPath);
        }
    }
}