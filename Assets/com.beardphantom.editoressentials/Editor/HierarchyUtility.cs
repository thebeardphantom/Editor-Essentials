using System.Text;
using UnityEngine;

namespace EditorEssentials.Editor
{
    public static class HierarchyUtility
    {
        #region Fields

        private static readonly StringBuilder _pathBuilder = new StringBuilder();

        #endregion

        #region Methods

        public static string GetPath(this Component cmp)
        {
            return GetPath(cmp.gameObject);
        }

        private static string GetPath(GameObject gameObject)
        {
            _pathBuilder.Length = 0;
            var obj = gameObject.transform;
            while (obj != null)
            {
                _pathBuilder.Insert(0, $"/{obj.name}");
                obj = obj.parent;
            }

            _pathBuilder.Remove(0, 1);
            var path = _pathBuilder.ToString();
            _pathBuilder.Length = 0;
            return path;
        }

        #endregion
    }
}