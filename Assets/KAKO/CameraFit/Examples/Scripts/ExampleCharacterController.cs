using UnityEngine;

namespace Kako.CameraFit.Examples
{
    public class ExampleCharacterController : MonoBehaviour
    {

        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;

        private const string RunHash = "Run";
        private static readonly int RunAnim = Animator.StringToHash(RunHash);

        private const string HorizontalHash = "Horizontal";
        private const string VerticalHash = "Vertical";

        private const float Speed = 5f;

        void Update()
        {
            Vector3 moveVector = new Vector3(Input.GetAxis(HorizontalHash), 0, Input.GetAxis(VerticalHash));
            characterController.Move(Time.deltaTime * Speed * moveVector);
            characterController.transform.LookAt(characterController.transform.position + moveVector, Vector3.up);
            animator.SetBool(RunAnim, moveVector.magnitude > 0);
        }
    }
}