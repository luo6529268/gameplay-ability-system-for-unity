using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMountains.TopDownEngine
{
    public class AssetObject
    {
        private string m_AssetName;
        private object m_Target;

        /// <summary>
        /// 获取对象名称。
        /// </summary>
        public string AssetName
        {
            get
            {
                return m_AssetName;
            }
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        public object Target
        {
            get
            {
                return m_Target;
            }
        }

        public static AssetObject Create(string name, object target) 
        {
            AssetObject assetObject = new AssetObject();
            assetObject.m_AssetName = name;
            assetObject.m_Target = target;

            return assetObject;
        }
    }
}
