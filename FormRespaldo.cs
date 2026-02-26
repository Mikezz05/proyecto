#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SistemaPintoSalinas
{
    public class FormRespaldo : Form
    {
        private TextBox txtRuta;
        private Button btnExaminar, btnRespaldar;

        public FormRespaldo()
        {
            this.Text = "Respaldo y Seguridad de Datos";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterScreen;


            this.Controls.Add(new Label() { Text = "Generar Copia de Seguridad (Backup)", Location = new Point(20, 20), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent });

            this.Controls.Add(new Label() { Text = "Ruta donde se guardará el respaldo:", Location = new Point(20, 70), AutoSize = true, BackColor = Color.Transparent });
            
            txtRuta = new TextBox() { Location = new Point(20, 95), Width = 320, ReadOnly = true };
            this.Controls.Add(txtRuta);

            btnExaminar = new Button() { Text = "Seleccionar Carpeta", Location = new Point(350, 93), Width = 120 };
            btnExaminar.Click += BtnExaminar_Click;
            this.Controls.Add(btnExaminar);

            btnRespaldar = new Button() { Text = "Ejecutar Respaldo Ahora", Location = new Point(20, 140), Width = 450, Height = 40, BackColor = Color.LightGreen };
            btnRespaldar.Click += BtnRespaldar_Click;
            this.Controls.Add(btnRespaldar);
        }

        private void BtnExaminar_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                if (fbd.ShowDialog() == DialogResult.OK) {
                    string fechaStr = DateTime.Now.ToString("dd-MM-yyyy_HH-mm");
                    txtRuta.Text = Path.Combine(fbd.SelectedPath, $"Respaldo_PintoSalinas_{fechaStr}.sql");
                }
            }
        }

        private void BtnRespaldar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtRuta.Text)) { MessageBox.Show("Seleccione una ruta primero."); return; }

            try {
                string mysqldump = FindMySqlDump();
                if (string.IsNullOrEmpty(mysqldump) || !File.Exists(mysqldump)) {
                    MessageBox.Show("No se encontró 'mysqldump.exe'. Por favor instala MySQL o selecciona el ejecutable manualmente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = mysqldump;
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = false;
                psi.Arguments = $"-u root escuela_pinto_salinas -r \"{txtRuta.Text}\"";
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using (Process proc = Process.Start(psi)) {
                    proc.WaitForExit();
                }

                try { AuditHelper.Log("Respaldo", "CrearBackup", $"archivo={txtRuta.Text}; usuario={SessionManager.CurrentUser}"); } catch {}
                MessageBox.Show("¡Respaldo generado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) {
                MessageBox.Show("Error al generar respaldo. " + ex.Message, "Error");
            }
        }

        private string FindMySqlDump()
        {
            try {
                string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                foreach (var dir in pathEnv.Split(';')) {
                    try {
                        if (string.IsNullOrWhiteSpace(dir)) continue;
                        string candidate = Path.Combine(dir.Trim('"'), "mysqldump.exe");
                        if (File.Exists(candidate)) return candidate;
                    } catch { }
                }

                var commonRoots = new[] {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Path.Combine("C:", "xampp"),
                    Path.Combine("C:", "wamp64")
                };

                foreach (var root in commonRoots) {
                    if (string.IsNullOrWhiteSpace(root)) continue;
                    try {
                        string[] candidates = new string[] {
                            Path.Combine(root, "mysql", "bin", "mysqldump.exe"),
                            Path.Combine(root, "MySQL", "MySQL Server 8.0", "bin", "mysqldump.exe"),
                            Path.Combine(root, "MySQL", "MySQL Server 5.7", "bin", "mysqldump.exe"),
                            Path.Combine(root, "MariaDB", "bin", "mysqldump.exe"),
                            Path.Combine(root, "bin", "mysqldump.exe")
                        };
                        foreach (var c in candidates) if (File.Exists(c)) return c;

                        if (Directory.Exists(root)) {
                            foreach (var d1 in Directory.EnumerateDirectories(root)) {
                                try {
                                    string p1 = Path.Combine(d1, "bin", "mysqldump.exe");
                                    if (File.Exists(p1)) return p1;
                                    foreach (var d2 in Directory.EnumerateDirectories(d1)) {
                                        try {
                                            string p2 = Path.Combine(d2, "bin", "mysqldump.exe");
                                            if (File.Exists(p2)) return p2;
                                        } catch { }
                                    }
                                } catch { }
                            }
                        }
                    } catch { }
                }

                if (MessageBox.Show("No se encontró 'mysqldump.exe' automáticamente. ¿Deseas buscarlo manualmente?", "Seleccionar mysqldump", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog()) {
                        ofd.Filter = "mysqldump.exe|mysqldump.exe|Todos los archivos|*.*";
                        if (ofd.ShowDialog() == DialogResult.OK) return ofd.FileName;
                    }
                }
            } catch { }
            return null;
        }
    }
}