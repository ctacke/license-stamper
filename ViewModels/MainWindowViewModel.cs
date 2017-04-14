using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LicenseStamper
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static string DefaultFolder = "<SELECT>";

        private string m_copy;
        private string m_rootFolder;
        private string m_filter;
        private string m_regionTitle;
        private bool m_includeSubfolders;

        public MainWindowViewModel()
        {
            RootFolder = DefaultFolder;
            IncludeSubFolders = true;
            FileSearchFilter = "*.cs";
            RegionTitle = "LICENSE";
        }

        public string RootFolder
        {
            get { return m_rootFolder; }
            set
            {
                m_rootFolder = value;
                RaisePropertyChanged();
            }
        }

        public string FileSearchFilter
        {
            get { return m_filter; }
            set
            {
                m_filter = value;
                RaisePropertyChanged();
            }
        }

        public bool IncludeSubFolders
        {
            get { return m_includeSubfolders; }
            set
            {
                m_includeSubfolders = value;
                RaisePropertyChanged();
            }
        }

        public string RegionTitle
        {
            get { return m_regionTitle; }
            set
            {
                m_regionTitle = value;
                RaisePropertyChanged();
            }
        }

        public string LicenseCopy
        {
            get { return m_copy; }
            set
            {
                // TODO: macro replacements (i.e. %YEAR%)
                m_copy = value;

                RaisePropertyChanged();
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand HandleBrowseClick
        {
            get { return new CommandHandler(() =>
            {
            }); }
        }

        public ICommand HandleStampClick
        {
            get
            {
                return new CommandHandler(OnStamp, ValidateCanStamp);
            }
        }

        public ICommand HandleRemoveClick
        {
            get
            {
                return new CommandHandler(OnRemove, ValidateCanRemove);
            }
        }

        private bool ValidateCanRemove()
        {
            if (!ValidateRootPath()) return false;
            if (!ValidateFilter()) return false;
            if (!ValidateRegionTitle()) return false;

            return true;
        }

        private bool ValidateCanStamp()
        {
            if (!ValidateRootPath()) return false;
            if (!ValidateFilter()) return false;
            if (!ValidateRegionTitle()) return false;
            if (!ValidateLicenseCopy()) return false;
            return true;
        }

        private async void OnStamp()
        {
            var svc = new StamperService();
            var count = await svc.AddLicenseToFiles(RootFolder, FileSearchFilter, RegionTitle, LicenseCopy, IncludeSubFolders);

            MessageBox.Show(string.Format("Stamped {0} files", count), "Done");
        }

        private async void OnRemove()
        {
            var svc = new StamperService();
            var count = await svc.RemoveLicenseFromFiles(RootFolder, FileSearchFilter, RegionTitle, IncludeSubFolders);

            MessageBox.Show(string.Format("Removed license from {0} files", count), "Done");
        }

        private bool ValidateLicenseCopy()
        {
            if (string.IsNullOrWhiteSpace(LicenseCopy))
            {
                MessageBox.Show("You must provide license text", "Error");
                return false;
            }

            return true;
        }

        private bool ValidateRegionTitle()
        {
            if (string.IsNullOrWhiteSpace(FileSearchFilter))
            {
                MessageBox.Show("You must provide a region title - something as simple as 'License' is enough.", "Error");
                return false;
            }

            return true;
        }

        private bool ValidateFilter()
        {
            if (string.IsNullOrWhiteSpace(FileSearchFilter))
            {
                MessageBox.Show("You must provide a filter.  If you want 'everything' then explicitly use '*.*'", "Error");
                return false;
            }

            return true;
        }

        private bool ValidateRootPath()
        {
            try
            {
                var di = new DirectoryInfo(RootFolder);
                if (!di.Exists)
                {
                    MessageBox.Show("Root folder must be a valid, existing folder", "Error");
                    return false;
                }

                return true;
            }
            catch
            {
                MessageBox.Show("Root folder must be a valid, existing folder", "Error");
                return false;
            }
        }
    }
}
