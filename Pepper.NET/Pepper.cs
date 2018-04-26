using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PepperNET
{
    public class Pepper
    {
        private LoadManager _LoadManager;
        private Stack<string> _FileToUpload;
        public Database Database { get; private set; }

        public void DbConnect(string user, string pass)
        {
            _LoadManager = new LoadManager(user, pass);
            _LoadManager.Init();
            _FileToUpload = new Stack<string>();

            Database = new Database();
            var signageData = _LoadManager.LoadData();
            if (signageData != null) Database.PopulateTables(signageData);
        }
        public SaveResult SaveChanges()
        {
            var changeList = Database.GetChangeList();
            if (changeList == null) return SaveResult.NoChanges;
            int changelistID = _LoadManager.SaveData(changeList, _FileToUpload);
            if (changelistID > 0) Database.ChangesCommitted(changelistID);
            return changelistID > 0 ? SaveResult.Success : SaveResult.Failure;
        }
        public void Sync()
        {
            var signageData = _LoadManager.LoadData();
            if (signageData != null) Database.PopulateTables(signageData);
        }

        public void UploadResource(string resourcePath)
        {
            Record newResource = Database["resources"].CreateRecord();
            newResource["resource_id"] = -1;
            newResource["resource_name"] = Path.GetFileNameWithoutExtension(resourcePath);
            newResource["resource_type"] = Path.GetExtension(resourcePath).Substring(1);
            newResource["default_player"] = Utils.GetDefaultPlayer(resourcePath);
            newResource["resource_bytes_total"] = File.ReadAllBytes(resourcePath).Length;
            newResource["resource_pixel_width"] = 0;
            newResource["resource_pixel_height"] = 0;
            newResource["resource_total_time"] = 0;
            newResource["resource_trust"] = false;
            newResource["resource_public"] = false;
            newResource["resource_module"] = false;
            newResource["tree_path"] = "";
            newResource["access_key"] = 0;
            newResource["resource_html"] = false;
            newResource["shortcut"] = false;
            newResource["shortcut_business_id"] = -1;
            newResource["shortcut_resource_id"] = -1;
            newResource["changelist_id"] = -1;
            Database["resources"].Add(newResource);
            _FileToUpload.Push(resourcePath);
        }
        public void DeleteResource(string resourceID)
        {
            var rec = Database["resources"].Where(r => r["resource_id"].ToString() == resourceID).FirstOrDefault();
            if (rec == null) return;
            Database["resources"].Remove(rec);
        }
    }
}