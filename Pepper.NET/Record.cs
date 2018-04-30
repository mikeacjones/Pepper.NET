using System.Collections.Generic;

namespace PepperNET
{
    public class Record : Dictionary<string, object>
    {
        private SetOnce<int?> _Handle = new SetOnce<int?>();
        public int Handle { get { return _Handle.Value ?? -1; } set { _Handle.Value = value; } }
        public bool Modified { get; private set; }
        public SetOnce<bool?> _NewRecord = new SetOnce<bool?>();
        public bool NewRecord { get { return _NewRecord.Value ?? false; } set { _NewRecord.Value = value; } }
        internal Record() : base()
        {
            Modified = false;
        }
        public void ChangeCommitted()
        {
            Modified = false;
        }
        public new object this[string index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (!base.ContainsKey(index)) base.Add(index, value);
                else
                {
                    if (value == base[index]) return;

                    Modified = true;
                    base[index] = value;
                    if (int.Parse(base["change_type"].ToString()) <= 1)
                        base["change_type"] = 1;
                }
            }
        }
    }
}