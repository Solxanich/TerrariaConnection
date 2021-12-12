using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using System.Reflection;

namespace TerrariaConnection
{
	public class TerrariaConnection : Mod
	{
        public override void Load()
        {
            var a = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.Mod").GetProperty("File", BindingFlags.Instance | BindingFlags.NonPublic);
            TmodFile file = (TmodFile)a.GetValue(this);

            string dir = Path.GetDirectoryName(file.path);

            if (!File.Exists("Libraries/TerrariaConnection/io.txt"))
                DirectoryCopy(Path.Combine(dir, "TerrariaConnection"), "Libraries/TerrariaConnection", false);
            
            DirectoryCopy(Path.Combine(dir, "StartScript"), "../tModLoader", false);

            Main.instance.Exiting += Instance_Exiting;
            On.Terraria.Main.OnceFailedLoadingAnAsset += Main_OnceFailedLoadingAnAsset;
            On.Terraria.Achievements.AchievementCondition.Complete += AchievementCondition_Complete;
        }

        private void AchievementCondition_Complete(On.Terraria.Achievements.AchievementCondition.orig_Complete orig, Terraria.Achievements.AchievementCondition self)
        {
            MulticastDelegate eventDelegate
            self.OnComplete(self);

            SendCmdToTerraria("Grant:" + achievement);
        }

        internal static void SendCmdToTerraria(string cmd)
        {
            using (var ws = File.AppendText("Libraries/TerrariaConnection/io.txt"))
                ws.WriteLine(cmd);
        }

        private void Instance_Exiting(object sender, System.EventArgs e)
        {
            SendCmdToTerraria("unload");
        }

        private void Main_OnceFailedLoadingAnAsset(On.Terraria.Main.orig_OnceFailedLoadingAnAsset orig, string assetPath, System.Exception e)
        {
            SendCmdToTerraria("CheckUpdates");
        }

        // Straight outta https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}