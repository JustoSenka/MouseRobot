using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Robot
{
    public class AssetManager
    {
        static private AssetManager m_Instance = new AssetManager();
        static public AssetManager Instance { get { return m_Instance; } }
        private AssetManager()
        {
            k_ScriptPath = Path.Combine(Application.StartupPath, ScriptFolder);
            k_ImagePath = Path.Combine(Application.StartupPath, ImageFolder);

            InitProject();
        }

        public LinkedList<Asset> Assets { get; private set; } = new LinkedList<Asset>();

        public event Action RefreshFinished;

        public const string ScriptFolder = "Scripts";
        public const string ImageFolder = "Images";

        private readonly string k_ScriptPath;
        private readonly string k_ImagePath;

        public void Refresh()
        {
            Assets.Clear();

            foreach (string fileName in Directory.GetFiles(k_ImagePath, "*.png").Select(Path.GetFileName))
                Assets.AddLast(new Asset(ImageFolder + "\\" + fileName));

            foreach (string fileName in Directory.GetFiles(k_ScriptPath, "*.mrb").Select(Path.GetFileName))
                Assets.AddLast(new Asset(ScriptFolder + "\\" + fileName));

            RefreshFinished?.Invoke();
        }

        public object LoadAsset(string path)
        {
            if (path.EndsWith(FileExtensions.Image))
                return new ImageImporter(path);

            else if (path.EndsWith(FileExtensions.Script))
                return new ScriptImporter(path);

            else
                return null;
        }

        private void InitProject()
        {
            if (!Directory.Exists(k_ScriptPath))
                Directory.CreateDirectory(k_ScriptPath);

            if (!Directory.Exists(k_ImagePath))
                Directory.CreateDirectory(k_ImagePath);
        }
    }
}
