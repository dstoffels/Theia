// Entity finite state machines are scriptable. This way it's very easy to
// modify monster/pet/player behaviours without touching core code.
// -> also allows for multiple monster AI types, which is always nice to have.
//
// ScriptableBrains have a completely open architecture. You can use other
// ScriptableObjects for states, you can use other AI assets, visual editors,
// etc. as long as you inherit from ScriptableBrain and overwrite the two Update
// functions!
// -> this is the most simple and most open solution.
using UnityEngine;

public abstract class ScriptableBrain : ScriptableObject
{
    // updates server state machine, returns next state
    public abstract string UpdateServer(EntityOLD entity);

    // updates client state machine
    public abstract void UpdateClient(EntityOLD entity);

    // DrawGizmos can be used to display debug information
    // (can't name it "On"DrawGizmos otherwise Unity complains about parameters)
    public virtual void DrawGizmos(EntityOLD entity) {}
}
