using SociallyDistant.Core;
using Thundershock.Gui.Elements;

namespace SociallyDistant.ContentEditor
{
    public interface IContentEditor
    {
        bool ShowEditor { get; set; }
        string DataDirectory { get; }
        
        void UpdateMenu();

        void Error(string message);

        void UpdateGoodies(AssetInfo info);
        void ExpandGoodieCategory(AssetInfo info);
        void SelectGoodie(IAsset asset);
        void UpdateGoodieLists();
        bool AskForFolder(string title, out string folder);
        void AddCategory(string name);
        void AddEditItem(string category, string name, string desc, IAssetPropertyEditor editor);
        void ClearCategories();

        void SetCustomViewElement(Element element);
    }
}