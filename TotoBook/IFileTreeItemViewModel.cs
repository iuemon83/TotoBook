using System.Collections.Generic;

namespace TotoBook
{
    public interface IFileTreeItemViewModel
    {
        bool IsExpanded { get; set; }
        string Name { get; }
        IFileTreeItemViewModel Parent { get; }
        IEnumerable<IFileTreeItemViewModel> Children { get; }
        bool IsSelected { get; set; }
    }
}
