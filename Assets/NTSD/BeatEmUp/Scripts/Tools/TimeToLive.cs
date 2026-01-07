using UnityEngine;
using System.Collections;

namespace BeatEmUpTemplate2D
{

    /**
     * TimeToLive类：用于在指定时间后销毁游戏对象
     * 该组件可以附加到任何游戏对象上，使其在预设的时间后自动销毁
     */
    public class TimeToLive : MonoBehaviour
    {

        // 生存时间（秒），默认为1秒
        // 可以在Unity编辑器中调整这个值
        public float timeToLive = 1f;

        /**
         * Start方法：在对象创建时启动协程
         * 使用协程来实现延时销毁功能
         */
        IEnumerator Start()
        {
            // 等待指定的时间（timeToLive秒）
            yield return new WaitForSeconds(timeToLive);
            // 时间到达后销毁当前游戏对象
            Destroy(gameObject);
        }
    }

}
