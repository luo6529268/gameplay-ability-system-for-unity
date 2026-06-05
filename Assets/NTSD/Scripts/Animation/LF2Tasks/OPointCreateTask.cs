using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation.LF2Tasks
{
    public class OPointCreateTask : LF2TaskBase, ILF2Recyclable
    {
        public override LF2TaskType TaskType => LF2TaskType.CreateObject;

        public ObjectPoint opoint;
        public LF2Entity parent;
        public int team;
        public Vector3 pos;
        public float z;
        public string dir;
        public float dvz;

        public bool useDirectVelocity;
        public float directVx;
        public float directVy;
        public float directVz;
        public bool preserveActionZero;

        public int ownerEntityIndex = -1;
        public int frameDelay = 0;
        public int attackExempt = 0;
        public bool releaseOpointSpawn;

        public bool IsFromPool { get; set; }
        public void Clear()
        {
            opoint = default; parent = null; team = 0;
            pos = Vector3.zero; z = 0f; dir = null; dvz = 0f;
            useDirectVelocity = false; directVx = 0f; directVy = 0f; directVz = 0f;
            preserveActionZero = false;
            ownerEntityIndex = -1;
            frameDelay = 0;
            attackExempt = 0;
            releaseOpointSpawn = false;
        }
    }
}
