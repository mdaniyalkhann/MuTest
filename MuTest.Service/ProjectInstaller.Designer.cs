namespace MuTest.Service
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
            this.MuTestProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.MuTestServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // MuTestProcessInstaller
            // 
            this.MuTestProcessInstaller.Password = null;
            this.MuTestProcessInstaller.Username = null;
            // 
            // MuTestServiceInstaller
            // 
            this.MuTestServiceInstaller.DelayedAutoStart = true;
            this.MuTestServiceInstaller.DisplayName = "MuTest Service";
            this.MuTestServiceInstaller.ServiceName = "MuTestService";
            this.MuTestServiceInstaller.ServicesDependedOn = new string[] {
        null};
            this.MuTestServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MuTestProcessInstaller,
            this.MuTestServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MuTestProcessInstaller;
        private System.ServiceProcess.ServiceInstaller MuTestServiceInstaller;
    }
}