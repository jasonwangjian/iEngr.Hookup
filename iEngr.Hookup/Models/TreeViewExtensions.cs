using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace iEngr.Hookup.Models
{
    public static class TreeViewExtensions
    {
        public static void BringItemIntoView(this TreeView treeView, object item)
        {
            if (treeView == null) throw new ArgumentNullException(nameof(treeView));

            InternalBringItemIntoView(treeView, item);
        }

        private static bool InternalBringItemIntoView(ItemsControl parent, object item)
        {
            if (parent == null || item == null) return false;

            // 尝试直接找到容器
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                treeViewItem.BringIntoView();
                //treeViewItem.IsSelected = true;
                return true;
            }

            // 递归查找子项
            foreach (object childItem in parent.Items)
            {
                var childContainer = parent.ItemContainerGenerator.ContainerFromItem(childItem) as ItemsControl;
                if (childContainer != null && InternalBringItemIntoView(childContainer, item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
