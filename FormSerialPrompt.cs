using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaPintoSalinas
{
    public class FormSerialPrompt : Form
    {
        private TextBox txtSerial;
        private Button btnOk, btnCancel;

        public string Serial => txtSerial.Text.Trim();

        public FormSerialPrompt()
        {
            this.Text = "Ingresar Serial de Seguridad";
            this.Size = new Size(400, 180);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label() { Text = "Ingrese el serial asignado:", Location = new Point(20, 20), AutoSize = true });
            txtSerial = new TextBox() { Location = new Point(20, 50), Width = 340 };
            this.Controls.Add(txtSerial);

            btnOk = new Button() { Text = "Verificar", Location = new Point(200, 90), Width = 80 };
            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            this.Controls.Add(btnOk);

            btnCancel = new Button() { Text = "Cancelar", Location = new Point(290, 90), Width = 80 };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }
    }
}
