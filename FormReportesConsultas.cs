#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace SistemaPintoSalinas
{
    public class FormReportesConsultas : Form
    {
        private ComboBox cmbConsultas, cmbReportes;
        private Button btnEjecutarConsulta, btnGenerarReporte;
        private DataGridView dgvResultados;

        public FormReportesConsultas()
        {
            this.Text = "Centro de Consultas y Reportes Oficiales";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;


            // PANEL IZQUIERDO: LAS 7 CONSULTAS VISUALES
            GroupBox grpConsultas = new GroupBox() { Text = "Módulo de Consultas (7 Características)", Location = new Point(20, 20), Size = new Size(450, 100), BackColor = Color.Transparent };
            cmbConsultas = new ComboBox() { Location = new Point(20, 30), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbConsultas.Items.AddRange(new string[] {
                "1. Consulta de Alumnos inscritos por Curso",
                "2. Cuadro de Honor (Promedios mayores a 15)",
                "3. Agenda de Actividades Planificadas",
                "4. Docentes y su Carga Académica",
                "5. Estudiantes con Requisitos Pendientes",
                "6. Estadísticas Generales de Notas",
                "7. Cursos Sin Docente Asignado"
            });
            grpConsultas.Controls.Add(cmbConsultas);
            btnEjecutarConsulta = new Button() { Text = "Ver Consulta en Pantalla", Location = new Point(20, 60), Width = 400, BackColor = Color.LightSkyBlue };
            btnEjecutarConsulta.Click += BtnEjecutarConsulta_Click;
            grpConsultas.Controls.Add(btnEjecutarConsulta);
            this.Controls.Add(grpConsultas);

            // PANEL DERECHO: LOS 7 REPORTES EXPORTABLES
            GroupBox grpReportes = new GroupBox() { Text = "Módulo de Reportes Documentales (7 Características)", Location = new Point(500, 20), Size = new Size(450, 100), BackColor = Color.Transparent };
            cmbReportes = new ComboBox() { Location = new Point(20, 30), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportes.Items.AddRange(new string[] {
                "1. Nómina General de Matrícula",
                "2. Listado de Docentes Activos",
                "3. Reporte de Auditoría de Seguridad",
                "4. Calendario Académico Oficial",
                "5. Récord Académico de Reprobados",
                "6. Constancia de Inscripción General",
                "7. Reporte de Inventario de Cursos"
            });
            grpReportes.Controls.Add(cmbReportes);
            btnGenerarReporte = new Button() { Text = "Exportar Reporte a Archivo", Location = new Point(20, 60), Width = 400, BackColor = Color.LightGreen };
            btnGenerarReporte.Click += BtnGenerarReporte_Click;
            grpReportes.Controls.Add(btnGenerarReporte);
            this.Controls.Add(grpReportes);

            // TABLA CENTRAL DE RESULTADOS
            dgvResultados = new DataGridView() { Location = new Point(20, 140), Size = new Size(930, 450), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            this.Controls.Add(dgvResultados);
        }

        private void BtnEjecutarConsulta_Click(object sender, EventArgs e)
        {
            if (cmbConsultas.SelectedIndex == -1) return;
            string q = "";
            int seleccion = cmbConsultas.SelectedIndex;

            switch (seleccion) {
                case 0: q = "SELECT c.nombre_curso AS Curso, e.ci AS Cedula, e.nombres AS Nombres, e.apellidos AS Apellidos FROM estudiantes e INNER JOIN cursos c ON e.curso_id = c.id ORDER BY c.nombre_curso"; break;
                case 1: q = "SELECT e.nombres, e.apellidos, c.nombre_curso, cal.nota_final FROM calificaciones cal JOIN estudiantes e ON cal.estudiante_id = e.id JOIN cursos c ON cal.curso_id = c.id WHERE cal.nota_final >= 15 ORDER BY cal.nota_final DESC"; break;
                case 2: q = "SELECT fecha, tipo_actividad, descripcion FROM actividades_calendario ORDER BY fecha"; break;
                case 3: q = "SELECT u.nombre_completo AS Docente, c.nombre_curso AS Asignatura, c.duracion_meses AS Duracion FROM cursos c JOIN usuarios u ON c.docente_id = u.id WHERE u.rol = 'Docente'"; break;
                case 4: q = "SELECT ci, nombres, apellidos, telefono FROM estudiantes WHERE requisitos_verificados = 0"; break;
                case 5: q = "SELECT c.nombre_curso AS Curso, AVG(cal.nota_final) AS Promedio_General, SUM(cal.aprobado) AS Total_Aprobados FROM calificaciones cal JOIN cursos c ON cal.curso_id = c.id GROUP BY c.nombre_curso"; break;
                case 6: q = "SELECT id, nombre_curso, duracion_meses FROM cursos WHERE docente_id IS NULL OR docente_id = 0"; break;
            }

            CargarDataGrid(q);
            try { AuditHelper.Log("Reportes", "Consulta", $"consulta={cmbConsultas.Text}"); } catch {}
        }

        private void CargarDataGrid(string query)
        {
            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvResultados.DataSource = dt;
                }
            } catch (Exception ex) { MessageBox.Show("No se encontró data para esta consulta. " + ex.Message); }
        }

        private DataTable ObtenerDatosReporte()
        {
            string q = "";
            switch (cmbReportes.SelectedIndex)
            {
                case 0: q = "SELECT e.ci AS Cedula, e.nombres AS Nombres, e.apellidos AS Apellidos, c.nombre_curso AS Curso, e.fecha_inscripcion AS Fecha FROM estudiantes e LEFT JOIN cursos c ON e.curso_id = c.id"; break;
                case 1: q = "SELECT nombre_completo AS Docente, usuario, rol FROM usuarios WHERE rol='Docente'"; break;
                case 2: q = "SELECT fecha, usuario, accion, modulo FROM auditoria ORDER BY fecha DESC"; break;
                case 3: q = "SELECT fecha, tipo_actividad, descripcion FROM actividades_calendario ORDER BY fecha"; break;
                case 4: q = "SELECT e.ci AS Cedula, e.nombres, c.nombre_curso, cal.nota_final FROM calificaciones cal JOIN estudiantes e ON cal.estudiante_id=e.id JOIN cursos c ON cal.curso_id=c.id WHERE cal.aprobado=0"; break;
                case 5: q = "SELECT CONCAT('CONSTANCIA DE INSCRIPCION\n\nEl estudiante ', nombres, ' ', apellidos, ' con CI: ', ci, ' se encuentra inscrito.') AS Documento FROM estudiantes"; break;
                case 6: q = "SELECT CONCAT('REPORTE DE CURSO\nCurso: ', nombre_curso, ' | Duración: ', duracion_meses, ' meses.') AS Documento FROM cursos"; break;
            }

            if (string.IsNullOrEmpty(q)) return null;

            try {
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection()) {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(q, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Error al ejecutar la consulta:\n{ex.Message}", "Error al generar el reporte", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void BtnGenerarReporte_Click(object sender, EventArgs e)
        {
            if (cmbReportes.SelectedIndex == -1) { MessageBox.Show("Seleccione un reporte"); return; }
            DataTable dt = ObtenerDatosReporte(); 
            if (dt == null || dt.Rows.Count == 0) { MessageBox.Show("No hay datos para exportar"); return; }

            string nombreBase = cmbReportes.SelectedItem.ToString();
            foreach (char c in Path.GetInvalidFileNameChars()) nombreBase = nombreBase.Replace(c, '_');

            string nombrePorDefecto = $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Documento PDF (*.pdf)|*.pdf", FileName = nombrePorDefecto, Title = "Guardar reporte" }) {
                if (sfd.ShowDialog() == DialogResult.OK) {
                    ExportarPDF(dt, sfd.FileName);
                    try { AuditHelper.Log("Reportes", "Exportar", $"reporte={cmbReportes.SelectedItem}; archivo={sfd.FileName}"); } catch {}
                }
            }
        }

        private void ExportarPDF(DataTable dt, string filename)
        {
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Reporte - " + DateTime.Now.ToString("dd/MM/yyyy");

            PdfPage page = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont fontNormal = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontBold   = new XFont("Arial", 10, XFontStyle.Bold);

            double y = 40;
            double marginLeft = 40;
            double marginRight = 40;

            double logoWidth = 0, logoHeight = 0;
            try {
                XImage logo = XImage.FromFile("logo.png");
                logoWidth = 80;
                logoHeight = 80;
                gfx.DrawImage(logo, marginLeft, y, logoWidth, logoHeight);
            } catch { }

            double textStart = marginLeft + (logoWidth > 0 ? logoWidth + 10 : 0);
            string[] headerLines = new[] {
                "República Bolivariana de Venezuela",
                "Ministerio del Poder Popular para la Educación Universitaria",
                "Escuela de Emprendedores Antonio Pinto Salinas",
                "Maracay, Edo. Aragua.",
                $"Fecha: {DateTime.Now:dd/MM/yyyy}" };
            
            foreach (var line in headerLines) {
                var size = gfx.MeasureString(line, fontBold);
                double xPos = (logoWidth > 0) ? textStart : (page.Width - size.Width) / 2;
                gfx.DrawString(line, fontBold, XBrushes.Black, new XPoint(xPos, y));
                y += size.Height + 5;
            }
            y += 15; 

            double usableWidth = page.Width - marginLeft - marginRight;
            int totalCols = dt.Columns.Count;
            int dateCols = 0;
            foreach (DataColumn c in dt.Columns) if (c.DataType == typeof(DateTime) || c.ColumnName.ToLower().Contains("fecha")) dateCols++;

            double dateColWidth = 120; 
            double otherColMin = 80;
            double otherCols = Math.Max(1, totalCols - dateCols);
            double remainingWidth = usableWidth - (dateCols * dateColWidth);
            if (remainingWidth < otherCols * otherColMin) {
                dateColWidth = Math.Max(90, (usableWidth - otherCols * otherColMin) / Math.Max(1, dateCols));
                remainingWidth = usableWidth - (dateCols * dateColWidth);
            }
            double otherColWidth = Math.Max(otherColMin, remainingWidth / otherCols);

            double[] colWidths = new double[totalCols];
            for (int i = 0; i < totalCols; i++) {
                var c = dt.Columns[i];
                colWidths[i] = (c.DataType == typeof(DateTime) || c.ColumnName.ToLower().Contains("fecha")) ? dateColWidth : otherColWidth;
            }

            double x = marginLeft;
            for (int i = 0; i < totalCols; i++) {
                gfx.DrawString(dt.Columns[i].ColumnName, fontBold, XBrushes.Black, new XRect(x, y, colWidths[i], 20), XStringFormats.TopLeft);
                x += colWidths[i];
            }
            y += 20;

            foreach (DataRow row in dt.Rows) {
                x = marginLeft;
                for (int ci = 0; ci < totalCols; ci++) {
                    DataColumn col = dt.Columns[ci];
                    double thisColWidth = colWidths[ci];
                    string text;
                    object val = row[col];
                    if (val == DBNull.Value) text = string.Empty;
                    else if (col.DataType == typeof(DateTime) || col.ColumnName.ToLower().Contains("fecha")) {
                        string raw = val.ToString();
                        DateTime dtVal;
                        if (DateTime.TryParse(raw, out dtVal)) {
                            string timeText = dtVal.ToString("dd/MM/yyyy hh:mm");
                            string suf = dtVal.ToString("tt").ToLower();
                            string ampm = suf.Contains("p") ? "p.m" : "a.m";
                            string combined = timeText + " " + ampm;
                            gfx.DrawString(combined, fontNormal, XBrushes.Black, new XRect(x + 2, y, thisColWidth - 4, 16), XStringFormats.TopLeft);
                            x += thisColWidth;
                            continue;
                        } else {
                            string cleaned = Regex.Replace(raw, "\\s*(a\\.?m\\.?|p\\.?m\\.?|am|pm)\\b", "", RegexOptions.IgnoreCase);
                            text = cleaned.Trim();
                        }
                    } else text = val.ToString();

                    gfx.DrawString(text, fontNormal, XBrushes.Black, new XRect(x + 4, y, thisColWidth - 6, 16), XStringFormats.TopLeft);
                    x += thisColWidth;
                }
                y += 16;
                if (y > page.Height - 100) {
                    page = pdf.AddPage();
                    gfx  = XGraphics.FromPdfPage(page);
                    y    = 40;
                    foreach (var line in headerLines) {
                        var size = gfx.MeasureString(line, fontBold);
                        double xPos = (page.Width - size.Width) / 2;
                        gfx.DrawString(line, fontBold, XBrushes.Black, new XPoint(xPos, y));
                        y += size.Height + 5;
                    }
                    y += 15;
                    x = marginLeft;
                    for (int i = 0; i < totalCols; i++) {
                        gfx.DrawString(dt.Columns[i].ColumnName, fontBold, XBrushes.Black, new XRect(x, y, colWidths[i], 20), XStringFormats.TopLeft);
                        x += colWidths[i];
                    }
                    y += 20;
                }
            }

            y = page.Height - 80;
            gfx.DrawString("_____________________________", fontNormal, XBrushes.Black, new XPoint(40, y));
            gfx.DrawString("Firma de la Institución", fontNormal, XBrushes.Black, new XPoint(40, y + 15));

            pdf.Save(filename);
        }
    }
}