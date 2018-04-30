using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PepperNET
{
    public class Pepper
    {
        private LoadManager _LoadManager;
        private Stack<FilePack> _FileToUpload;
        public Database Database { get; private set; }

        public Pepper(string user = null, string pass = null)
        {
            if (user != null && pass != null)
            {
                _LoadManager = new LoadManager(user, pass);
                _LoadManager.Init();
            }
        }
        public void DbConnect(string user = null, string pass = null)
        {
            if (user != null && pass != null)
            {
                _LoadManager = new LoadManager(user, pass);
                _LoadManager.Init();
            }
            if (_LoadManager == null)
                throw new System.Exception("Missing username and password");
            _FileToUpload = new Stack<FilePack>();

            Database = new Database();
            var signageData = _LoadManager.LoadData();
            if (signageData != null) Database.PopulateTables(signageData);
        }
        public StandardReturn SaveChanges()
        {
            var changeList = Database.GetChangeList();
            if (changeList == null) return null;
            var svRet = _LoadManager.SaveData(changeList, _FileToUpload);
            int changelistID = Utils.GetChangelistID(svRet);
            if (changelistID > 0) Database.ChangesCommitted(changelistID);
            return svRet;
        }
        public void Sync()
        {
            var signageData = _LoadManager.LoadData();
            if (signageData != null) Database.PopulateTables(signageData);
        }

        public int UploadResource(string resourcePath)
        {
            Record newResource = Database["resources"].CreateRecord();
            newResource["resource_id"] = -1;
            newResource["changelist_id"] = -1;
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
            _FileToUpload.Push(new FilePack { FilePath = resourcePath, FileRecord = newResource });
            return newResource.Handle;
        }
        public int CreateBranchStation(string stationName, int campaignBoardID)
        {
            Record newBranchStation = Database["branch_stations"].CreateRecord();
            newBranchStation["branch_station_id"] = -1;
            newBranchStation["branch_id"] = -1;
            newBranchStation["campaign_board_id"] = campaignBoardID;
            newBranchStation["station_name"] = stationName;
            newBranchStation["reboot_exceed_mem_enabled"] = false;
            newBranchStation["reboot_exceed_mem_value"] = 1;
            newBranchStation["reboot_time_enabled"] = false;
            newBranchStation["reboot_time_value"] = 0;
            newBranchStation["reboot_error_enabled"] = true;
            newBranchStation["monitor_standby_enabled"] = false;
            newBranchStation["monitor_standby_from"] = 3600;
            newBranchStation["monitor_standby_to"] = 14400;
            newBranchStation["location_long"] = -1;
            newBranchStation["location_lat"] = -1;
            newBranchStation["map_zoom"] = 4;
            newBranchStation["reboot_exceed_mem_action"] = 1;
            newBranchStation["reboot_time_action"] = 2;
            newBranchStation["reboot_error_action"] = 1;
            newBranchStation["station_mode"] = 1;
            newBranchStation["power_mode"] = 0;
            newBranchStation["power_on_day1"] = 25200;
            newBranchStation["power_off_day1"] = 68400;
            newBranchStation["power_on_day2"] = 25200;
            newBranchStation["power_off_day2"] = 68400;
            newBranchStation["power_on_day3"] = 25200;
            newBranchStation["power_off_day3"] = 68400;
            newBranchStation["power_on_day4"] = 25200;
            newBranchStation["power_off_day4"] = 68400;
            newBranchStation["power_on_day5"] = 25200;
            newBranchStation["power_off_day5"] = 68400;
            newBranchStation["power_on_day6"] = 25200;
            newBranchStation["power_off_day6"] = 68400;
            newBranchStation["power_on_day7"] = 25200;
            newBranchStation["power_off_day7"] = 68400;
            newBranchStation["send_notification"] = false;
            newBranchStation["frame_rate"] = 24;
            newBranchStation["quality"] = 2;
            newBranchStation["transition_enabled"] = true;
            newBranchStation["lan_server_enabled"] = false;
            newBranchStation["lan_server_port"] = 9999;
            Database["branch_stations"].Add(newBranchStation);
            return newBranchStation.Handle;
        }
        public void DeleteResource(string resourceID)
        {
            var rec = Database["resources"].Where(r => r["resource_id"].ToString() == resourceID).FirstOrDefault();
            if (rec == null) return;
            Database["resources"].Remove(rec);
        }

        public XmlDocument GetStationStatus(int branch = -1)
        {
            var ret = new XmlDocument();
            ret.LoadXml(_LoadManager.GetStatusStatus(branch));
            return ret;
        }
        public void StopStationByID(string stationID)
        {
            _LoadManager.SendCommandToStation(stationID, "stop");
        }
        public void StartStationByID(string stationID)
        {
            _LoadManager.SendCommandToStation(stationID, "start");
        }
        public void RebootStationByID(string stationID)
        {
            _LoadManager.SendCommandToStation(stationID, "rebootPlayer");
        }
        public void ClearStationCachingByID(string stationID)
        {
            _LoadManager.SendCommandToStation(stationID, "clearCaching");
        }
    }
    internal struct FilePack
    {
        public Record FileRecord { get; set; }
        public string FilePath { get; set; }
    }
}