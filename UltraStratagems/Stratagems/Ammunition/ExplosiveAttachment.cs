using System;
using System.Collections.Generic;
using System.Text;

namespace UltraStratagems.Stratagems.Ammunition;

public class ExplosiveAttachment : MonoBehaviour
{
    public Action<Vector3> onDestroy;
    public void OnDestroy()
    {
        onDestroy.Invoke(this.transform.position);
    }

}
