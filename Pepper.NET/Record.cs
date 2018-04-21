using System.Collections.Generic;

namespace PepperNET
{
    public class Record : Dictionary<string, object>
    {
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
                    Modified = true;
                    base[index] = value;
                    base["change_type"] = 1;
                }
            }
        }
    }
}
