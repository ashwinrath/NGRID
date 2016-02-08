namespace NGRID
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NGRIDProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.NGRIDServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // NGRIDProcessInstaller
            // 
            this.NGRIDProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.NGRIDProcessInstaller.Password = null;
            this.NGRIDProcessInstaller.Username = null;
            // 
            //NGRIDServiceInstaller
            // 
            this.NGRIDServiceInstaller.Description = "NGRID";
            this.NGRIDServiceInstaller.DisplayName = "NGRID";
            this.NGRIDServiceInstaller.ServiceName = "NGRIDService";
            this.NGRIDServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.NGRIDProcessInstaller,
            this.NGRIDServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller NGRIDProcessInstaller;
        private System.ServiceProcess.ServiceInstaller NGRIDServiceInstaller;
    }
}