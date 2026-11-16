namespace WinFormsApp1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new HomeForm()); // <-- Changed from LoginForm to HomeForm
        }
    }
}