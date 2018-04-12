using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Pepper.NET
{
    public class Table
    {
        private string _Name;
        private Field[] _Fields;
        private List<Record> _Records = new List<Record>();

        public Table(string tableName)
        {
            string url = string.Format("https://js.signage.me/Table_{0}.js", tableName);

            string jsFile = Utils.GetHttpResponse(url);
            Match m = Regex.Match(jsFile, "this.m_name = \"(.+?)\"");
            if (m.Success)
            {
                _Name = m.Groups[1].Value.ToString();
            }
            m = Regex.Match(jsFile, "this.m_fields = \\[(.+?)\\];", RegexOptions.Singleline);
            if (m.Success)
            {
                List<Field> fields = JsonConvert.DeserializeObject<List<Field>>("[" + m.Groups[1].Value.ToString() + "]");
                _Fields = fields.ToArray();
            }
        }
        public void LoadRecords(XmlNode table)
        {
            _Records = new List<Record>();
            foreach (XmlNode record in table.ChildNodes)
            {
                Record rec = new Record();
                for (int i = 0; i < _Fields.Length && i < record.ChildNodes.Count; i++)
                    rec[_Fields[i].FieldName] = record.ChildNodes[i].InnerText;
                _Records.Add(rec);
            }
        }
        public void ChangesCommitted(int changeID)
        {
            var recordsWithChanges = _Records.Where(r => r.Modified);
            foreach (var record in recordsWithChanges)
            {
                record["changelist_id"] = changeID;
                record.ChangeCommitted();
            }
        }
        public void AppendChangesToChangelist(XElement changeList)
        {
            if (!HasChanges) return;
            var recordsWithChanges = _Records.Where(r => r.Modified);
            XElement table = new XElement("Table", new XAttribute("name", _Name),
                new XElement("Update",
                    recordsWithChanges.Select(rec => new XElement("Rec",
                        _Fields.Where(f => f.ForeignTable != null).Select(f => new XAttribute(f.FieldName, rec[f.FieldName])),
                        _Fields.Select(f => new XElement("Col", rec[f.FieldName] ?? "null"))
                    ))
                ),
                new XElement("Fields", _Fields.Select(f => new XElement("Field", f.FieldName)))
            );
            changeList.Add(table);
        }
        public int RecordsCount { get { return _Records.Count; } }
        public Record this[int index]
        {
            get
            {
                return _Records[index];
            }
        }
        public bool HasChanges
        {
            get
            {
                return _Records.Count(rec => rec.Modified) > 0;
            }
        }

        public IEnumerator<Record> GetEnumerator()
        {
            return _Records.GetEnumerator();
        }
        public IEnumerable<Record> Where(Func<Record, bool> predicate)
        {
            foreach (var record in _Records)
            {
                if (predicate(record))
                    yield return record;
            }
        }
    }
    public class Record
    {
        public bool Modified { get; private set; }
        private Dictionary<string, object> Columns { get; set; }
        public Record()
        {
            Columns = new Dictionary<string, object>();
            Modified = false;
        }
        public void ChangeCommitted()
        {
            Modified = false;
        }
        public object this[string index]
        {
            get
            {
                return Columns[index];
            }
            set
            {
                if (!Columns.ContainsKey(index)) Columns.Add(index, value);
                else
                {
                    Modified = true;
                    Columns[index] = value;
                    Columns["change_type"] = 1;
                }
            }
        }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Columns.GetEnumerator();
        }
    }
    public class Field
    {
        readonly SetOnce<string> _FieldName = new SetOnce<string>();
        readonly SetOnce<string> _ForeignTable = new SetOnce<string>();
        private SetOnce<bool> _IsNullable = new SetOnce<bool>();


        [JsonProperty("field")]
        public string FieldName
        {
            get { return _FieldName; }
            set { _FieldName.Value = value; }
        }
        [JsonProperty("foriegn")]
        public string ForeignTable
        {
            get { return _ForeignTable; }
            set { _ForeignTable.Value = value; }
        }
        [JsonProperty("isNullAble")]
        public bool IsNullable
        {
            get { return _IsNullable; }
            set { _IsNullable.Value = value; }
        }
    }
}
