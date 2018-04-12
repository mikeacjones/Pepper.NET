using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Pepper.NET
{
    public class Database
    {
        private Dictionary<string, Table> _Tables;
        private int _LastChangeID;

        internal Database()
        {
            _Tables = new Dictionary<string, Table>();
            foreach (string tableName in _TableNames)
                _Tables.Add(tableName, new Table(tableName));
        }
        internal void PopulateTables(XmlDocument xdoc)
        {
            _LastChangeID = int.Parse(xdoc.DocumentElement.GetAttribute("lastChangelistId"));
            foreach (XmlNode table in xdoc.FirstChild.ChildNodes)
            {
                string tableName = table.Attributes["name"].Value;
                if (!_Tables.ContainsKey(tableName)) continue;
                _Tables[tableName].LoadRecords(table);
            }
        }
        internal XDocument GetChangeList()
        {
            XDocument xdoc = new XDocument();

            var tablesWithChanges = _Tables.Where(t => t.Value.HasChanges).Select(t => t.Value);
            if (tablesWithChanges.Count() == 0) return null;

            var changeList = new XElement("Changelist");
            xdoc.Add(changeList);

            foreach (var table in tablesWithChanges)
                table.AppendChangesToChangelist(changeList);

            return xdoc;
        }
        public Table this[string tableName]
        {
            get
            {
                if (!_Tables.ContainsKey(tableName)) return null;
                return _Tables[tableName];
            }
        }
        public int TableCount { get { return _Tables.Count; } }
        public string[] TableName { get { return _Tables.Keys.ToArray(); } }
        internal void ChangesCommitted(int changeID)
        {
            var tablesWithChanges = _Tables.Where(t => t.Value.HasChanges).Select(t => t.Value);
            foreach (var table in tablesWithChanges)
                table.ChangesCommitted(changeID);
        }
        public IEnumerator<Table> GetEnumerator()
        {
            return _Tables.Values.GetEnumerator();
        }

        #region STATIC VARIABLES
        private readonly string[] _TableNames = new string[]
        {
            "global_settings",
            "resources",
            "ad_local_packages",
            "ad_local_contents",
            "category_values",
            "catalog_items",
            "catalog_item_infos",
            "catalog_item_resources",
            "catalog_item_categories",
            "player_data",
            "boards",
            "campaigns",
            "campaign_channels",
            "campaign_channel_players",
            "campaign_timelines",
            "campaign_events",
            "campaign_boards",
            "board_templates",
            "board_template_viewers",
            "campaign_timeline_chanels",
            "campaign_timeline_channels",
            "campaign_timeline_board_templates",
            "campaign_timeline_board_viewer_chanels",
            "campaign_timeline_board_viewer_channels",
            "campaign_timeline_chanel_players",
            "campaign_timeline_schedules",
            "campaign_timeline_sequences",
            "scripts",
            "music_channels",
            "music_channel_songs",
            "branch_stations",
            "ad_rates",
            "station_ads",
            "ad_out_packages",
            "ad_out_package_contents",
            "ad_out_package_stations",
            "ad_in_domains",
            "ad_in_domain_businesses",
            "ad_in_domain_business_packages",
            "ad_in_domain_business_package_stations"
        };
        #endregion
    }
}
