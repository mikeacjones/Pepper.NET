using Newtonsoft.Json;

namespace PepperNET
{
    public class Field
    {
        readonly SetOnce<string> _FieldName = new SetOnce<string>();
        readonly SetOnce<string> _ForeignTable = new SetOnce<string>();
        private SetOnce<bool> _IsNullable = new SetOnce<bool>();

        internal Field() { }

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
