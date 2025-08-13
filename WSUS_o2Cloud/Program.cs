using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WSUS_o2Cloud
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Configuration de l'application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Vérifier qu'une seule instance est en cours d'exécution
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "WSUS_o2Cloud_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("Une instance de l'application est déjà en cours d'exécution.",
                        "Application déjà lancée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Vérifier les privilèges administrateur
                    if (!IsRunningAsAdministrator())
                    {
                        // Tentative de relancement avec privilèges élevés
                        if (args.Length == 0 || args[0] != "--elevated")
                        {
                            RestartAsAdministrator();
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Impossible d'obtenir les privilèges administrateur.\nL'application ne peut pas fonctionner correctement.",
                                "Erreur de privilèges", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Initialiser le gestionnaire d'exceptions global
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    Application.ThreadException += Application_ThreadException;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                    // Démarrer l'application principale
                    LogMessage("Démarrage de l'application WSUS o2Cloud");
                    Application.Run(new Form1());
                }
                catch (Exception ex)
                {
                    LogError("Erreur fatale au démarrage", ex);
                    MessageBox.Show($"Erreur fatale au démarrage de l'application:\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    LogMessage("Arrêt de l'application WSUS o2Cloud");
                }
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static void RestartAsAdministrator()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Arguments = "--elevated",
                    Verb = "runas"
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible de redémarrer avec les privilèges administrateur:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogError("Exception non gérée dans le thread d'interface", e.Exception);

            string message = $"Une erreur inattendue s'est produite:\n\n{e.Exception.Message}\n\nDétails techniques:\n{e.Exception}";

            DialogResult result = MessageBox.Show(
                message + "\n\nVoulez-vous continuer l'exécution de l'application ?",
                "Erreur non gérée",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (result == DialogResult.No)
            {
                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogError("Exception non gérée dans le domaine d'application", ex ?? new Exception("Exception inconnue"));

            string message = "Une erreur critique s'est produite. L'application va se fermer.\n\n";
            if (ex != null)
            {
                message += $"Erreur: {ex.Message}\n\nDétails: {ex}";
            }
            else
            {
                message += "Exception inconnue";
            }

            MessageBox.Show(message, "Erreur critique", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            // Forcer la fermeture en cas d'erreur critique
            Environment.Exit(1);
        }

        private static void LogMessage(string message)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "WSUSo2Cloud.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO - {message}\r\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignorer les erreurs de logging pour éviter les boucles infinies
            }
        }

        private static void LogError(string message, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "WSUSo2Cloud.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - {message}\r\n";
                if (ex != null)
                {
                    logEntry += $"Exception: {ex.Message}\r\n";
                    logEntry += $"Stack Trace: {ex.StackTrace}\r\n";
                    if (ex.InnerException != null)
                    {
                        logEntry += $"Inner Exception: {ex.InnerException.Message}\r\n";
                    }
                }
                logEntry += "\r\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignorer les erreurs de logging
            }
        }
    }
}