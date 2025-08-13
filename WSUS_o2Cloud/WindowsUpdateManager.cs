using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WUApiLib;

namespace WSUS_o2Cloud
{
    public class WindowsUpdateManager
    {
        private UpdateSession updateSession;
        private IUpdateSearcher updateSearcher;
        private UpdateCollection updatesToDownload;
        private UpdateCollection updatesToInstall;

        public WindowsUpdateManager()
        {
            try
            {
                updateSession = new UpdateSession();
                updateSearcher = updateSession.CreateUpdateSearcher();
                updatesToDownload = new UpdateCollection();
                updatesToInstall = new UpdateCollection();
            }
            catch (COMException ex)
            {
                throw new Exception($"Erreur d'initialisation Windows Update: {ex.Message}", ex);
            }
        }

        public List<IUpdate> SearchForUpdates(Action<int, string> progressCallback = null)
        {
            var updates = new List<IUpdate>();

            try
            {
                progressCallback?.Invoke(10, "Connexion au service Windows Update...");

                // Recherche des mises à jour
                string criteria = "IsInstalled=0 and Type='Software' and IsHidden=0";
                progressCallback?.Invoke(30, "Recherche des mises à jour disponibles...");

                ISearchResult searchResult = updateSearcher.Search(criteria);

                progressCallback?.Invoke(70, $"Analyse de {searchResult.Updates.Count} mise(s) à jour trouvée(s)...");

                foreach (IUpdate update in searchResult.Updates)
                {
                    if (update != null)
                    {
                        updates.Add(update);
                        progressCallback?.Invoke(80, $"Trouvé: {update.Title}");
                    }
                }

                progressCallback?.Invoke(100, $"Recherche terminée - {updates.Count} mise(s) à jour disponible(s)");
            }
            catch (COMException ex)
            {
                progressCallback?.Invoke(0, $"Erreur COM: {ex.Message}");
                throw new Exception($"Erreur lors de la recherche des mises à jour: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                progressCallback?.Invoke(0, $"Erreur: {ex.Message}");
                throw;
            }

            return updates;
        }

        public void DownloadUpdates(List<IUpdate> updates, Action<int, string> progressCallback = null)
        {
            if (updates == null || updates.Count == 0)
                return;

            try
            {
                updatesToDownload.Clear();

                // Ajouter les mises à jour à télécharger
                foreach (IUpdate update in updates)
                {
                    if (update.EulaAccepted == false)
                    {
                        update.AcceptEula();
                    }
                    updatesToDownload.Add(update);
                }

                if (updatesToDownload.Count == 0)
                {
                    progressCallback?.Invoke(100, "Aucune mise à jour à télécharger");
                    return;
                }

                progressCallback?.Invoke(10, $"Préparation du téléchargement de {updatesToDownload.Count} mise(s) à jour...");

                // Créer le downloader
                IUpdateDownloader downloader = updateSession.CreateUpdateDownloader();
                downloader.Updates = updatesToDownload;

                // Événement de progression
                var downloadProgressHandler = new DownloadProgressChangedCallback(progressCallback);

                progressCallback?.Invoke(20, "Début du téléchargement...");

                // Démarrer le téléchargement
                IDownloadResult downloadResult = downloader.Download();

                // Vérifier les résultats
                if (downloadResult.ResultCode == OperationResultCode.orcSucceeded)
                {
                    progressCallback?.Invoke(100, "Téléchargement terminé avec succès");
                }
                else if (downloadResult.ResultCode == OperationResultCode.orcSucceededWithErrors)
                {
                    progressCallback?.Invoke(90, "Téléchargement terminé avec des avertissements");
                }
                else
                {
                    throw new Exception($"Échec du téléchargement. Code: {downloadResult.ResultCode}");
                }

                // Préparer les mises à jour pour l'installation
                updatesToInstall.Clear();
                for (int i = 0; i < updatesToDownload.Count; i++)
                {
                    if (downloadResult.GetUpdateResult(i).ResultCode == OperationResultCode.orcSucceeded)
                    {
                        updatesToInstall.Add(updatesToDownload[i]);
                    }
                }

                progressCallback?.Invoke(100, $"{updatesToInstall.Count} mise(s) à jour prête(s) pour l'installation");
            }
            catch (COMException ex)
            {
                throw new Exception($"Erreur lors du téléchargement: {ex.Message}", ex);
            }
        }

        public InstallationResult InstallUpdates(List<IUpdate> updates, Action<int, string> progressCallback = null)
        {
            var result = new InstallationResult { Success = false, RebootRequired = false };

            try
            {
                if (updatesToInstall.Count == 0)
                {
                    progressCallback?.Invoke(0, "Aucune mise à jour à installer");
                    return new InstallationResult { Success = true, RebootRequired = false, Message = "Aucune installation nécessaire" };
                }

                progressCallback?.Invoke(10, $"Préparation de l'installation de {updatesToInstall.Count} mise(s) à jour...");

                // Créer l'installer
                IUpdateInstaller installer = updateSession.CreateUpdateInstaller();
                installer.Updates = updatesToInstall;

                progressCallback?.Invoke(20, "Début de l'installation...");

                // Démarrer l'installation
                IInstallationResult installResult = installer.Install();

                // Analyser les résultats
                bool allSucceeded = true;
                bool rebootRequired = installResult.RebootRequired;

                for (int i = 0; i < updatesToInstall.Count; i++)
                {
                    IUpdateInstallationResult updateResult = installResult.GetUpdateResult(i);
                    IUpdate update = updatesToInstall[i];

                    switch (updateResult.ResultCode)
                    {
                        case OperationResultCode.orcSucceeded:
                            progressCallback?.Invoke(50 + (i * 40 / updatesToInstall.Count),
                                $"Installé avec succès: {update.Title}");
                            break;

                        case OperationResultCode.orcSucceededWithErrors:
                            progressCallback?.Invoke(50 + (i * 40 / updatesToInstall.Count),
                                $"Installé avec avertissements: {update.Title}");
                            break;

                        case OperationResultCode.orcFailed:
                            allSucceeded = false;
                            progressCallback?.Invoke(50 + (i * 40 / updatesToInstall.Count),
                                $"Échec de l'installation: {update.Title} - Code: {updateResult.HResult}");
                            break;

                        case OperationResultCode.orcAborted:
                            allSucceeded = false;
                            progressCallback?.Invoke(50 + (i * 40 / updatesToInstall.Count),
                                $"Installation interrompue: {update.Title}");
                            break;
                    }
                }

                if (installResult.ResultCode == OperationResultCode.orcSucceeded ||
                    installResult.ResultCode == OperationResultCode.orcSucceededWithErrors)
                {
                    result.Success = true;
                    result.RebootRequired = rebootRequired;

                    if (rebootRequired)
                    {
                        result.Message = "Installation réussie. Redémarrage requis.";
                        progressCallback?.Invoke(100, "Installation terminée - Redémarrage requis");
                    }
                    else
                    {
                        result.Message = "Installation réussie.";
                        progressCallback?.Invoke(100, "Installation terminée avec succès");
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = $"Échec de l'installation. Code: {installResult.ResultCode}";
                    progressCallback?.Invoke(100, result.Message);
                }
            }
            catch (COMException ex)
            {
                result.Success = false;
                result.Message = $"Erreur COM lors de l'installation: {ex.Message}";
                progressCallback?.Invoke(0, result.Message);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Erreur lors de l'installation: {ex.Message}";
                progressCallback?.Invoke(0, result.Message);
            }

            return result;
        }

        public void Dispose()
        {
            if (updatesToDownload != null)
            {
                Marshal.ReleaseComObject(updatesToDownload);
                updatesToDownload = null;
            }

            if (updatesToInstall != null)
            {
                Marshal.ReleaseComObject(updatesToInstall);
                updatesToInstall = null;
            }

            if (updateSearcher != null)
            {
                Marshal.ReleaseComObject(updateSearcher);
                updateSearcher = null;
            }

            if (updateSession != null)
            {
                Marshal.ReleaseComObject(updateSession);
                updateSession = null;
            }
        }
    }

    // Classe pour gérer les callbacks de progression de téléchargement
    public class DownloadProgressChangedCallback
    {
        private Action<int, string> progressCallback;

        public DownloadProgressChangedCallback(Action<int, string> callback)
        {
            progressCallback = callback;
        }

        public void Invoke(IDownloadJob downloadJob, IDownloadProgressChangedCallbackArgs callbackArgs)
        {
            if (callbackArgs.Progress.TotalBytesToDownload > 0)
            {
                double progressPercent =
    ((double)callbackArgs.Progress.CurrentUpdateBytesDownloaded /
     (double)callbackArgs.Progress.TotalBytesToDownload) * 100;
                progressCallback?.Invoke((int)progressPercent,
                    $"Téléchargement: {progressPercent:F1}% ({FormatBytes(callbackArgs.Progress.CurrentUpdateBytesDownloaded)} / {FormatBytes(callbackArgs.Progress.TotalBytesToDownload)})");
            }
        }

        private string FormatBytes(decimal bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = (double)bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}