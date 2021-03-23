using UnityEngine;

public class Bowmen : Unit
{
    protected override void IdleA()
    {
        if (HasSpeed) {
            ChangeState(UnitFSM.Rotate);
            var anim = animations.idleCombat[0];
            if (currentAnim != anim) {
                PlayAnimation(anim, anim.Length);
            }
            nextAnimTime = 0f;
        }
        isRunning = false;
    }

    protected override void Rotate()
    {
        var current = worldTransform.rotation;
        var target = (seekingTarget.worldTransform.position - worldTransform.position).ToRotation();
        if (Mathf.Abs(Quaternion.Dot(current, target)) < 0.999999f) {
            worldTransform.rotation = Quaternion.RotateTowards(current, target, RotationSpeed * 2f);
        } else {
            if (currentTime > nextAnimTime) {
                ChangeState(UnitFSM.RangeReload);
            }
        }
    }
    
    
}
