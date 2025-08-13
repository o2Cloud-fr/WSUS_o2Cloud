using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.IO;
using WUApiLib;

namespace WSUS_o2Cloud
{
    public partial class Form1 : Form
    {
        private WindowsUpdateManager updateManager;
        private BackgroundWorker searchWorker;
        private BackgroundWorker downloadWorker;
        private BackgroundWorker installWorker;
        private List<IUpdate> availableUpdates;
        private bool isSearching = false;
        private bool isInstalling = false;

        public Form1()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            // Vérifier les privilèges administrateur
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Cette application nécessite des privilèges administrateur.\nVeuillez redémarrer en tant qu'administrateur.",
                    "Privilèges insuffisants", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            updateManager = new WindowsUpdateManager();
            availableUpdates = new List<IUpdate>();

            InitializeBackgroundWorkers();

            // Recherche automatique au démarrage
            Task.Run(() => {
                System.Threading.Thread.Sleep(1000); // Délai pour l'initialisation de l'interface
                this.Invoke(new Action(() => SearchForUpdates()));
            });

            LogMessage("Application démarrée avec privilèges administrateur");
        }

        private void InitializeBackgroundWorkers()
        {
            // Worker pour la recherche
            searchWorker = new BackgroundWorker();
            searchWorker.WorkerReportsProgress = true;
            searchWorker.DoWork += SearchWorker_DoWork;
            searchWorker.ProgressChanged += SearchWorker_ProgressChanged;
            searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;

            // Worker pour le téléchargement
            downloadWorker = new BackgroundWorker();
            downloadWorker.WorkerReportsProgress = true;
            downloadWorker.DoWork += DownloadWorker_DoWork;
            downloadWorker.ProgressChanged += DownloadWorker_ProgressChanged;
            downloadWorker.RunWorkerCompleted += DownloadWorker_RunWorkerCompleted;

            // Worker pour l'installation
            installWorker = new BackgroundWorker();
            installWorker.WorkerReportsProgress = true;
            installWorker.DoWork += InstallWorker_DoWork;
            installWorker.ProgressChanged += InstallWorker_ProgressChanged;
            installWorker.RunWorkerCompleted += InstallWorker_RunWorkerCompleted;
        }

        private bool IsRunningAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void SearchForUpdates()
        {
            if (isSearching) return;

            btnSearchUpdates.Enabled = false;
            btnInstallUpdates.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;
            txtDetails.AppendText("Recherche des mises à jour en cours...\r\n");
            lblStatus.Text = "Recherche en cours...";

            searchWorker.RunWorkerAsync();
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                isSearching = true;
                var updates = updateManager.SearchForUpdates((progress, message) => {
                    searchWorker.ReportProgress(progress, message);
                });
                e.Result = updates;
            }
            catch (Exception ex)
            {
                LogError("Erreur lors de la recherche", ex);
                e.Result = new List<IUpdate>();
            }
        }

        private void SearchWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                txtDetails.AppendText($"{e.UserState}\r\n");
                txtDetails.ScrollToCaret();
            }
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isSearching = false;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;
            btnSearchUpdates.Enabled = true;

            if (e.Result is List<IUpdate> updates)
            {
                availableUpdates = updates;
                DisplaySearchResults();
            }
        }

        private void DisplaySearchResults()
        {
            if (availableUpdates.Count == 0)
            {
                lblStatus.Text = "Aucune mise à jour disponible";
                txtDetails.AppendText("Aucune mise à jour n'a été trouvée.\r\n");
                MessageBox.Show("Votre système est à jour !", "Mises à jour", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Calculer la taille totale
            long totalSize = 0;
            int criticalCount = 0, importantCount = 0, optionalCount = 0;

            foreach (IUpdate update in availableUpdates)
            {
                if (update.MaxDownloadSize > 0)
                    totalSize += (long)update.MaxDownloadSize;

                if (update.MsrcSeverity != null)
                {
                    switch (update.MsrcSeverity.ToLower())
                    {
                        case "critical": criticalCount++; break;
                        case "important": importantCount++; break;
                        default: optionalCount++; break;
                    }
                }
                else
                {
                    optionalCount++;
                }
            }

            string sizeText = FormatFileSize(totalSize);
            string message = $"{availableUpdates.Count} mise(s) à jour trouvée(s)\n\n";
            message += $"• Critiques: {criticalCount}\n";
            message += $"• Importantes: {importantCount}\n";
            message += $"• Optionnelles: {optionalCount}\n\n";
            message += $"Taille totale: {sizeText}\n\n";
            message += "Voulez-vous installer ces mises à jour maintenant ?";

            lblStatus.Text = $"{availableUpdates.Count} mise(s) à jour disponible(s)";
            txtDetails.AppendText($"\n=== RÉSULTATS DE LA RECHERCHE ===\n");
            txtDetails.AppendText($"Mises à jour trouvées: {availableUpdates.Count}\n");
            txtDetails.AppendText($"Taille totale: {sizeText}\n\n");

            foreach (IUpdate update in availableUpdates)
            {
                txtDetails.AppendText($"• {update.Title}\n");
                if (!string.IsNullOrEmpty(update.Description))
                    txtDetails.AppendText($"  Description: {update.Description.Substring(0, Math.Min(100, update.Description.Length))}...\n");
                txtDetails.AppendText($"  Taille: {FormatFileSize((long)update.MaxDownloadSize)}\n\n");
            }

            btnInstallUpdates.Enabled = true;

            var result = MessageBox.Show(message, "Mises à jour disponibles",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                InstallUpdates();
            }
        }

        private void InstallUpdates()
        {
            if (isInstalling || availableUpdates.Count == 0) return;

            btnSearchUpdates.Enabled = false;
            btnInstallUpdates.Enabled = false;
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Blocks;
            txtDetails.AppendText("\n=== DÉBUT DE L'INSTALLATION ===\n");
            lblStatus.Text = "Téléchargement en cours...";

            downloadWorker.RunWorkerAsync();
        }

        private void DownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                updateManager.DownloadUpdates(availableUpdates, (progress, message) => {
                    downloadWorker.ReportProgress(progress, message);
                });
                e.Result = true;
            }
            catch (Exception ex)
            {
                LogError("Erreur lors du téléchargement", ex);
                e.Result = false;
            }
        }

        private void DownloadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = Math.Min(e.ProgressPercentage, 100);
            if (e.UserState != null)
            {
                txtDetails.AppendText($"{e.UserState}\r\n");
                txtDetails.ScrollToCaret();
            }
        }

        private void DownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
            {
                lblStatus.Text = "Installation en cours...";
                txtDetails.AppendText("Téléchargement terminé. Début de l'installation...\n");
                progressBar.Value = 0;
                installWorker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Erreur lors du téléchargement des mises à jour.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetInterface();
            }
        }

        private void InstallWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                isInstalling = true;
                var result = updateManager.InstallUpdates(availableUpdates, (progress, message) => {
                    installWorker.ReportProgress(progress, message);
                });
                e.Result = result;
            }
            catch (Exception ex)
            {
                LogError("Erreur lors de l'installation", ex);
                e.Result = new InstallationResult { Success = false, RebootRequired = false };
            }
        }

        private void InstallWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = Math.Min(e.ProgressPercentage, 100);
            if (e.UserState != null)
            {
                txtDetails.AppendText($"{e.UserState}\r\n");
                txtDetails.ScrollToCaret();
            }
        }

        private void InstallWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isInstalling = false;

            if (e.Result is InstallationResult result)
            {
                if (result.Success)
                {
                    txtDetails.AppendText("\n=== INSTALLATION TERMINÉE ===\n");
                    lblStatus.Text = "Installation terminée";

                    string message = "Installation des mises à jour terminée avec succès !";
                    if (result.RebootRequired)
                    {
                        message += "\n\nUn redémarrage est requis pour finaliser l'installation.\nVoulez-vous redémarrer maintenant ?";
                        var rebootResult = MessageBox.Show(message, "Installation terminée",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (rebootResult == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("shutdown", "/r /t 5 /c \"Redémarrage pour finaliser les mises à jour\"");
                            Application.Exit();
                        }
                    }
                    else
                    {
                        MessageBox.Show(message, "Installation terminée",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("L'installation a échoué. Consultez les détails pour plus d'informations.",
                        "Erreur d'installation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            ResetInterface();
        }

        private void ResetInterface()
        {
            btnSearchUpdates.Enabled = true;
            btnInstallUpdates.Enabled = availableUpdates.Count > 0;
            progressBar.Value = 0;
            if (availableUpdates.Count == 0)
                lblStatus.Text = "Prêt";
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void LogMessage(string message)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "UpdateLog.txt");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch { /* Ignore logging errors */ }
        }

        private void LogError(string message, Exception ex)
        {
            string fullMessage = $"{message}: {ex.Message}";
            LogMessage($"ERREUR - {fullMessage}");
            txtDetails.AppendText($"ERREUR: {fullMessage}\r\n");
        }

        // Event handlers pour les boutons
        private void btnSearchUpdates_Click(object sender, EventArgs e)
        {
            SearchForUpdates();
        }

        private void btnInstallUpdates_Click(object sender, EventArgs e)
        {
            InstallUpdates();
        }
    }

    public class InstallationResult
    {
        public bool Success { get; set; }
        public bool RebootRequired { get; set; }
        public string Message { get; set; }
    }
}