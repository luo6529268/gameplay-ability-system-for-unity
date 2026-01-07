using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.TopDownEngine;

public class RebindingManager : MonoBehaviour
{
    public Character Character;
    public int bindingIndex = 0; // 指定要覆盖的绑定索引
    public Key Key = Key.None;

    private bool bModification;
    private Key LastKey;
    private void Update()
    {
        RebindKey(Key);
    }

    // 核心方法：重新绑定按键
    public void RebindKey(Key newKey)
    {
        if (newKey == Key.None)
            return;

        if (LastKey == newKey)
            return;

        if (bModification) return;

        // 创建一个新的绑定对象，指定新的路径
        InputBinding newBinding = new InputBinding
        {
            overridePath = $"<Keyboard>/{newKey.ToString().ToLower()}" // 构造新的路径，例如"<Keyboard>/
        };

        Debug.LogErrorFormat("overridePath   : {0}", newBinding.overridePath);
        // 应用覆盖绑定到指定的绑定索引
        if (Character != null && Character._CharacterInput != null && Character._CharacterInput.AttackAction != null)
        {
            Character._CharacterInput.AttackAction.ApplyBindingOverride(bindingIndex, newBinding);
        }

        bModification = true;
        LastKey = newKey;
    }
}
