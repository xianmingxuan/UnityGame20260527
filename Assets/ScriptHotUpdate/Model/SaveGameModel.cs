using QFramework;
using System.Collections.Generic;

namespace UG20260527
{
    public interface ISaveGameModel : IModel
    {
        public BindableProperty<List<object>> saveGames { get; set; }
        public BindableProperty<int> selectSaveGameIndex { get; set; }

    }

    public class SaveGameModel : AbstractModel, ISaveGameModel
    {
        public BindableProperty<List<object>> saveGames { get; set; } = new BindableProperty<List<object>>(new List<object>());

        public BindableProperty<int> selectSaveGameIndex { get; set; } = new BindableProperty<int>();


        protected override void OnInit()
        {
            selectSaveGameIndex.Value = 0;
            for (int i = 0; i < 7; i++) saveGames.Value.Add(i);
        }
    }
}
