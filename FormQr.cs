#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;
using ZXing;
using QRCoder;

namespace SistemaPintoSalinas
{
    public class FormQr : Form
    {
        private TextBox txtInput;
        private Button btnGenerate, btnScan;
        private PictureBox picQr;

        public FormQr() : this(null) { }
        public FormQr(string prefill)
        {
            Initialize();
            if (!string.IsNullOrEmpty(prefill)) txtInput.Text = prefill;
        }

        private void Initialize()
        {
            this.Text = "Consulta de Notas por QR";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lbl = new Label() { Text = "Cédula / Usuario:", Location = new Point(20, 20) };
            this.Controls.Add(lbl);
            txtInput = new TextBox() { Location = new Point(130, 18), Width = 200 };
            txtInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) GenerateQr(); };
            this.Controls.Add(txtInput);

            btnGenerate = new Button() { Text = "Generar QR", Location = new Point(340, 16), Size = new Size(100, 25) };
            btnGenerate.Click += (s, e) => GenerateQr();
            this.Controls.Add(btnGenerate);

            btnScan = new Button() { Text = "Escanear imagen", Location = new Point(20, 55), Size = new Size(150, 25) };
            btnScan.Click += (s, e) => ScanFromFile();
            this.Controls.Add(btnScan);

            picQr = new PictureBox() { Location = new Point(200, 55), Size = new Size(250, 250), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            this.Controls.Add(picQr);
        }

        private void GenerateQr()
        {
            string input = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Ingrese cédula o usuario a consultar.");
                return;
            }
            // reuse logic from login form
            try
            {
                string ci = input;
                ConexionDB db = new ConexionDB();
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string qEst = "SELECT CONCAT(nombres,' ',apellidos) FROM estudiantes WHERE ci=@ci";
                    MySqlCommand cmdEst = new MySqlCommand(qEst, conn);
                    cmdEst.Parameters.AddWithValue("@ci", ci);
                    object nombreObj = cmdEst.ExecuteScalar();
                    string nombre = nombreObj == null ? null : nombreObj.ToString();
                    if (nombre == null)
                    {
                        string qUser = "SELECT ci FROM usuarios WHERE usuario=@u LIMIT 1";
                        MySqlCommand cmdUser = new MySqlCommand(qUser, conn);
                        cmdUser.Parameters.AddWithValue("@u", input);
                        object ciObj = cmdUser.ExecuteScalar();
                        if (ciObj != null && ciObj != DBNull.Value)
                        {
                            ci = ciObj.ToString();
                            cmdEst.Parameters.Clear();
                            cmdEst.Parameters.AddWithValue("@ci", ci);
                            nombreObj = cmdEst.ExecuteScalar();
                            nombre = nombreObj == null ? null : nombreObj.ToString();
                        }
                    }

                    if (nombre != null)
                    {
                        string qNotas = 
                            "SELECT cur.nombre_curso AS 'Materia', cal.nota_final AS 'Definitiva' " +
                            "FROM calificaciones cal " +
                            "INNER JOIN estudiantes est ON cal.estudiante_id = est.id " +
                            "INNER JOIN cursos cur ON cal.curso_id = cur.id " +
                            "WHERE est.ci = @ci";
                        MySqlCommand cmdNotas = new MySqlCommand(qNotas, conn);
                        cmdNotas.Parameters.AddWithValue("@ci", ci);
                        MySqlDataAdapter da = new MySqlDataAdapter(cmdNotas);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        string texto = "CI:" + ci + "\n" + nombre + "\n";
                        foreach (DataRow r in dt.Rows)
                        {
                            texto += r["Materia"] + ": " + r["Definitiva"] + "\n";
                        }
                        using (QRCodeGenerator gen = new QRCodeGenerator())
                        {
                            QRCodeData data = gen.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
                            using (QRCode qr = new QRCode(data))
                            {
                                picQr.Image = qr.GetGraphic(20);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se encontró un estudiante o usuario coincidente.");
                        picQr.Image = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar QR: " + ex.Message);
            }
        }

        private void ScanFromFile()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (Bitmap bmp = (Bitmap)Bitmap.FromFile(dlg.FileName))
                        {
                            var reader = new BarcodeReaderGeneric();
                            var result = reader.Decode(ConvertBitmapToLuminance(bmp));
                            if (result != null)
                                MessageBox.Show("Contenido del QR:\n" + result.Text);
                            else
                                MessageBox.Show("No se pudo leer ningún QR válido en la imagen.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al leer archivo: " + ex.Message);
                    }
                }
            }
        }

        // helper: convierte un Bitmap a LuminanceSource usado por ZXing
        internal static ZXing.LuminanceSource ConvertBitmapToLuminance(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int stride = bmpData.Stride;
            int len = stride * bmp.Height;
            byte[] pixels = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, len);
            bmp.UnlockBits(bmpData);
            return new ZXing.RGBLuminanceSource(pixels, bmp.Width, bmp.Height, ZXing.RGBLuminanceSource.BitmapFormat.RGB24);
        }
    }
}