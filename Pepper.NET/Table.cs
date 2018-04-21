using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace PepperNET
{
    public class Table : List<Record>
    {
        private string _Name;
        private Field[] _Fields;
        private List<Record> _RemovedRecords;

        internal Table(string tableName) : base()
        {
            _RemovedRecords = new List<Record>();
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
        public new void Remove(Record rec)
        {
            _RemovedRecords.Add(rec);
            base.Remove(rec);
        }
        public void LoadRecords(XmlNode table)
        {
            this.Clear();
            foreach (XmlNode record in table.ChildNodes)
            {
                Record rec = new Record();
                for (int i = 0; i < _Fields.Length && i < record.ChildNodes.Count; i++)
                    rec[_Fields[i].FieldName] = record.ChildNodes[i].InnerText;
                this.Add(rec);
            }
        }
        public void ChangesCommitted(int changeID)
        {
            var recordsWithChanges = this.Where(r => r.Modified);
            foreach (var record in recordsWithChanges)
            {
                record["changelist_id"] = changeID;
                record.ChangeCommitted();
            }
        }
        public void AppendChangesToChangelist(XElement changeList)
        {
            if (!HasChanges) return;
            var recordsWithChanges = this.Where(r => r.Modified && !r.NewRecord);
            var newRecords = this.Where(r => r.NewRecord);
            XElement table = new XElement("Table", new XAttribute("name", _Name),
                new XElement("Update",
                    recordsWithChanges.Select(rec => new XElement("Rec",
                        _Fields.Where(f => f.ForeignTable != null).Select(f => new XAttribute(f.FieldName, rec[f.FieldName])),
                        _Fields.Select(f => new XElement("Col", rec[f.FieldName] ?? "null"))
                    ))
                ),
                new XElement("New",
                newRecords.Select(rec => new XElement("Rec",
                        _Fields.Where(f => f.ForeignTable != null).Select(f => new XAttribute(f.FieldName, rec[f.FieldName])),
                        _Fields.Select(f => new XElement("Col", rec[f.FieldName] ?? "null"))
                    ))
                ),
                new XElement("Fields", _Fields.Select(f => new XElement("Field", f.FieldName)))
            );
            changeList.Add(table);
        }
        public void AppendDeletesToChangelist(XElement changeList)
        {
            if (_RemovedRecords.Count == 0) return;
            XElement table = new XElement("Table", new XAttribute("name", _Name),
                new XElement("Delete",
                    _RemovedRecords.Select(rec => new XElement("Rec",
                        new XAttribute("pk", rec[_Fields[0].FieldName])
                    ))
                ),
                new XElement("Fields", _Fields.Select(f => new XElement("Field", f.FieldName)))
            );
            changeList.Add(table);
        }
        public int RecordsCount { get { return this.Count; } }
        public bool HasChanges
        {
            get
            {
                return this.Count(rec => rec.Modified) > 0;
            }
        }
        public bool HasRemoved
        {
            get
            {
                return _RemovedRecords.Count > 0;
            }
        }
        public Record CreateRecord()
        {
            Record rec = new Record() { NewRecord = true };
            for (int i = 0; i < _Fields.Length; i++)
            {
                rec[_Fields[i].FieldName] = null;
            }
            return rec;
        }
    }
}
