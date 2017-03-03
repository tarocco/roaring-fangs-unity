/*
Adapted from EventSystem improvement by Lyuben Atanasov
http://blog.pastelstudios.com/2015/09/07/unity-tips-tricks-multiple-event-systems-single-scene-unity-5-1/
*/

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BetterEventSystem : EventSystem
{
    protected override void OnEnable()
    {
        // Do not assign EventSystem.current
    }

    protected override void Update()
    {
        if (gameObject.scene == SceneManager.GetActiveScene())
        {
            EventSystem original = current;
            // In order to avoid reimplementing half of the EventSystem class, just
            // temporarily assign this EventSystem to be the globally current one
            current = this;
            base.Update();
            current = original;
        }
    }
}
