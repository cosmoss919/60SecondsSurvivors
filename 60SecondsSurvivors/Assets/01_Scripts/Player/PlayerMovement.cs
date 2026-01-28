using UnityEngine;

namespace _60SecondsSurvivors.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;

        private Rigidbody2D _rigidbody;
        private Vector2 _input;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _input.x = Input.GetAxisRaw("Horizontal");
            _input.y = Input.GetAxisRaw("Vertical");
            _input = _input.normalized;
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = _input * _moveSpeed;
        }
    }
}

