namespace SistemaPintoSalinas;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Esta línea prepara la configuración visual
        ApplicationConfiguration.Initialize();
        
        // AQUÍ ESTABA EL ERROR: Cambiamos 'new Form1()' por 'new FormLogin()'
        Application.Run(new FormLogin());
    }
}