using QFramework;
using System.Collections.Generic;

namespace UG20260527
{
    public interface ISaveGameModel : IModel
    {
        /// <summary>
        /// json存档
        /// </summary>
        public BindableProperty<Dictionary<string, string>> saveGames { get; set; }
        public BindableProperty<string> selectSaveGameKey { get; set; }

    }

    public class SaveGameModel : AbstractModel, ISaveGameModel
    {
        public BindableProperty<Dictionary<string, string>> saveGames { get; set; } = new BindableProperty<Dictionary<string, string>>(new Dictionary<string, string>());

        public BindableProperty<string> selectSaveGameKey { get; set; } = new BindableProperty<string>("");


        protected override void OnInit()
        {

        }
    }
}
