public class LongSwordmen : Unit
{
    /* Combing separate knockdown and get up animation */
    protected override void KnockdownStart()
    {
        ChangeState(UnitFSM.Knockdown);
        var anim = squad.data.animations.knockdown.GetRandom(1);
        PlayAnimation(anim, anim.Length);
    }
    
    protected override void Knockdown()
    {
        if (currentTime > nextAnimTime) {
            ChangeState(UnitFSM.Wait);
            var anim = squad.data.animations.knockdown[0];
            PlayAnimation(anim, anim.Length, 1f,  0f, false);
        }
    }
}
