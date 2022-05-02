using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float shiftSpeed;
    [SerializeField] private float sprintSpeed;
    [Header("Stamina")]
    [SerializeField] private float maxStamina;
    [SerializeField] private float minStamina;
    [SerializeField] private float staminaConsumption;
    [SerializeField] private float staminaRegeneration;
    [SerializeField] private Image staminaBar;

    private Rigidbody2D _rigidBody;
    private float _stamina;

    private void Start()
    {
        _stamina = maxStamina;
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var sprinted = false;
        
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Move(shiftSpeed);
        }
        else
        {
            if (_stamina >= minStamina && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                _stamina -= staminaConsumption * Time.deltaTime;
                Move(sprintSpeed);
                sprinted = true;
            }
            else
            {
                Move(movementSpeed);
            }
        }

        if (!sprinted)
        {
            _stamina = Mathf.Clamp(_stamina + staminaRegeneration * Time.deltaTime, minStamina, maxStamina);
        }

        staminaBar.fillAmount = Mathf.Clamp(_stamina / maxStamina, 0f, 1f);
    }

    private void Move(float speed)
    {
        var horizontal = Input.GetAxis("Horizontal") * speed;
        var vertical = Input.GetAxis("Vertical") * speed;
        _rigidBody.velocity = new Vector2(horizontal, vertical);
    }
}