using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation.LF2Tasks
{
    public class OPointCreateMultipleTask : LF2TaskBase, ILF2Recyclable
    {
        public override LF2TaskType TaskType => LF2TaskType.CreateMultipleObjects;

        public ObjectPoint opoint;
        public LF2Entity parent;
        public int team;
        public Vector3 pos;
        public float z;
        public string dir;
        public float dvz;
        public int number;

        public bool IsFromPool { get; set; }
        public void Clear()
        {
            opoint = default; parent = null; team = 0;
            pos = Vector3.zero; z = 0f; dir = null; dvz = 0f; number = 0;
        }
    }
}
