using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenseStamper
{
    public class StamperService
    {
        public async Task<int> AddLicenseToFiles(string rootPath, string fileSelectionFilter, string licenseBlockTitle, string licenseText, bool includeSubfolders = true)
        {
            var count = 0;

            foreach (var f in Directory.GetFiles(rootPath, fileSelectionFilter))
            {
                if (await AddLicenseToFile(f, licenseBlockTitle, licenseText))
                {
                    count++;
                }
            }

            if (includeSubfolders)
            {
                var folders = Directory.GetDirectories(rootPath);

                foreach (var folder in folders)
                {
                    count += await AddLicenseToFiles(folder, fileSelectionFilter, licenseBlockTitle, licenseText, includeSubfolders);
                }
            }

            return count;
        }

        public async Task<bool> AddLicenseToFile(string filePath, string licenseBlockTitle, string licenseText)
        {
            if (string.IsNullOrWhiteSpace(licenseBlockTitle)) return false;
            if (string.IsNullOrWhiteSpace(licenseText)) return false;

            // find and remove any existing license block
            await RemoveLicenseFromFile(filePath, licenseBlockTitle);

            return await Task.Run(() =>
            {
                try
                {
                    // add the new block
                    var tempName = filePath + ".tmplic";
                    using (var writer = File.CreateText(tempName))
                    {
                        writer.Write("#region " + licenseBlockTitle + Environment.NewLine);
                        writer.Write(licenseText + Environment.NewLine);
                        writer.Write("#endregion" + Environment.NewLine);

                        writer.Write(File.ReadAllText(filePath));
                    }

                    File.Delete(filePath);
                    File.Move(tempName, filePath);

                    return true;
                }
                catch (Exception ex)
                {
                    // TODO: raise some error event
                    Debug.WriteLine(ex.Message);
                    if (Debugger.IsAttached) Debugger.Break();

                    return false;
                }
            });
        }

        public async Task<int> RemoveLicenseFromFiles(string rootPath, string fileSelectionFilter, string licenseBlockTitle, bool includeSubfolders = true)
        {
            if (!File.Exists(rootPath)) return 0;
            if (string.IsNullOrWhiteSpace(licenseBlockTitle)) return 0;

            var count = 0;

            foreach (var f in Directory.GetFiles(rootPath, fileSelectionFilter))
            {
                if (await RemoveLicenseFromFile(f, licenseBlockTitle))
                {
                    count++;
                }
            }

            if (includeSubfolders)
            {
                var folders = Directory.GetDirectories(rootPath);

                foreach (var folder in folders)
                {
                    count += await RemoveLicenseFromFiles(folder, fileSelectionFilter, licenseBlockTitle, includeSubfolders);
                }
            }

            return count;
        }

        public async Task<bool> RemoveLicenseFromFile(string filePath, string licenseBlockTitle)
        {
            if (!File.Exists(filePath)) return false;
            if (string.IsNullOrWhiteSpace(licenseBlockTitle)) return false;

            return await Task.Run(() =>
            {
                try
                {
                    var content = File.ReadAllText(filePath);
                    var start = GetLicenseStart(content, licenseBlockTitle);
                    if (start >= 0)
                    {
                        var end = GetLicenseEnd(content, start);

                        using (var writer = File.CreateText(filePath))
                        {
                            writer.Write(content.Substring(0, start));
                            writer.Write(content.Substring(end));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    // TODO: raise some error event
                    Debug.WriteLine(ex.Message);
                    if (Debugger.IsAttached) Debugger.Break();

                    return false;
                }

                return true;
            });
        }

        private int GetLicenseStart(string fileContent, string licenseBlockTitle)
        {
            var index = fileContent.IndexOf("#region " + licenseBlockTitle, StringComparison.InvariantCultureIgnoreCase);
            return index;
        }

        private int GetLicenseEnd(string fileContent, int startPosition)
        {
            if (startPosition < 0) return -1;
            var endToken = "#endregion";
            var index = fileContent.IndexOf(endToken, startPosition);
            if (index >= 0)
            {
                index += endToken.Length;
            }
            while (index < fileContent.Length && (fileContent[index] == '\r' || fileContent[index] == '\n'))
            {
                index++;
            }

            return index;
        }
    }
}
