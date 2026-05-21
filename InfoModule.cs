using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
/**
 * @Author Lada 
 * @Created 2018.08.29 
 */
namespace VideoPlayer
{
    public class InfoModule
    {
        private const string softFolder = "Olimi Soft";
        private const string appFolder = "Video Player";
        private const string appData = "vp.data";
        private Dictionary<long, VItem> collection;
        private List<VItem> files;
        private string _path;
        public PercentEvent PercentEvent { get; private set; }
        public EventHandler<OnClickEvent> VItemOnClick;

        public InfoModule()
        {
            files = new List<VItem>();
            collection = new Dictionary<long, VItem>();
            PercentEvent = new PercentEvent();
            load();
        }

        public void Append(VItem v)
        {
            v.EventOnClick += EventOnClick;
            files.Add(v);
        }

        public static string GetWorkFolder()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Combine(path, softFolder);
            CreateDir(path);

            path = Path.Combine(path, appFolder);
            CreateDir(path);

            return path;
        }

        private void load()
        {
            try
            {
                _path = Path.Combine(GetWorkFolder(), appData);

                if (File.Exists(_path))
                {
                    Read(_path);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void Read(string appData)
        {
            using (var sr = new StreamReader(appData))
            {
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (string.IsNullOrEmpty(s))
                        return;

                    var item = new VItem(s, PercentEvent);
                    item.EventOnClick += EventOnClick;
                    if (item.Length > 0)
                    {
                        files.Add(item);
                        if(!collection.ContainsKey(item.Length))                    
                            collection.Add(item.Length, item);
                    }                    
                }
            }
        }

        private void EventOnClick(object sender, EventArgs e)
        {
            var item = sender as VItem;
            if (item != null && VItemOnClick != null)
            {
                VItemOnClick.Invoke(this, new OnClickEvent(item));
            }
        }

        public void SaveInfo()
        {
            try
            {
                using (var sw = new StreamWriter(_path))
                {
                    foreach (var v in files)
                    {
                        sw.WriteLine(v.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void CreateDir(string p)
        {
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
        }

        internal IEnumerable<VItem> GetList()
        {
            return collection.Select(x => x.Value).OrderBy(x => x.Title);
        }
        
        internal void Remove(VItem item)
        {
            var k = collection.Where(p=>p.Value == item);
            if (k != null && k.Any())
            {
                collection.Remove(k.First().Key);
                SaveInfo();
            }
        }
    }
}
