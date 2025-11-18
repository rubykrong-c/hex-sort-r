namespace Code.App.Models
{
    public class UserDataModel : IUserDataGetter, IUserDataSetter
    {
        public int CurrentLevel { get; set; }
    }
}