using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace Dropa.MongoBackupRobot
{
    public partial class Backup : Form
    {
        public Backup()
        {
            InitializeComponent();
        }

        private async void btnStartStop_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void SetLog(string log)
        {
            if (string.IsNullOrEmpty(log)) return;
            lstLog.Items.Add(log);
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
            lstLog.SelectedIndex = -1;
        }

        //private async Task Archive()
        //{
        //    string name = $"{DateTime.Now}_{Guid.NewGuid()}";
        //    string fullfilename = Path.Combine(txtPath.Text, name);
        //    ZipFile.CreateFromDirectory(txtPath.Text, fullfilename, CompressionLevel.Optimal, false);
        //    SetLog($"Success -Archive : {name}");
        //}

        private async Task BackupUp()
        {
            string strCmdText = $@"mongodump --host  {txtIP.Text} -d {txtDbName.Text} --port {txtPort.Text} --username {txtUserName.Text} --password {txtPassword.Text} --archive={txtPath.Text}\\{DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss")}-{Guid.NewGuid()}.gz --gzip";
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine(strCmdText);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            SetLog($"Success -BackupUp {DateTime.Now}");
        }

        private async void tmrBackup_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text))
            {
                Stop();
            }
            else
            {
                await BackupUp();
            }
        }

        private void Stop()
        {
            tmrBackup.Stop();
            tmrBackup.Enabled = false;
        }

        private void Start()
        {
            tmrBackup.Enabled = true;
            tmrBackup.Start();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            string connectionStr = $"mongodb://{txtUserName.Text}:{txtPassword.Text}@{txtIP.Text}:{txtPort.Text}/{txtDbName.Text}";
            var client = new MongoClient(connectionStr);
            var server = client.GetDatabase(txtDbName.Text);
            try
            {
                bool isMongoLive = server.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
                if (isMongoLive)
                {
                    MessageBox.Show($"Success : Mongo is live");
                }
                else
                {
                    MessageBox.Show("Fail : Connection error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail : {ex.Message}");
            }
        }
    }
}