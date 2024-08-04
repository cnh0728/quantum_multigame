using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class LocalInput : MonoBehaviour {
    private void Start()
    {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    public void PollInput(CallbackPollInput callback)
    {
        Quantum.Input input = new Quantum.Input();

        // GetButton Down이나 Up쓰지 말고 GetButton 써야함. Quantum이 스스로 계산하기때문
        input.Jump = UnityEngine.Input.GetButton("Jump");

        var x = UnityEngine.Input.GetAxis("Horizontal");
        var y = UnityEngine.Input.GetAxis("Vertical");

        input.Direction = new Vector2 (x, y).ToFPVector2();

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }
}
