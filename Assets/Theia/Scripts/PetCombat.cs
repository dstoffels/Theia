public class PetCombat : Combat
{
    public override void OnStartServer()
    {
        // forward events to owner
        onDamageDealtTo.AddListener((victim) => {
            ((Pet)entity).owner.combat.onDamageDealtTo.Invoke(victim);
        });
        onKilledEnemy.AddListener((victim) => {
            ((Pet)entity).owner.combat.onKilledEnemy.Invoke(victim);
        });
    }
}
