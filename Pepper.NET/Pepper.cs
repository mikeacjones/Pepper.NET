namespace Pepper.NET
{
    public class Pepper
    {
        private LoadManager _LoadManager;
        public Database Database { get; private set; }

        public void DbConnect(string user, string pass)
        {
            _LoadManager = new LoadManager(user, pass);
            _LoadManager.Init();

            Database = new Database();
            var signageData = _LoadManager.LoadData();
            if (signageData != null) Database.PopulateTables(signageData);
        }
        public SaveResult SaveChanges()
        {
            var changeList = Database.GetChangeList();
            if (changeList == null) return SaveResult.NoChanges;
            int changelistID = _LoadManager.SaveData(changeList);
            if (changelistID > 0) Database.ChangesCommitted(changelistID);
            return changelistID > 0 ? SaveResult.Success : SaveResult.Failure;
        }
        public void Sync()
        {
            var signageData = _LoadManager.LoadData();
            if (signageData != null) Database.PopulateTables(signageData);
        }
    }
    public enum SaveResult
    {
        NoChanges,
        Success,
        Failure
    }
}
